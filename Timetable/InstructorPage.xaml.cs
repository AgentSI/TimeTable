using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Timetable.Models;

namespace Timetable
{
    public partial class InstructorPage : UserControl
    {
        private const string InstructorsFileName = "instructors.json";
        private List<Instructor> instructors = [];
        public Instructor? SelectedInstructor { get; set; }

        public InstructorPage()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadInstructors();
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
                        instructors = loadedInstructors;
                        InstructorsDataGrid.ItemsSource = instructors;
                    }
                }
                catch (JsonException ex) 
                {
                    MessageBox.Show($"Error reading instructors data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error); 
                }
                catch (IOException ex) 
                {
                    MessageBox.Show($"Error accessing file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error); 
                }
            }
        }

        private void InstructorSelect(object sender, SelectionChangedEventArgs e)
        {
            SelectedInstructor = InstructorsDataGrid.SelectedItem as Instructor;
            if (SelectedInstructor != null)
            {
                InstructorNameEditTextBox.Text = SelectedInstructor.Name;
            }
            else
            {
                InstructorNameEditTextBox.Clear();
            }
        }

        private void AddInstructor(object sender, RoutedEventArgs e)
        {
            string instructorName = InstructorNameAddTextBox.Text.Trim();

            if (string.IsNullOrEmpty(instructorName))
            {
                MessageBox.Show("Please enter Instructor Name.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (instructors.Any(i => i.Name.Equals(instructorName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Instructor with Name '{instructorName}' already exists. Please use a unique Name.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Instructor newInstructor = new Instructor(instructorName);
            instructors.Add(newInstructor);
            SaveInstructors();

            MessageBox.Show($"Instructor Added\nName: {instructorName}", "Instructor Added", MessageBoxButton.OK, MessageBoxImage.Information);

            InstructorNameAddTextBox.Clear();
        }

        private void SaveInstructors()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(instructors, options);
                File.WriteAllText(InstructorsFileName, jsonString);
                LoadInstructors();
            }
            catch (JsonException ex) 
            {
                MessageBox.Show($"Error writing instructors data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateInstructor(object sender, RoutedEventArgs e)
        {
            if (SelectedInstructor == null)
            {
                MessageBox.Show("Please select an instructor to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string updatedName = InstructorNameEditTextBox.Text.Trim();
            if (string.IsNullOrEmpty(updatedName))
            {
                MessageBox.Show("Instructor Name cannot be empty.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var instructorToUpdate = instructors.FirstOrDefault(i => i.Name == SelectedInstructor.Name);
            if (instructorToUpdate != null)
            {
                if (instructors.Any(i => i.Name.Equals(updatedName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"Instructor with Name '{updatedName}' already exists. Please use a unique Name.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                instructorToUpdate.Name = updatedName;
            }

            SaveInstructors();

            MessageBox.Show($"Instructor updated to: {updatedName}", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            InstructorsDataGrid.SelectedItem = null;
        }

        private void DeleteInstructor(object sender, RoutedEventArgs e)
        {
            if (SelectedInstructor == null)
            {
                MessageBox.Show("Please select an instructor to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete instructor '{SelectedInstructor.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                instructors.Remove(SelectedInstructor);
                MessageBox.Show($"Instructor '{SelectedInstructor.Name}' deleted.", "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                SaveInstructors();
                InstructorsDataGrid.SelectedItem = null;
            }
        }
    }
}
