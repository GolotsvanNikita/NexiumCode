using NexiumCode.Controllers;

namespace NexiumCode.JSON
{
    public class LessonJson
    {
        public int Id { get; set; }
        public int CourseId { get; set; } = 1;
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsTheory { get; set; }
        public int Order { get; set; }
        public List<QuizQuestionJson> QuizQuestions { get; set; }
        public List<PracticeTaskJson> PracticeTasks { get; set; }
    }
}
