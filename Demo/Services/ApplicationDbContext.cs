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
        //public DbSet<Course> Courses { get; set; }


    }
}
