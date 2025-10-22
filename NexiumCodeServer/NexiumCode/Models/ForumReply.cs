using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexiumCode.Models
{
    public class ForumReply
    {
        public int Id { get; set; }

        [Required]
        public int ThreadId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string Content { get; set; }

        public int? ParentReplyId { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ForumThread Thread { get; set; }

        public User User { get; set; }

        public ForumReply ParentReply { get; set; }

        public ICollection<ForumReply> ChildReplies { get; set; }
    }
}
