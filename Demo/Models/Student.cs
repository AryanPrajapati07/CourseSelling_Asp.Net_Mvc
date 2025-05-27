using System.ComponentModel.DataAnnotations;
using MathNet.Numerics;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        [MaxLength(100),Required]
        public string Country { get; set; }

        
        [MaxLength(100), Required]
        public string State { get; set; }

        //[MaxLength(100)]
        //public string RazorpayPaymentId { get; set; }
        //public decimal? PaymentAmount { get; set; }

        //public List<SelectListItem> States { get; set; }
        //public List<SelectListItem> Countries { get; set; }

    }
}
