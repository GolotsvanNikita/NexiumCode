using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public class ForumReplyRepository : GenericRepository<ForumReply>, IForumReplyRepository
    {
        public ForumReplyRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<ForumReply>> GetRepliesByThread(int threadId)
        {
            return await _context.ForumReplies
                .Where(r => r.ThreadId == threadId)
                .Include(r => r.User)
                .ToListAsync();
        }
    }
}
