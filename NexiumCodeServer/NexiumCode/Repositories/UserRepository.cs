using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.Models;
using NexiumCode.Services;

namespace NexiumCode.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly IPasswordHash _passwordHash;

        public UserRepository(AppDbContext context, IPasswordHash passwordHash)
            : base(context)
        {
            _passwordHash = passwordHash;
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUser(User user, string password)
        {
            user.PasswordHash = _passwordHash.HashPassword(password);
            user.CreatedAt = DateTimeOffset.UtcNow;
            await Add(user);
        }

        public async Task<User> GetProfile(int userId)
        {
            return await _context.Users
                .Include(u => u.Certificates)
                .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateRating(int userId, int rating)
        {
            var user = await GetById(userId);
            if (user != null)
            {
                user.Rating = rating;
                user.CreatedAt = DateTimeOffset.UtcNow;
                await Update(user);
            }
        }
    }
}
