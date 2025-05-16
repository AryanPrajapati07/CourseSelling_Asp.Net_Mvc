namespace Demo.Models
{
    public class Course
    {

        public int Id { get; set; }
        public string CourseTitle { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public string Instructor { get; set; }
        public decimal Price { get; set; }
        public string Hours { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string WhatLearn { get; set; }
        public IFormFile Tumbnail { get; set; }
        public IFormFile Video { get; set; }
        public string Options { get; set; }

      
    }
}
