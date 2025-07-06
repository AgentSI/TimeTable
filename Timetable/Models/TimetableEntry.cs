namespace Timetable.Models
{
    public class TimetableEntry(string time, DateTime day, string courseName, string room, string instructorName)
    {
        public string Time { get; set; } = time;
        public DateTime Day { get; set; } = day;
        public string CourseName { get; set; } = courseName;
        public string InstructorName { get; set; } = instructorName;
        public string Room { get; set; } = room;
    }
}
