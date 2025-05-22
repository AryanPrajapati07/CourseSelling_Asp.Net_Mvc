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


        public List<Student> GetStudentsSortedByName()
        {
            return Students.FromSqlRaw("EXEC GetStudentsSortedByName").ToList();
        }

        public List<Student> GetStudentsSortedByAge()
        {
            return Students.FromSqlRaw("EXEC GetStudentsSortedByAge").ToList();
        }

        public List<Student> GetStudentsSortedByGender(string gender)
        {
            return Students.FromSqlInterpolated($"EXEC GetStudentsSortedByGender @Gender = {gender}").ToList();
        }

        public List<Student> GetStudentsSortedByDate(DateTime dobFrom, DateTime dobTo)
        {

            return Students.FromSqlInterpolated($"EXEC GetStudentsSortedByDate @DobFrom = {dobFrom}, @DobTo = {dobTo}").ToList();

        }




    }
}
