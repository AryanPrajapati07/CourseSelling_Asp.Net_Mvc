using System.ComponentModel.DataAnnotations;
using MathNet.Numerics;

namespace Demo.Models
{
    public class Student
    {

        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string Gender { get; set; }

        public DateTime Dob { get; set; }

        [MaxLength(100)]
        public string ImageFileName { get; set; }

        public string Age { get; set; }


    }
}
