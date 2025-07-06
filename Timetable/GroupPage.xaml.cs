using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Timetable.Models;
using Group = Timetable.Models.Group;

namespace Timetable
{
    public partial class GroupPage : UserControl
    {
        private const string GroupsFileName = "groups.json";
        private const string CoursesFileName = "courses.json";

        private List<Group> groups = [];
        private List<Course> allAvailableCourses = [];

        public Group? SelectedGroup { get; set; }
        public List<Course> AllAvailableCoursesForAdd = [];

        public GroupPage()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadAllCourses();
            LoadGroups();
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
                        groups = loadedGroups;
                        GroupsDataGrid.ItemsSource = groups;
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error reading groups data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error accessing file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveGroups()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(groups.ToList(), options);
                File.WriteAllText(GroupsFileName, jsonString);
                LoadGroups();
                SubmitButton.Visibility = Visibility.Visible;
                UpdateButton.Visibility = Visibility.Hidden;
                DeleteButton.Visibility = Visibility.Hidden;
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error writing Groups data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAllCourses()
        {
            if (File.Exists(CoursesFileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(CoursesFileName);
                    List<Course>? loadedCourses = JsonSerializer.Deserialize<List<Course>>(jsonString);
                    if (loadedCourses != null)
                    {
                        allAvailableCourses = loadedCourses;
                        AllAvailableCoursesForAdd = allAvailableCourses;
                        CoursesListBox.ItemsSource = AllAvailableCoursesForAdd;
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error reading courses data for group page: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error accessing courses file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddGroup(object sender, RoutedEventArgs e)
        {
            string groupName = GroupNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("Please enter the Group Name.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (groups.Any(d => d.Name.Equals(groupName, System.StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Group with Name '{groupName}' already exists. Please use a unique Name.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<Course> selectedCourseNames = AllAvailableCoursesForAdd
                                                .Where(c => c.IsSelected)
                                                .ToList();

            Group newGroup = new Group(groupName, selectedCourseNames);
            groups.Add(newGroup);
            SaveGroups();

            MessageBox.Show($"Group Added\nName: {groupName}", "Group Added", MessageBoxButton.OK, MessageBoxImage.Information);

            GroupNameTextBox.Clear();
            ClearCourseSelection();
        }

        private void GroupSelect(object sender, SelectionChangedEventArgs e)
        {
            SubmitButton.Visibility = Visibility.Hidden;
            UpdateButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            SelectedGroup = GroupsDataGrid.SelectedItem as Group;
            if (SelectedGroup != null)
            {
                GroupNameTextBox.Text = SelectedGroup.Name;
                UpdateCoursesListBoxSelection();
            }
            else
            {
                GroupNameTextBox.Clear();
                ClearCourseSelection();
            }
        }

        private void UpdateGroup(object sender, RoutedEventArgs e)
        {
            if (SelectedGroup == null)
            {
                MessageBox.Show("Please select a group to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string updatedGroupName = GroupNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(updatedGroupName))
            {
                MessageBox.Show("Group Name cannot be empty.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (groups.Any(d => d.Name.Equals(updatedGroupName, System.StringComparison.OrdinalIgnoreCase) && d != SelectedGroup))
            {
                MessageBox.Show($"Group with Name '{updatedGroupName}' already exists. Please use a unique Name.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<Course> updatedSelectedCourseNames = AllAvailableCoursesForAdd
                                                        .Where(c => c.IsSelected)
                                                        .ToList();

            SelectedGroup.Name = updatedGroupName;
            SelectedGroup.Courses = updatedSelectedCourseNames;

            MessageBox.Show($"Group updated to\nName: {updatedGroupName}\nCourses Count: {SelectedGroup.Courses.Count}", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            SaveGroups();
        }

        private void DeleteGroup(object sender, RoutedEventArgs e)
        {
            if (SelectedGroup == null)
            {
                MessageBox.Show("Please select a group to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete group '{SelectedGroup.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                groups.Remove(SelectedGroup);
                MessageBox.Show($"Group '{SelectedGroup.Name}' deleted.", "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                SaveGroups();
                GroupsDataGrid.SelectedItem = null;
                ClearCourseSelection();
            }
        }

        private void UpdateCoursesListBoxSelection()
        {
            if (AllAvailableCoursesForAdd != null && SelectedGroup != null)
            {
                foreach (var course in AllAvailableCoursesForAdd)
                {
                    course.IsSelected = SelectedGroup.Courses!.Any(c => c.Name == course.Name);
                }
            }
            else
            {
                ClearCourseSelection();
            }
        }

        private void ClearCourseSelection()
        {
            if (AllAvailableCoursesForAdd != null)
            {
                foreach (var course in AllAvailableCoursesForAdd)
                {
                    course.IsSelected = false;
                }
            }
        }
    }
}