using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public class QuizQuestionRepository : GenericRepository<QuizQuestion>, IQuizQuestionRepository
    {
        public QuizQuestionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<QuizQuestion>> GetQuizQuestionsByLesson(int lessonId)
        {
            return await _context.Quizzes
                .Where(q => q.LessonId == lessonId)
                .ToListAsync();
        }
        public async Task<Lesson> GetLesson(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }
    }
}
