using Demo.Models;
using Demo.Services;
using Intuit.Ipp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Razorpay.Api;
using System.Net;
using System.Net.Mail;




namespace Demo.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext context;

        //country and states data
        private static readonly Dictionary<string, List<string>> CountryStates = new()
        {
            { "India", new List<string> { "Gujarat", "Maharashtra", "Karnataka", "Delhi" } },
            { "USA", new List<string> { "California", "Texas", "Florida" } },
            { "Canada", new List<string> { "Ontario", "Quebec", "British Columbia" } }

        };

        public List<SelectListItem> Countries { get; private set; }
        public List<SelectListItem> States { get; private set; }

        public StudentsController(ApplicationDbContext context)
        {
            this.context = context;

        }

        //add dashboard page 
        public IActionResult AddCourse()
        {
            return View();
        }


        public IActionResult Index(string searchString, string sortBy, string gender, DateTime? dobFrom, DateTime? dobTo)
        {
            List<Student> students;

            if (sortBy == "Name" || sortBy == "Age" || sortBy == "Gender" || sortBy == "Date" ||
                !string.IsNullOrEmpty(gender) || (dobFrom.HasValue && dobTo.HasValue))
            {
                students = context.GetStudentsData(sortBy, gender, dobFrom, dobTo);
            }
            else
            {
                students = context.Students.ToList();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students
                    .Where(s => s.Name.Contains(searchString) || s.Email.Contains(searchString))
                    .ToList();
            }

            return View(students);
        }


        //Add Student Data
        [HttpGet]
        public IActionResult Create()
        {
            var model = new StudentDto
            {
                Countries = new List<SelectListItem>
            {
                new SelectListItem { Value = "India", Text = "India" },
                new SelectListItem { Value = "USA", Text = "USA" },
                new SelectListItem { Value = "Canada", Text = "Canada" }
            },
                States = new List<SelectListItem>()
            };
            return View(model);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Create(StudentDto studentDto)
        {
            if (studentDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }


            // Check if email already exists
            bool emailExists = context.Students.Any(s => s.Email == studentDto.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email is already registered");
            }

            if (!ModelState.IsValid)
            {
                return View(studentDto);
            }

            try
            {
                // Save image
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(wwwRootPath))
                    Directory.CreateDirectory(wwwRootPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(studentDto.ImageFile.FileName);
                string filePath = Path.Combine(wwwRootPath, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    studentDto.ImageFile.CopyTo(fileStream);
                }

                var student = new Student
                {
                    Name = studentDto.Name,
                    Email = studentDto.Email,
                    Gender = studentDto.Gender,
                    Dob = studentDto.DOB,
                    Age = studentDto.Age,
                    Country = studentDto.Country,
                    State = studentDto.State,
                    ImageFileName = "/images/" + fileName

                };

                context.Students.Add(student);
                context.SaveChanges();




                return RedirectToAction("Index", "Students");
            }
            catch (Exception ex)
            {

                return Content("Error: " + ex.Message);
            }
        }

        //Get states based on selected country
        [HttpGet]
        public JsonResult GetStates(string country)
        {
            var states = CountryStates.ContainsKey(country)
                 ? CountryStates[country]
                 : new List<string>();
            return Json(states);
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

            var existingStudent = context.Students.FirstOrDefault(s => s.Id == id);
            if (existingStudent == null)
            {
                return NotFound();
            }

            // Handle file upload
            var file = Request.Form.Files["ImageFile"];
            if (file != null && file.Length > 0)
            {
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/products");
                if (!Directory.Exists(wwwRootPath))
                    Directory.CreateDirectory(wwwRootPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(wwwRootPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                existingStudent.ImageFileName = "/products/" + fileName;
            }

            // Update other fields
            existingStudent.Name = student.Name;
            existingStudent.Email = student.Email;
            existingStudent.Gender = student.Gender;
            existingStudent.Dob = student.Dob;
            existingStudent.Age = student.Age;
            //existingStudent.Country = student.Country;
            //existingStudent.State = student.State;

            context.Students.Update(existingStudent);
            context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }



        //OTP logic
        private string GenerateOtp(int length = 6)
        {
            var random = new Random();
            string otp = "";
            for (int i = 0; i < length; i++)
                otp += random.Next(0, 10).ToString();
            return otp;
        }
        private void SendOtpEmail(string email, string otp)
        {
            
            System.Diagnostics.Debug.WriteLine($"Send OTP {otp} to {email}");


            var fromAddress = new MailAddress("aryanprajapati5523@gmail.com", "EduMaster");
            var toAddress = new MailAddress(email);
            const string fromPassword = "bojvatecoqvsxjds"; // Use App Password, not your Gmail password
            const string subject = "Your OTP Code";
            string body = $"Your OTP is: {otp}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }



        //STUDENT LOGIN

        public IActionResult StudentLogin()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StudentLogin(string Email, string Password)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                return View();
            }

            var student = context.Students.FirstOrDefault(s => s.Email == Email);
            if (student == null)
            {
                ModelState.AddModelError("Email", "Email not found.");
                return View();
            }

            //password: Name + ddMM
            string expectedPassword = $"{student.Name.Trim()}{student.Dob:ddMM}";

            if (!string.Equals(Password, expectedPassword, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Password", "Invalid password.");
                return View();
            }

            TempData["StudentEmail"] = student.Email;

            // Generate OTP
            string otp = GenerateOtp();
            TempData["StudentOtp"] = otp;
            TempData["StudentEmail"] = Email;

            // Send OTP via email (replace with your email sending logic)
            SendOtpEmail(Email, otp);

            // Redirect to OTP verification page
            return RedirectToAction("VerifyOtp");
        }

        //otp verification logic
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(string enteredOtp)
        {
            var storedOtp = TempData["StudentOtp"] as string;
            var email = TempData["StudentEmail"] as string;

            if (storedOtp == null || email == null)
            {
                ModelState.AddModelError("", "Session expired. Please login again.");
                return RedirectToAction("StudentLogin");
            }

            if (enteredOtp == storedOtp)
            {
                // OTP is correct, proceed to student home
                TempData["StudentEmail"] = email; // Keep for next request
                return RedirectToAction(nameof(StudentHome));
            }
            else
            {
                ModelState.AddModelError("", "Invalid OTP. Please try again.");
                TempData["StudentOtp"] = storedOtp; // Keep OTP for retry
                TempData["StudentEmail"] = email;
                return View();
            }
        }



        //student home page
        public IActionResult StudentHome()
        {
            return View();
        }


        public IActionResult StudentCourses()
        {

            var courses = context.Courses.ToList();
            return View(courses);
        }

        public IActionResult CourseDetails(int id)
        {
            var course = context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }


        // Updated Profile method to fix CS0021 error
        public IActionResult Profile()
        {
            var email = TempData["StudentEmail"] as string;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("StudentLogin");

            var student = context.Students.FirstOrDefault(s => s.Email == email);
            if (student == null)
                return NotFound();

            // Fetch payment from Razorpay


            TempData.Keep("StudentEmail");
            return View(student);
        }



    }
}
