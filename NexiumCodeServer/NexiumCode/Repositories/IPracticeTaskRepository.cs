using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface IPracticeTaskRepository : IRepository<PracticeTask>
    {
        Task<PracticeTask> GetTaskWithTests(int taskId);
        Task<Lesson> GetLesson(int lessonId);
    }
}
