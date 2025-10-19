using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface IProgressRepository : IRepository<Progress>
    {
        Task<Progress> GetProgressByUserAndCourse(int userId, int courseId);

        Task UpdateTheoryProgress(int userId, int courseId, int progress);

        Task UpdatePracticeProgress(int userId, int courseId, int progress);
        Task UpdateLessonProgress(int userId, int courseId, int lessonId, int progress);
    }
}
