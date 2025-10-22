using System.ComponentModel.DataAnnotations;

namespace NexiumCode.DTO
{
    public class ReplyDTO
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public int? ParentReplyId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public List<ReplyDTO> ChildReplies { get; set; }
    }
}
