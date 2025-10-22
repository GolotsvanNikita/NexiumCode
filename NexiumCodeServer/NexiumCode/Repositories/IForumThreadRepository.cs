using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface IForumThreadRepository : IRepository<ForumThread>
    {
        Task<ForumThread> GetThreadWithReplies(int id);
        Task<IEnumerable<ForumThread>> GetThreadsWithReplies();
        Task MarkAsResolved(int id);
        Task SoftDelete(int id);
    }
}
