using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.Models;
using System.Runtime.CompilerServices;

namespace NexiumCode.Repositories
{
    public class ProgressRepository : GenericRepository<Progress>, IProgressRepository
    {
        public ProgressRepository(AppDbContext context) : base(context) { }

        public async Task<Progress> GetProgressByUserAndCourse(int userId, int courseId)
        {
            return await _context.Progresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);
        }

        public async Task UpdateTheoryProgress(int userId, int courseId, int progress)
        {
            var progressRecord = await GetProgressByUserAndCourse(userId, courseId);
            if (progressRecord == null)
            {
                progressRecord = new Progress
                {
                    UserId = userId,
                    CourseId = courseId,
                    TheoryProgress = progress,
                    LastUpdated = DateTimeOffset.UtcNow
                };
                await Add(progressRecord);
            }
            else
            {
                progressRecord.TheoryProgress = progress;
                progressRecord.LastUpdated = DateTimeOffset.UtcNow;
                await Update(progressRecord);
            }
        }

        public async Task UpdatePracticeProgress(int userId, int courseId, int progress)
        {
            var progressRecord = await GetProgressByUserAndCourse(userId, courseId);
            if (progressRecord == null)
            {
                progressRecord = new Progress
                {
                    UserId = userId,
                    CourseId = courseId,
                    PracticeProgress = progress,
                    LastUpdated = DateTimeOffset.UtcNow
                };
                await Add(progressRecord);
            }
            else
            {
                progressRecord.PracticeProgress = progress;
                progressRecord.LastUpdated = DateTimeOffset.UtcNow;
                await Update(progressRecord);
            }
        }

        public async Task UpdateLessonProgress(int userId, int courseId, int lessonId, int progress)
        {
            var progressRecord = await GetProgressByUserAndCourse(userId, courseId);
            if (progressRecord == null)
            {
                progressRecord = new Progress
                {
                    UserId = userId,
                    CourseId = courseId,
                    TheoryProgress = 0,
                    PracticeProgress = 0,
                    LastUpdated = DateTimeOffset.UtcNow
                };
                await Add(progressRecord);
            }

            var lessonProgress = progressRecord.ProgressLessons
                .FirstOrDefault(pl => pl.LessonId == lessonId);
            if (lessonProgress == null)
            {
                lessonProgress = new ProgressLesson
                {
                    ProgressId = progressRecord.Id,
                    LessonId = lessonId,
                    ProgressValue = progress
                };
                _context.ProgressLessons.Add(lessonProgress);
            }
            else
            {
                lessonProgress.ProgressValue = progress;
            }
            progressRecord.LastUpdated = DateTimeOffset.UtcNow;
            await Update(progressRecord);
            await _context.SaveChangesAsync();
        }
    }
}