using Demo.Models;
using Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    public class InstructorsController : Controller
    {

        private readonly ApplicationDbContext context;
        public InstructorsController(ApplicationDbContext context)
        {
            this.context = context;

        }
        public IActionResult Index()
        {

            List<Instructor> instructors = context.Instructors.ToList();


            return View();
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Instructor instructor)
        {

            // Get all selected specializations from the form
            var selectedSpecializations = Request.Form["Specialization"];
            if (selectedSpecializations.Count > 0)
            {
                instructor.Specialization = string.Join(",", selectedSpecializations);
            }


            if (ModelState.IsValid)
            {
                // Handle image upload
                if (instructor.Profile != null && instructor.Profile.Length > 0)
                {
                    var imageFileName = Guid.NewGuid() + Path.GetExtension(instructor.Profile.FileName);
                    var imagePath = Path.Combine("wwwroot/uploads/images", imageFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await instructor.Profile.CopyToAsync(stream);
                    }   
                    instructor.ProfilePath = "/uploads/images/" + imageFileName;
                }

               
                context.Instructors.Add(instructor);
                await context.SaveChangesAsync();
                return RedirectToAction("Instructor", "Courses");
            }
            return View(instructor);
        }
    }
}
