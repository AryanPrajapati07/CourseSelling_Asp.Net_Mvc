using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Demo.Models;

namespace Demo.Services
{
    public interface IEmailService
    {
        Task SendEnrollmentEmailAsync(string studentName, string toEmail, Course course, string paymentId, decimal amount);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_config["Smtp:Host"])
            {
                Port = int.Parse(_config["Smtp:Port"]),
                Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Smtp:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendEnrollmentEmailAsync(string studentName, string toEmail, Course course, string paymentId, decimal amount)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var subject = "Course Enrollment Confirmation";
            var body = $@"
            <p>Dear {studentName},</p>
            <p>Thank you for enrolling in <strong>{course.CourseTitle}</strong>!</p>
            <ul>
                <li><strong>Instructor:</strong> {course.Instructor}</li>
                <li><strong>Hours:</strong> {course.Hours}</li>
                <li><strong>Amount: ₹</strong> {amount}</li>
                <li><strong>Payment ID:</strong> {paymentId}</li>
            </ul>
            <p>You can view your enrollment and download the invoice anytime from your profile.</p>
            <p>Best regards,<br/>Learning Platform Team</p>";

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(
                    emailSettings["FromAddress"],
                    emailSettings["FromName"]);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtpClient = new SmtpClient(
                    emailSettings["SmtpServer"],
                    int.Parse(emailSettings["Port"])))
                {
                    smtpClient.Credentials = new NetworkCredential(
                        emailSettings["Username"],
                        emailSettings["Password"]);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 10000;

                    await smtpClient.SendMailAsync(message);
                }
            }
        }
    }
}

