using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        [Required]
        public string StudentEmail { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }

}
