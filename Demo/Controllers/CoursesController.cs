using Microsoft.AspNetCore.Mvc;
using Demo.Models;
using Demo.Services;
using Microsoft.EntityFrameworkCore;
using MathNet.Numerics.Distributions;
using OfficeOpenXml;
using System.Text;
using System.Net.Mail;
using Microsoft.AspNetCore.Antiforgery;
using System.Net;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using FluentAssertions.Extensions;

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

        public IActionResult PaymentList()
        {
            
            var payments = (from e in context.Enrollments
                            join c in context.Courses on e.CourseId equals c.Id
                            join s in context.Students on e.StudentEmail equals s.Email
                            select new
                            {
                                EnrollmentId = e.Id,
                                StudentEmail = s.Email,
                                CourseId = c.Id,
                                PaymentId = e.PaymentId,
                                Amount = e.Amount,
                                PaymentDate = e.PaymentDate
                            }).ToList();

            ViewBag.TotalRevenue = payments.Sum(p => p.Amount);
            ViewBag.TotalEnrolledCourses = payments.Count();



            ViewBag.Payments = payments;

            return View();

        }

        //download enrollment csv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DownloadCsv()
        {
            var enrollments = context.Enrollments.ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Enrollment ID,Email,Course ID,Payment ID,Amount,Payment Date");

            foreach (var e in enrollments)
            {
                csv.AppendLine($"En250{e.Id},{e.StudentEmail},CS250{e.CourseId},{e.PaymentId},₹{e.Amount},{e.PaymentDate:yyyy-MM-dd}");
            }

            byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());
            string fileName = $"EnrollmentData-{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        //cancel enrollment 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelEnrollment(int id, string email)
        {
            var enrollment = context.Enrollments.FirstOrDefault(e => e.Id == id && e.StudentEmail == email);
            if (enrollment == null)
            {
                return NotFound("Enrollment not found.");
            }

            var courseId = enrollment.CourseId;
            var paymentDate = enrollment.PaymentDate;

            // Check if payment is older than 7 days
            var daysSincePayment = (DateTime.Now - paymentDate).TotalDays;
            if (daysSincePayment > 7)
            {
                return BadRequest("Enrollment cannot be cancelled. Refund period (7 days) has expired.");
            }

            context.Enrollments.Remove(enrollment);
            context.SaveChanges();

            SendCancellationEmail(email, courseId);
            return Ok("Enrollment cancelled successfully.");
        }



        private void SendCancellationEmail(string studentEmail, int courseId)
        {
            var fromAddress = new MailAddress("aryanprajapati5523@gmail.com", "EduMaster");
            var toAddress = new MailAddress(studentEmail);
            const string fromPassword = "qjqpozuuabxjbqvk"; 

            var course = context.Courses.FirstOrDefault(c => c.Id == courseId);
            var subject = "Enrollment Cancelled";
            var body = $"Your enrollment in course '{course?.CourseTitle}' has been cancelled. Refund will be process in 5-7 business days.";

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

        //Review Display

        public IActionResult Reviews()
        {
            var reviews = context.Reviews.Include(r => r.Course).ToList();
            return View(reviews);
        }

    }

}

