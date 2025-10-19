namespace NexiumCode.Models
{
    public class ProgressLesson
    {
        public int Id { get; set; }
        public int ProgressId { get; set; }
        public int LessonId { get; set; }
        public int ProgressValue { get; set; }
        public Progress Progress { get; set; }
    }
}
