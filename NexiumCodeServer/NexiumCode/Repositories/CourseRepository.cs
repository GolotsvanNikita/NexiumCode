using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context) { }

        public async Task<Course> GetCourseWithLessons(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }
    }
}
