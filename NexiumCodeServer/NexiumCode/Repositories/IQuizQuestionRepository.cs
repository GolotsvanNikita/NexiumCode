using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface IQuizQuestionRepository : IRepository<QuizQuestion>
    {
        Task<IEnumerable<QuizQuestion>> GetQuizQuestionsByLesson(int lessonId);
        Task<Lesson> GetLesson(int lessonId);
    }
}
