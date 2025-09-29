using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexiumCode.Models
{
    public class Certificate
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTimeOffset IssueDate { get; set; } = DateTimeOffset.UtcNow;

        public string CertificateUrl { get; set; }

        public User User { get; set; }

        public Course Course { get; set; }
    }
}
