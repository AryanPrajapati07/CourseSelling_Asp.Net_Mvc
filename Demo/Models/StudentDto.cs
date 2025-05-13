using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    public class StudentDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [Required, MaxLength(100)]
        public string Gender { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        public IFormFile ImageFile { get; set; }
    }
}
