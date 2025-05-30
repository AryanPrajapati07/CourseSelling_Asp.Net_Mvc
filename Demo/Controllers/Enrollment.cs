using System;
using Demo.Models;

namespace Demo.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        // Foreign key
        public int? StudentId { get; set; }

        // Navigation property
        public Student? Student { get; set; }

        public string StudentEmail { get; set; }
        public int CourseId { get; set; }
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        public string? InvoicePath { get; set; }
    }
}
