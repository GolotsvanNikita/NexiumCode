using System.ComponentModel.DataAnnotations;

namespace NexiumCode.DTO
{
    public class IssueCertificateDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
