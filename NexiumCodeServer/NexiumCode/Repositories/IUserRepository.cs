using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmail(string email);

        Task AddUser(User user, string password);

        Task<User> GetProfile(int userId);

        Task UpdateRating(int userId, int rating);
    }
}
