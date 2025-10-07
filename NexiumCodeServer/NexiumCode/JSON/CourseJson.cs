using NexiumCode.Controllers;

namespace NexiumCode.JSON
{
    public class CourseJson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<LessonJson> Lessons { get; set; }
    }
}
