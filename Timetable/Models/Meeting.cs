namespace Timetable.Models
{
    public class Meeting(string courseName, string time, DateTime day, string roomNumber)
    {
        public string CourseName { get; set; } = courseName;
        public string Time { get; set; } = time;
        public DateTime Day { get; set; } = day;
        public string RoomNumber { get; set; } = roomNumber;
    }
}