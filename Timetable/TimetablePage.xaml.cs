using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Timetable.Models;

namespace Timetable
{
    public partial class TimetablePage : UserControl, INotifyPropertyChanged
    {
        private const string GroupsFileName = "groups.json";
        private const string MeetingsFileName = "meetings.json";
        private const string CoursesFileName = "courses.json";

        private List<Group> allGroups = [];
        private List<Meeting> allMeetings = [];
        private List<Course> allCourses = [];

        public ObservableCollection<TimetableEntry> MeetingsForDisplay { get; set; } = new ObservableCollection<TimetableEntry>() { };

        private Group? _selectedGroup;
        public Group? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    OnPropertyChanged();
                    FilterAndDisplayMeetings();
                }
            }
        }


        private DateTime _currentDisplayDate;
        public DateTime CurrentDisplayDate
        {
            get => _currentDisplayDate;
            set
            {
                if (_currentDisplayDate != value)
                {
                    _currentDisplayDate = value;
                    OnPropertyChanged();
                    FilterAndDisplayMeetings();
                }
            }
        }

        private bool _hasMeetings;
        public bool HasMeetings
        {
            get => _hasMeetings;
            set
            {
                if (_hasMeetings != value)
                {
                    _hasMeetings = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TimetablePage()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadMeetings();
            LoadCourses();
            LoadGroups();
            FilterAndDisplayMeetings();
            _currentDisplayDate = DateTime.Today;
        }

        private void LoadGroups()
        {
            if (File.Exists(GroupsFileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(GroupsFileName);
                    List<Group>? loadedGroups = JsonSerializer.Deserialize<List<Group>>(jsonString);
                    if (loadedGroups != null)
                    {
                        var allGroupEntry = new Group("Toate", allCourses);
                        loadedGroups.Insert(0, allGroupEntry);
                        allGroups = loadedGroups;
                        if (allGroups.Any())
                        {
                            SelectedGroup = allGroupEntry;
                        }
                        GroupComboBox.ItemsSource = allGroups;
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Eroare la citirea datelor grupelor: {ex.Message}", "Eroare JSON", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Eroare la accesarea fișierului {GroupsFileName}: {ex.Message}", "Eroare Fișier", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"Nu există grupuri create", "Avertisment", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool LoadMeetings()
        {
            if (File.Exists(MeetingsFileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(MeetingsFileName);
                    List<Meeting>? loadedMeetings = JsonSerializer.Deserialize<List<Meeting>>(jsonString);
                    if (loadedMeetings != null)
                    {
                        allMeetings = loadedMeetings;
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Eroare la citirea datelor întâlnirilor: {ex.Message}\nAsigură-te că fișierul '{MeetingsFileName}' are formatul corect.", "Eroare JSON", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Eroare la accesarea fișierului {MeetingsFileName}: {ex.Message}", "Eroare Fișier", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }

        private void LoadCourses()
        {
            if (File.Exists(CoursesFileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(CoursesFileName);
                    List<Course>? loadedCourses = JsonSerializer.Deserialize<List<Course>>(jsonString);
                    if (loadedCourses != null)
                    {
                        allCourses = loadedCourses;
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error reading courses data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error accessing file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FilterAndDisplayMeetings()
        {
            if (allMeetings.Count != 0)
            {
                List<Course> coursesToFilterBy = [];
                if (SelectedGroup != null)
                {
                    if (SelectedGroup.Name == "Toate")
                    {
                        coursesToFilterBy = allCourses;
                    }
                    else
                    {
                        if (SelectedGroup.Courses != null)
                        {
                            coursesToFilterBy = SelectedGroup.Courses;
                        }
                    }
                }

                MeetingsForDisplay.Clear();

                List<Meeting> meetingsForSelectedCourses = allMeetings
                                .Where(m => coursesToFilterBy.Any(c => c.Name == m.CourseName) &&
                                        m.Day.Date == CurrentDisplayDate.Date)
                                .OrderBy(m => m.Time)
                                .ToList();

                foreach (var meeting in meetingsForSelectedCourses)
                {
                    Course? course = allCourses.FirstOrDefault(c => c.Name == meeting.CourseName);
                    var timetableEntry = new TimetableEntry(meeting.Time, meeting.Day, meeting.CourseName, meeting.RoomNumber, course!.InstructorName);
                    MeetingsForDisplay!.Add(timetableEntry);
                }

                HasMeetings = MeetingsForDisplay!.Any();
            }
        }

        private void Today(object sender, RoutedEventArgs e)
        {
            CurrentDisplayDate = DateTime.Today;
        }

        private void PreviousDay(object sender, RoutedEventArgs e)
        {
            CurrentDisplayDate = CurrentDisplayDate.AddDays(-1);
        }

        private void NextDay(object sender, RoutedEventArgs e)
        {
            CurrentDisplayDate = CurrentDisplayDate.AddDays(1);
        }
    }
}
