using System.Reflection;
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
