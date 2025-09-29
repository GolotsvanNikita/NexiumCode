using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface IForumThreadRepository : IRepository<ForumThread>
    {
        Task<IEnumerable<ForumThread>> GetThreadsWithReplies();
        Task<ForumThread> GetThreadWithReplies(int threadId);

        Task MarkAsResolved(int threadId);

        Task SoftDelete(int threadId);
    }
}
