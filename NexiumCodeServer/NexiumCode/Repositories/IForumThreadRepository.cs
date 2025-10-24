using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface IForumThreadRepository : IRepository<ForumThread>
    {
        Task<ForumThread> GetThreadWithReplies(int id);
        Task<IEnumerable<ForumThread>> GetThreadsWithReplies();
        Task MarkAsResolved(int id);
        Task SoftDelete(int id);
        Task<(IEnumerable<ForumThread> Threads, int TotalCount)> GetThreadsPagedAsync(string? category, string? search, int page, int pageSize);
    }
}
