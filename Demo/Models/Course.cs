using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Demo.Models
{
    public class Course
    {

        public int Id { get; set; }

        [Required,MaxLength(100)]
        public string CourseTitle { get; set; }
        
        [Required]
        public string Category { get; set; }

        [Required]
        public string Level { get; set; }

        [Required]
        public string Instructor { get; set; }

        [Precision(16, 2)]
        public decimal Price { get; set; }

        [Required]
        public string Hours { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Requirements { get; set; }

        [Required]
        public string WhatLearn { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public IFormFile? Video { get; set; }
        //public string Options { get; set; }

        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }


    }
}
