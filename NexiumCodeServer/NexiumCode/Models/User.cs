using System.ComponentModel.DataAnnotations;

namespace NexiumCode.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public int Rating { get; set; } = 0;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Progress> Progresses { get; set; }
        public ICollection<Certificate> Certificates { get; set; }
        public ICollection<ForumThread> ForumThreads { get; set; }
        public ICollection<ForumReply> ForumReplies { get; set; }
    }
}
