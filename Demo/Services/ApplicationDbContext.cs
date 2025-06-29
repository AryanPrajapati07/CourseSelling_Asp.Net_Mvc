﻿using System.Reflection;
using Demo.Controllers;
using Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services
{
    public class ApplicationDbContext : DbContext
    {
     

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
     : base(options)
        {
        }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }

        public DbSet<Instructor> Instructors { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId);


            modelBuilder.Entity<Enrollment>().HasKey(e => e.Id);

            modelBuilder.Entity<Enrollment>()
      .Property(e => e.Amount)
      .HasPrecision(18, 2);
        }


        public List<Student> GetStudentsData(String sortBy = null, String gender = null, DateTime? dobFrom = null, DateTime? dobTo = null)
        {
            return Students.FromSqlInterpolated($@"
                    EXEC GetStudentsData
                        @SortBy = {sortBy},
                        @Gender = {gender},
                        @DobFrom = {dobFrom},
                        @DobTo = {dobTo}").ToList();
        }




    }
}
