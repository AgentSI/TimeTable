using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Timetable.Models;

namespace Timetable
{
    public partial class RoomPage : UserControl
    {
        private const string RoomsFileName = "rooms.json";
        private List<Room> rooms = [];
        public Room? SelectedRoom { get; set; }

        public RoomPage()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadRooms();
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
                        RoomsDataGrid.ItemsSource = rooms;
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

        private void AddRoom(object sender, RoutedEventArgs e)
        {
            string roomNumber = RoomNumberAddTextBox.Text.Trim();

            if (string.IsNullOrEmpty(roomNumber))
            {
                MessageBox.Show("Please enter the Room Number.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (rooms.Any(r => r.RoomNumber.Equals(roomNumber, System.StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Room with number '{roomNumber}' already exists. Please use a unique Room Number.", "Duplicate Room", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Room newRoom = new Room(roomNumber);
            rooms.Add(newRoom);
            SaveRooms();

            MessageBox.Show($"Room Added\nNumber: {roomNumber}", "Room Added", MessageBoxButton.OK, MessageBoxImage.Information);

            RoomNumberAddTextBox.Clear();
            RoomNumberAddTextBox.Focus();
        }

        private void SaveRooms()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(rooms.ToList(), options);
                File.WriteAllText(RoomsFileName, jsonString);
                LoadRooms();
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error writing rooms data: {ex.Message}", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RoomSelect(object sender, SelectionChangedEventArgs e)
        {
            SelectedRoom = RoomsDataGrid.SelectedItem as Room;
            if (SelectedRoom != null)
            {
                RoomNumberEditTextBox.Text = SelectedRoom.RoomNumber;
            }
            else
            {
                RoomNumberEditTextBox.Clear();
            }
        }

        private void UpdateRoom(object sender, RoutedEventArgs e)
        {
            if (SelectedRoom == null)
            {
                MessageBox.Show("Please select a room to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string updatedRoomNumber = RoomNumberEditTextBox.Text.Trim();

            if (string.IsNullOrEmpty(updatedRoomNumber))
            {
                MessageBox.Show("Room Number cannot be empty.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (rooms.Any(r => r.RoomNumber.Equals(updatedRoomNumber, System.StringComparison.OrdinalIgnoreCase) && r != SelectedRoom))
            {
                MessageBox.Show($"Room with number '{updatedRoomNumber}' already exists. Please use a unique Room Number.", "Duplicate Room", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedRoom.RoomNumber = updatedRoomNumber;
            SaveRooms();

            MessageBox.Show($"Room updated to: {updatedRoomNumber}", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            RoomsDataGrid.SelectedItem = null;
        }

        private void DeleteRoom(object sender, RoutedEventArgs e)
        {
            if (SelectedRoom == null)
            {
                MessageBox.Show("Please select a room to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete room '{SelectedRoom.RoomNumber}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                rooms.Remove(SelectedRoom);
                MessageBox.Show($"Room '{SelectedRoom.RoomNumber}' deleted.", "Delete Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                SaveRooms();
                RoomsDataGrid.SelectedItem = null;
            }
        }
    }
}
