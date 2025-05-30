using System.Data;
using Demo.Models;
using Demo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Demo.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration _configuration; // Change type from object to IConfiguration

        public InstructorsController(ApplicationDbContext context, IConfiguration configuration) // Add IConfiguration to constructor
        {
            this.context = context;
            this._configuration = configuration; // Assign the injected configuration
        }

        // GET: Instructors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Instructors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (instructor.Profile != null && instructor.Profile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "instructors");
                    Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(instructor.Profile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await instructor.Profile.CopyToAsync(stream);
                    }

                    // Save relative path to database
                    instructor.ProfilePath = "/uploads/instructors/" + uniqueFileName;
                }

                context.Instructors.Add(instructor);
                await context.SaveChangesAsync();
                return RedirectToAction("Dashboard","Courses"); // Or your desired page
            }
            return View(instructor);
        }


        public List<Instructor> GetInstructors()
        {
            var instructors = new List<Instructor>();
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"))) // Fix type issue
            {
                using (var command = new SqlCommand("GetAllInstructors", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            instructors.Add(new Instructor
                            {
                                Id = (int)reader["Id"],
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                Department = reader["Department"].ToString(),
                                Position = reader["Position"].ToString(),
                                Experience = reader["Experience"].ToString(),
                                Availability = reader["Availability"].ToString()


                            });
                        }
                    }
                }
            }
            return instructors;
        }
    }
}
