using NexiumCode.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexiumCode.Repositories
{
    public interface IPracticeTaskRepository : IRepository<PracticeTask>
    {
        Task<PracticeTask> GetTaskWithTests(int taskId);
        Task<Lesson> GetLesson(int lessonId);
        Task<IEnumerable<PracticeTask>> GetTasksByCourse(int courseId);
    }
}