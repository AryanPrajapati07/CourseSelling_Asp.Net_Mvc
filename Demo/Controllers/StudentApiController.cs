using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Demo.Models;
using Demo.Services;

namespace Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentApiController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public StudentApiController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("students")]
        public IActionResult GetStudents()
        {
            var students = context.Students.ToList();
            return Ok(students);
        }

        [HttpPost("register")]
        public IActionResult RegisterStudent([FromBody] Student student)
        {
           if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            context.Students.Add(student);
            context.SaveChanges();
            return Ok(new { message = "Student registered successfully", studentId = student.Id });
        }

        [HttpGet("enrollments")]
        public IActionResult GetEnrollments()
        {
            var enrollments = context.Enrollments.ToList();
            //var count = context.Enrollments.Count();
            //Console.WriteLine("Enrollment count: " + count);

            return Ok(enrollments);

        }

        [HttpGet("instructors")]
        public IActionResult GetInstructors()
        {
            var instructors = context.Instructors.ToList();
            return Ok(instructors);
        }

        [HttpGet("courses")]
        public IActionResult GetCourses()
        {
            var courses = context.Courses.ToList();
            return Ok(courses);
        }

    }
}
