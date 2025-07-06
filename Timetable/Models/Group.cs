namespace Timetable.Models
{
    public class Group(string name, List<Course>? courses)
    {
        public string Name { get; set; } = name;
        public List<Course>? Courses { get; set; } = courses;
        public string CoursesDisplay => string.Join(", ", Courses?.Select(c => c.Name) ?? Enumerable.Empty<string>());

        public override string ToString()
        {
            return Name;
        }
    }
}