using System.Net;
using System.Net.Mail;
using Demo.Models;
using Demo.Services;
using DinkToPdf.Contracts;
using Intuit.Ipp.Core.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;





namespace Demo.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly InvoiceService invoiceService;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly IConverter _converter;


        //country and states data
        private static readonly Dictionary<string, List<string>> CountryStates = new()
        {
            { "India", new List<string> { "Gujarat", "Maharashtra", "Karnataka", "Delhi" } },
            { "USA", new List<string> { "California", "Texas", "Florida" } },
            { "Canada", new List<string> { "Ontario", "Quebec", "British Columbia" } }

        };


        public List<SelectListItem> Countries { get; private set; }
        public List<SelectListItem> States { get; private set; }

        public StudentsController(ApplicationDbContext context, InvoiceService invoiceService, IMemoryCache cache, IEmailService emailService, IConverter converter)
        {
            this.context = context;
            this.invoiceService = invoiceService;
            this._cache = cache;
            this._emailService = emailService;
            this._converter = converter;
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
            const string fromPassword = "qjqpozuuabxjbqvk"; // Use App Password, not your Gmail password
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
                HttpContext.Session.SetString("StudentEmail", email);
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

            var student = context.Students.FirstOrDefault(s => s.Email == User.Identity.Name);


            ViewBag.StudentName = student?.Name;
            ViewBag.StudentEmail = student?.Email;
            ViewBag.StudentCountry = student?.Country;
            ViewBag.StudentState = student?.State;



            return View(course);
        }


        // Updated Profile method to fix CS0021 error
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("StudentEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("StudentLogin");

            var student = context.Students.FirstOrDefault(s => s.Email == email);
            if (student == null)
                return NotFound();

            var enrollments = (from e in context.Enrollments
                               join c in context.Courses on e.CourseId equals c.Id
                               join s in context.Students on e.StudentEmail equals s.Email
                               where s.Email == email
                               select new
                               {
                                   Enrollment = new
                                   {
                                       e.CourseId,
                                       e.PaymentId,
                                       e.Amount,
                                       e.PaymentDate,
                                       e.InvoicePath
                                   },
                                   Course = new
                                   {
                                       c.CourseTitle,
                                       c.Hours,
                                       c.Instructor
                                   },
                                   Student = new
                                   {
                                       s.Name,
                                       s.Email
                                   }
                               }).ToList();

            ViewBag.Enrollments = enrollments;

            return View(student);
        }

        //private void SendConfirmationEmail(string toEmail, string subject, string body)
        //{
        //    var fromEmail = "aryanprajapati5523@gmail.com";
        //    var password = "qjqpozuuabxjbqvk";

        //    var message = new MailMessage();
        //    message.From = new MailAddress(fromEmail, "EduMaster");
        //    message.To.Add(toEmail);
        //    message.Subject = subject;
        //    message.Body = body;
        //    message.IsBodyHtml = true;

        //    try
        //    {
        //        using (var smtp = new SmtpClient("smtp.gmail.com", 587))
        //        {
        //            smtp.Credentials = new NetworkCredential(fromEmail, password);
        //            smtp.EnableSsl = true;
        //            smtp.Send(message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Email Error: " + ex.Message);
        //        // Or log to file, or show in view temporarily
        //    }

        //}

        //Generate Invoice 
        public async Task<IActionResult> PaymentSuccess(string paymentId, decimal amount, int courseId)
        {
            var email = HttpContext.Session.GetString("StudentEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("StudentLogin");

            var course = context.Courses.FirstOrDefault(c => c.Id == courseId);
            var student = context.Students.FirstOrDefault(s => s.Email == email);

            if (course == null || student == null)
                return NotFound();

    //        // Compose email content
    //        string subject = "Course Enrollment Confirmation";
    //        string body = $@"
    //    Dear {student.Name},<br/><br/>

    //    Thank you for enrolling in <strong>{course.CourseTitle}</strong>.<br/>
    //    Here are your enrollment details:<br/><br/>

    //    <strong>Payment ID:</strong> {paymentId}<br/>
    //    <strong>Amount Paid:</strong> ₹{amount}<br/>
    //    <strong>Course Duration:</strong> {course.Hours} Hours<br/>
    //    <strong>Instructor:</strong> {course.Instructor}<br/>
    //    <strong>Enrollment Date:</strong> {DateTime.Now:MMMM dd, yyyy}<br/><br/>

    //    If you have any questions, feel free to contact us at support@yourplatform.com.<br/><br/>

    //    Best regards,<br/>
    //    EduMaster Team
    //";

    //        // Send email
    //        SendConfirmationEmail(student.Email, subject, body);

    //        ViewBag.EmailStatus = "Confirmation email sent to " + student.Email;
            return RedirectToAction("Profile");

            //return RedirectToAction("Profile");
        }
        




        //send mail





        [HttpPost]
        public IActionResult Enroll([FromBody] EnrollmentDto dto)
        {
            var email = HttpContext.Session.GetString("StudentEmail");
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Session expired. Please log in again." });
            }


            try
            {
                var enrollment = new Enrollment
                {
                    StudentEmail = email,
                    CourseId = dto.CourseId,
                    PaymentId = dto.PaymentId,
                    Amount = dto.Amount,
                    PaymentDate = dto.PaymentDate
                };




                context.Enrollments.Add(enrollment);
                context.SaveChanges(); //  This line causes the error

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log full inner exception to identify the real problem
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "DB Error: " + errorMessage });
            }
        }




        public class EnrollmentDto
        {
            public int CourseId { get; set; }
            public string PaymentId { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
        }



        public async Task<IActionResult> DownloadEnrollmentPdf(string paymentId)
        {
            var email = HttpContext.Session.GetString("StudentEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("StudentLogin");

            var enrollment = context.Enrollments.FirstOrDefault(e => e.PaymentId == paymentId && e.StudentEmail == email);
            if (enrollment == null)
                return NotFound("Enrollment not found.");

            var student = context.Students.FirstOrDefault(s => s.Email == email);
            var course = context.Courses.FirstOrDefault(c => c.Id == enrollment.CourseId);

            if (student == null || course == null)
                return NotFound("Required data not found.");

            var pdfBytes = invoiceService.GenerateInvoice(
                enrollment.PaymentId,
                enrollment.Amount,
                course.CourseTitle,
                course.Hours,
                course.Instructor,
                student.Name,
                student.Email
            );


            string subject = "Your Course Enrollment Invoice";
            string body = $@"
<p>Dear {student.Name},</p>

<p>Thank you for enrolling in <strong>{course.CourseTitle}</strong>.</p>

<p><strong>Here are your invoice details:</strong></p>
<ul>
    <li><strong>Payment ID:</strong> {enrollment.PaymentId}</li>
    <li><strong>Amount Paid:</strong> ₹{enrollment.Amount}</li>
    <li><strong>Course Title:</strong> {course.CourseTitle}</li>
    <li><strong>Course Duration:</strong> {course.Hours} Hours</li>
    <li><strong>Instructor:</strong> {course.Instructor}</li>
    <li><strong>Student Name:</strong> {student.Name}</li>
    <li><strong>Enrollment Date:</strong> {DateTime.Now:MMMM dd, yyyy}</li>
</ul>

<p>If you have any questions, feel free to contact us at <a href='mailto:aryanprajapati5523@gmail.com'>support@edumaster.com</a>.</p>

<br/>
<p>Best regards,<br/>EduMaster Team</p>";


            invoiceService.SendInvoiceEmail(email, subject, body, pdfBytes);



            var fileName = $"enrollment_{paymentId}.pdf";



            return File(pdfBytes, "application/pdf", fileName);

            

           
        }


    }
}
