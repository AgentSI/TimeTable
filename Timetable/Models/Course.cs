using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Timetable.Models
{
    public class Course(string name, string instructorName) : INotifyPropertyChanged
    {
        public string Name { get; set; } = name;
        public string InstructorName { get; set; } = instructorName;
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}