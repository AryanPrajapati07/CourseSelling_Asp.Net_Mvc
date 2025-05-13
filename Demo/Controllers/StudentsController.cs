using Demo.Models;
using Demo.Services;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext context;

        public StudentsController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            var students = context.Students.ToList();

            return View(students);
        }


        //Add Student Data
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentDto studentDto)
        {


            try
            {
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/products");
                if (!Directory.Exists(wwwRootPath))
                    Directory.CreateDirectory(wwwRootPath);


                if (studentDto.ImageFile == null)
                {
                    ModelState.AddModelError("ImageFile", "The image file is required");
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(studentDto.ImageFile.FileName);
                string filePath = Path.Combine(wwwRootPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    studentDto.ImageFile.CopyTo(fileStream);
                }

                if (studentDto.ImageFile == null || string.IsNullOrEmpty(studentDto.ImageFile.FileName))
                {
                    return Content("Image file is not uploaded properly.");
                }



                var student = new Student
                {
                    Name = studentDto.Name,
                    Email = studentDto.Email,
                    Gender = studentDto.Gender,
                    Dob = studentDto.DOB,
                    ImageFileName = "/products/" + fileName

                };

                context.Students.Add(student);
                context.SaveChanges();

                return RedirectToAction("Index", "Students");
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message + "\n" + ex.StackTrace);
            }


        }


        //Delete Student Data
        public IActionResult Delete(int id)
        {

            var student = context.Students.FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {

            var student = context.Students.FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }


            context.Students.Remove(student);


            context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        
        public IActionResult Edit(int id)
        {
            
            var student = context.Students.FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

           
            return View(student);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Student student)
        {
            if (id != student.Id)
            {
                return NotFound(); 
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    context.Students.Update(student);
                    context.SaveChanges();
                }
                catch (Exception)
                {
                   
                    return Content("An error occurred while updating the student.");
                }

                return RedirectToAction(nameof(Index)); 
            }

            return RedirectToAction("Index", "Students"); 
        }


    }
}
