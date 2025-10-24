using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public class PracticeTaskRepository : GenericRepository<PracticeTask>, IPracticeTaskRepository
    {
        public PracticeTaskRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<PracticeTask>> GetTasksByCourse(int courseId)
        {
            return await _context.PracticeTasks
                .Include(t => t.Lesson)
                .Where(t => t.Lesson.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<PracticeTask> GetTaskWithTests(int taskId)
        {
            return await _context.PracticeTasks.FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task<Lesson> GetLesson(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }
    }
}