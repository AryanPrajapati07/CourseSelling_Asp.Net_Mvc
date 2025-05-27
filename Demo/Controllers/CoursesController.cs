using Microsoft.AspNetCore.Mvc;
using Demo.Models;
using Demo.Services;
using Microsoft.EntityFrameworkCore;
using MathNet.Numerics.Distributions;

namespace Demo.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext context;
        public CoursesController(ApplicationDbContext context)
        {
            this.context = context;

        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Add()
        {


            var instructors = context.Instructors
               .Select(i => new
                {
                
                Name = i.FirstName + " " + i.LastName
                })
                .ToList();

            ViewBag.Instructors = instructors;



            return View();
        }


        // POST: Courses/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Course course)
        {

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (course.ImageFile != null && course.ImageFile.Length > 0)
                {
                    var imageFileName = Guid.NewGuid() + Path.GetExtension(course.ImageFile.FileName);
                    var imagePath = Path.Combine("wwwroot/uploads/images", imageFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await course.ImageFile.CopyToAsync(stream);
                    }
                    course.ImagePath = "/uploads/images/" + imageFileName;
                }

                // Handle video upload
                if (course.Video != null && course.Video.Length > 0)
                {
                    var videoFileName = Guid.NewGuid() + Path.GetExtension(course.Video.FileName);
                    var videoPath = Path.Combine("wwwroot/uploads/videos", videoFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(videoPath)!);

                    using (var stream = new FileStream(videoPath, FileMode.Create))
                    {
                        await course.Video.CopyToAsync(stream);
                    }
                    course.VideoPath = "/uploads/videos/" + videoFileName;
                }

                context.Courses.Add(course);
                await context.SaveChangesAsync();
                return RedirectToAction("AddCourse", "Students");
            }


            //// Repopulate instructors for redisplay
            //ViewBag.Instructors = context.Instructors
            //    .Select(i => new
            //    {
            //        Id = i.Id,
            //        Name = i.FirstName + " " + i.LastName
            //    })
            //    .ToList();

            //ViewBag.Instructors = context.Instructors.ToList();


            return View(course);


        }

        

        public IActionResult Dashboard(string searchCourse)
        {

            List<Course> courses = context.Courses.ToList();

            if (!string.IsNullOrEmpty(searchCourse))
            {

                courses = courses
                    .Where(s => s.CourseTitle.Contains(searchCourse) || s.Instructor.Contains(searchCourse))
                    .ToList();
            }


            var instructors = GetInstructors();
            ViewBag.Instructors = instructors;

            return View(courses);
            
        }

        

        public IActionResult Instructor()
        {
            return View();
        }
        private List<Instructor> GetInstructors()
        {
            return context.Instructors.ToList();
        }


    }

}

