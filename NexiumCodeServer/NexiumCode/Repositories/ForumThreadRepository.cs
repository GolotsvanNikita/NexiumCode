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
                    .ThenInclude(r => r.User)
                .Include(t => t.User)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<ForumThread> GetThreadWithReplies(int threadId)
        {
            return await _context.ForumThreads
                .Include(t => t.Replies)
                    .ThenInclude(r => r.User)
                .Include(t => t.Replies)
                    .ThenInclude(r => r.ChildReplies)
                        .ThenInclude(c => c.User)
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
                await Update(thread);
            }
        }

        public async Task SoftDelete(int threadId)
        {
            var thread = await GetById(threadId);
            if (thread != null)
            {
                thread.IsDeleted = true;
                await Update(thread);
            }
        }
    }
}
