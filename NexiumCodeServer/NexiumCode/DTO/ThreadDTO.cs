using System.ComponentModel.DataAnnotations;

namespace NexiumCode.DTO
{
    public class ThreadDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsResolved { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Username { get; set; }
        public int ReplyCount { get; set; }
        public List<ReplyDTO> Replies { get; set; }
    }
}
