using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public class ForumThreadRepository : GenericRepository<ForumThread>, IForumThreadRepository
    {
        public ForumThreadRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<ForumThread>> GetThreadsWithReplies()
        {
            return await _context.ForumThreads
                .Include(t => t.Replies)
                .Include(t => t.User)
                .Where(t => !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<ForumThread> GetThreadWithReplies(int threadId) 
        {
            return await _context.ForumThreads
                .Include(t => t.Replies)
                .Include(t => t.User)
                .Where(t => t.Id == threadId && !t.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task MarkAsResolved(int threadId)
        {
            var thread = await GetById(threadId);
            if (thread != null && !thread.IsDeleted)
            {
                thread.IsResolved = true;
                thread.CreatedAt = DateTimeOffset.UtcNow;
                await Update(thread);
            }
        }

        public async Task SoftDelete(int threadId)
        {
            var thread = await GetById(threadId);
            if (thread != null)
            {
                thread.IsDeleted = true;
                thread.CreatedAt = DateTimeOffset.UtcNow;
                await Update(thread);
            }
        }
    }
}
