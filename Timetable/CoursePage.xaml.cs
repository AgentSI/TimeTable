using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Timetable.Models;

namespace Timetable
{
    public partial class CoursePage : UserControl
    {
        private const string CoursesFileName = "courses.json";
        private const string InstructorsFileName = "instructors.json";

        private List<Course> courses = [];
        private List<Instructor> availableInstructors = [];

        public Course? SelectedCourse { get; set; }

        public CoursePage()
        {
            InitializeComponent();
            this.DataContext = this;

            LoadInstructors();
            LoadCourses();
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
                        courses = loadedCourses;
                        CoursesDataGrid.ItemsSource = courses;
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

        private void LoadInstructors()
        {
            if (File.Exists(InstructorsFileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(InstructorsFileName);
                    List<Instructor>? loadedInstructors = JsonSerializer.Deserialize<List<Instructor>>(jsonString);
                    if (loadedInstructors != null)
                    {
                        availableInstructors = loadedInstructors;
                        InstructorsAddComboBox.ItemsSource = availableInstructors;
                        InstructorsEditComboBox.ItemsSource = availableInstructors;
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error reading instructors data for course page: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error accessing instructors file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveCourses()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(courses.ToList(), options);
                File.WriteAllText(CoursesFileName, jsonString);
                LoadCourses();
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error writing courses data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCourse(object sender, RoutedEventArgs e)
        {
            string courseName = CourseNameAddTextBox.Text.Trim();
            Instructor? selectedInstructor = InstructorsAddComboBox.SelectedItem as Instructor;
            string? selectedInstructorName = selectedInstructor?.Name;

            if (string.IsNullOrEmpty(courseName))
            {
                MessageBox.Show("Please enter the Course Name.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedInstructorName))
            {
                MessageBox.Show("Please select one instructor.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (courses.Any(c => c.Name.Equals(courseName, System.StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Course with Name '{courseName}' already exists. Please use a unique Name.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Course newCourse = new Course(courseName, selectedInstructorName);
            courses.Add(newCourse);
            SaveCourses();

            MessageBox.Show($"Course Added\nName: {courseName}\nInstructor: {selectedInstructorName}", "Course Added", MessageBoxButton.OK, MessageBoxImage.Information);

            CourseNameAddTextBox.Clear();
            InstructorsAddComboBox.SelectedItem = null;
        }

        private void CourseSelect(object sender, SelectionChangedEventArgs e)
        {
            SelectedCourse = CoursesDataGrid.SelectedItem as Course;
            if (SelectedCourse != null)
            {
                CourseNameEditTextBox.Text = SelectedCourse.Name;
                InstructorsEditComboBox.SelectedItem = null;

                var instructorToSelect = availableInstructors.FirstOrDefault(i => i.Name == SelectedCourse.InstructorName);
                if (instructorToSelect != null)
                {
                    InstructorsEditComboBox.SelectedItem = instructorToSelect;
                }
            }
            else
            {
                CourseNameEditTextBox.Clear();
                InstructorsEditComboBox.SelectedItem = null;
            }
        }

        private void UpdateCourse(object sender, RoutedEventArgs e)
        {
            if (SelectedCourse == null)
            {
                MessageBox.Show("Please select a course to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string updatedCourseName = CourseNameEditTextBox.Text.Trim();
            Instructor? updatedSelectedInstructor = InstructorsEditComboBox.SelectedItem as Instructor;
            string? updatedInstructorName = updatedSelectedInstructor?.Name;

            if (string.IsNullOrEmpty(updatedCourseName))
            {
                MessageBox.Show("Course Name cannot be empty.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(updatedInstructorName))
            {
                MessageBox.Show("Please select one instructor.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (courses.Any(c => c.Name.Equals(updatedCourseName, System.StringComparison.OrdinalIgnoreCase) && c != SelectedCourse))
            {
                MessageBox.Show($"Course with Name '{updatedCourseName}' already exists. Please use a unique Name.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedCourse.Name = updatedCourseName;
            SelectedCourse.InstructorName = updatedInstructorName;

            SaveCourses();

            MessageBox.Show($"Course updated to\nName: {updatedCourseName}\nInstructor: {updatedInstructorName}", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            CoursesDataGrid.SelectedItem = null;
        }

        private void DeleteCourse(object sender, RoutedEventArgs e)
        {
            if (SelectedCourse == null)
            {
                MessageBox.Show("Please select a course to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete course '{SelectedCourse.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                courses.Remove(SelectedCourse);
                SaveCourses();

                MessageBox.Show($"Course '{SelectedCourse.Name}' deleted.", "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                CoursesDataGrid.SelectedItem = null;
            }
        }
    }
}
