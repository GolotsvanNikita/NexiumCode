using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course> GetCourseWithLessons(int courseId);
    }
}
