using System.ComponentModel.DataAnnotations;

namespace NexiumCode.Models
{
    public class ForumThread
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        public bool IsResolved { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public User User { get; set; }

        public ICollection<ForumReply> Replies { get; set; }
    }
}
