using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Timetable.Models;

namespace Timetable
{
    public partial class MeetingPage : UserControl
    {
        private const string MeetingsFileName = "meetings.json";
        private const string CoursesFileName = "courses.json";
        private const string RoomsFileName = "rooms.json";

        private List<Meeting> meetings = [];
        private List<Course> availableCourses = [];
        private List<Room> rooms = [];

        public List<string> AvailableTimes = [];

        public Meeting? SelectedMeeting { get; set; }

        public MeetingPage()
        {
            InitializeComponent();
            this.DataContext = this;

            InitializeComboBoxData();
            LoadCourses();      
            LoadRooms();
            LoadMeetings();
        }

        private void InitializeComboBoxData()
        {
            AvailableTimes = new List<string>
            {
                "8:00 - 9:30",
                "9:45 - 11:15",
                "11:30 - 13:00",
                "13:30 - 15:00",
                "15:15 - 16:45",
                "17:00 - 18:30",
                "18:45 - 20:15",
                "17:30 - 19:00",
                "19:10 - 20:40"
            };

            TimeComboBox.ItemsSource = AvailableTimes;
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
                        availableCourses = loadedCourses;
                        CourseComboBox.ItemsSource = availableCourses;
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error reading courses data for meeting page: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error accessing courses file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadMeetings()
        {
            if (File.Exists(MeetingsFileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(MeetingsFileName);
                    List<Meeting>? loadedMeetings = JsonSerializer.Deserialize<List<Meeting>>(jsonString);
                    if (loadedMeetings != null)
                    {
                        meetings = loadedMeetings;
                        MeetingsDataGrid.ItemsSource = meetings;
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error reading meetings data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error accessing meetings file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadRooms()
        {
            if (File.Exists(RoomsFileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(RoomsFileName);
                    List<Room>? loadedRooms = JsonSerializer.Deserialize<List<Room>>(jsonString);
                    if (loadedRooms != null)
                    {
                        rooms = loadedRooms;
                        RoomComboBox.ItemsSource = rooms;
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error reading rooms data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error accessing file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddMeeting(object sender, RoutedEventArgs e)
        {
            Course? selectedCourse = CourseComboBox.SelectedItem as Course;
            string? courseName = selectedCourse?.Name;
            Room? selectedRoom = RoomComboBox.SelectedItem as Room;
            string? roomNr = selectedRoom?.RoomNumber;
            string? selectedTime = TimeComboBox.SelectedItem?.ToString();
            DateTime? selectedDate = DayDatePicker.SelectedDate;

            if (string.IsNullOrEmpty(courseName))
            {
                MessageBox.Show("Please select a Course.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedTime))
            {
                MessageBox.Show("Please select a valid Time.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedDate == null)
            {
                MessageBox.Show("Please select Day.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(roomNr))
            {
                MessageBox.Show("Please select a valid Room.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (meetings.Any(m => m.CourseName.Equals(courseName, System.StringComparison.OrdinalIgnoreCase) &&
                                   m.Time.Equals(selectedTime) &&
                                   m.Day.Equals(selectedDate)))
            {
                MessageBox.Show($"A meeting for '{courseName}' at {selectedTime} on {selectedDate} already exists.", "Duplicate Meeting", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (meetings.Any(m => m.Day.Equals(selectedDate) && m.Time.Equals(selectedTime) && m != SelectedMeeting && m.RoomNumber.Equals(roomNr)))
            {
                MessageBox.Show($"The room {roomNr} for time {selectedTime} on {selectedDate} is already occupied.", "Room Conflict", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Meeting newMeeting = new Meeting(courseName, selectedTime, selectedDate.Value, roomNr);
            meetings.Add(newMeeting);
            SaveMeetings();

            MessageBox.Show($"Meeting Added\nCourse: {courseName}\nRoom: {roomNr}\nTime: {selectedTime}\nDay: {selectedDate.Value.ToString("dd.MM.yyyy")}", "Meeting Added", MessageBoxButton.OK, MessageBoxImage.Information);

            CourseComboBox.SelectedItem = null;
            RoomComboBox.SelectedItem = null;
            TimeComboBox.SelectedItem = null;
            DayDatePicker.SelectedDate = null;
        }

        private void SaveMeetings()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(meetings.ToList(), options);
                File.WriteAllText(MeetingsFileName, jsonString);
                LoadMeetings();
                SubmitButton.Visibility = Visibility.Visible;
                UpdateButton.Visibility = Visibility.Hidden;
                DeleteButton.Visibility = Visibility.Hidden;
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error writing meetings data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MeetingSelect(object sender, SelectionChangedEventArgs e)
        {
            SubmitButton.Visibility = Visibility.Hidden;
            UpdateButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            SelectedMeeting = MeetingsDataGrid.SelectedItem as Meeting;
            if (SelectedMeeting != null)
            {
                CourseComboBox.SelectedItem = availableCourses.FirstOrDefault(c => c.Name == SelectedMeeting.CourseName);
                RoomComboBox.SelectedItem = rooms.FirstOrDefault(r => r.RoomNumber == SelectedMeeting.RoomNumber);
                TimeComboBox.SelectedItem = SelectedMeeting.Time;
                DayDatePicker.SelectedDate = SelectedMeeting.Day;
            }
            else
            {
                CourseComboBox.SelectedItem = null;
                RoomComboBox.SelectedItem = null;
                TimeComboBox.SelectedItem = null;
                DayDatePicker.SelectedDate = null;
            }
        }

        private void UpdateMeeting(object sender, RoutedEventArgs e)
        {
            if (SelectedMeeting == null)
            {
                MessageBox.Show("Please select a meeting to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Course? updatedSelectedCourse = CourseComboBox.SelectedItem as Course;
            string? updatedCourseName = updatedSelectedCourse?.Name;
            Room? updatedSelectedRoom = RoomComboBox.SelectedItem as Room;
            string? updatedRoom = updatedSelectedRoom?.RoomNumber;
            string? updatedTime = TimeComboBox.SelectedItem?.ToString();
            DateTime? updatedDay = DayDatePicker.SelectedDate;

            if (string.IsNullOrEmpty(updatedCourseName))
            {
                MessageBox.Show("Please select a Course.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(updatedTime))
            {
                MessageBox.Show("Please select a Time.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (updatedDay == null)
            {
                MessageBox.Show("Please select a Day.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(updatedRoom))
            {
                MessageBox.Show("Please select a valid Room.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (meetings.Any(m => m.CourseName.Equals(updatedCourseName, System.StringComparison.OrdinalIgnoreCase) &&
                                   m.Time.Equals(updatedTime) &&
                                   m.Day.Equals(updatedDay) &&
                                   m != SelectedMeeting))
            {
                MessageBox.Show($"A meeting for '{updatedCourseName}' at {updatedTime} on {updatedDay} already exists.", "Duplicate Meeting", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (meetings.Any(m => m.Day.Equals(updatedDay) && m.Time.Equals(updatedTime) && m != SelectedMeeting && m.RoomNumber.Equals(updatedRoom)))
            {
                MessageBox.Show($"The room {updatedRoom} for time {updatedTime} on {updatedDay} is already occupied.", "Room Conflict", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedMeeting.CourseName = updatedCourseName;
            SelectedMeeting.RoomNumber = updatedRoom;
            SelectedMeeting.Time = updatedTime;
            SelectedMeeting.Day = updatedDay.Value;
            MessageBox.Show($"Meeting updated to\nCourse: {updatedCourseName}\nTime: {updatedTime}\nDay: {updatedDay.Value.ToString("dd.MM.yyyy")}", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);

            SaveMeetings();
            MeetingsDataGrid.SelectedItem = null;
        }

        private void DeleteMeeting(object sender, RoutedEventArgs e)
        {
            if (SelectedMeeting == null)
            {
                MessageBox.Show("Please select a meeting to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the meeting for '{SelectedMeeting.CourseName}' at {SelectedMeeting.Time} on {SelectedMeeting.Day}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                meetings.Remove(SelectedMeeting);
                MessageBox.Show($"Meeting for '{SelectedMeeting.CourseName}' deleted.", "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                SaveMeetings();
                MeetingsDataGrid.SelectedItem = null;
            }
        }
    }
}
