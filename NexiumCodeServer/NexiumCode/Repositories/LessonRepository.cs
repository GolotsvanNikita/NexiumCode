using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public class LessonRepository : GenericRepository<Lesson>, ILessonRepository
    {
        public LessonRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Lesson>> GetLessonsByCourse(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<Lesson> GetLessonWithDetails(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.QuizQuestions)
                .Include(l => l.PracticeTasks)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }
    }
}
