using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<IEnumerable<Lesson>> GetLessonsByCourse(int courseId);

        Task<Lesson> GetLessonWithDetails(int lessonId);
    }
}
