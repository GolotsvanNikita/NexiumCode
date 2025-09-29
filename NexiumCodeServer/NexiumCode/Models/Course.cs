using System.ComponentModel.DataAnnotations;

namespace NexiumCode.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Lesson> Lessons { get; set; }
    }
}
