using System.Reflection;
using Demo.Controllers;
using Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }

        public DbSet<Instructor> Instructors { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; } // Ensure Enrollment class is public
                                                           // 
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





        // Ensure the Enrollment class is public  
       






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
