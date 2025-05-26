using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public DateTime Birth { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        public string Experience { get; set; }

        [Required]
        public DateTime JoinDate { get; set; }

       

        [Required]
        public string Specialization { get; set; }

        [Required]
        public string HighestDegree { get; set; }

        [Required]
        public string University { get; set; }

        [Required]
        public string GraduationYear { get; set; }

        [Required]
        public string Grade { get; set; }

        [Required]
        public string Availability { get; set; }

        [NotMapped]
        public IFormFile? Profile { get; set; }

        public string? ProfilePath { get; set; }




    }
}
