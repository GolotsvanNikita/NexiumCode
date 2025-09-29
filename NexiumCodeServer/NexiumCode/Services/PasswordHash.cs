using BCrypt.Net;

namespace NexiumCode.Services
{
    public class PasswordHash : IPasswordHash
    {
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) 
            {
                throw new ArgumentException("Password not be empty", nameof(password));
            }
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash)) 
            {
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
