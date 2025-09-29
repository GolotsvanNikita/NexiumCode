using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface IForumReplyRepository : IRepository<ForumReply>
    {
        Task<IEnumerable<ForumReply>> GetRepliesByThread(int threadId);
    }
}
