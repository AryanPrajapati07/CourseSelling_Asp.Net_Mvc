using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Reflection.Metadata;
using DinkToPdf;
using DinkToPdf.Contracts;

public class InvoiceService
{
    private readonly IConverter _converter;
    public InvoiceService(IConverter converter)
    {
        _converter = converter;
    }

    public void SendInvoiceEmail(string toEmail, string subject, string bodyText, byte[] pdfData)
    {
        var fromEmail = "aryanprajapati5523@gmail.com";
        var password = "qjqpozuuabxjbqvk"; // Use secure method in production

        var message = new MailMessage();
        message.From = new MailAddress(fromEmail, "EduMaster");
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = bodyText;
        message.IsBodyHtml = true;

        // Attach PDF
        var stream = new MemoryStream(pdfData);
        var attachment = new Attachment(stream, "Invoice.pdf", MediaTypeNames.Application.Pdf);
        message.Attachments.Add(attachment);

        using (var smtp = new SmtpClient("smtp.gmail.com", 587)) // Update this
        {
            smtp.Credentials = new NetworkCredential(fromEmail, password);
            smtp.EnableSsl = true;
            smtp.Send(message);
        }
    }






    public byte[] GenerateInvoice(string paymentId, decimal amount, string courseName, string duration, string instructor, string studentName, string email)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 40px;
            color: #333;
        }}
        h2 {{
            text-align: center;
            color: #4CAF50;
        }}
        .section {{
            margin-bottom: 20px;
        }}
        .label {{
            font-weight: bold;
        }}
        hr {{
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <h2>Course Enrollment Invoice</h2>

    <div class='section'>
        <p><span class='label'>Student Name:</span> {studentName}</p>
        <p><span class='label'>Email:</span> {email}</p>
    </div>

<hr/>

    <div class='section'>
        <p><span class='label'>Course Name:</span> {courseName}</p>
        <p><span class='label'>Duration:</span> {duration} Hours</p>
        <p><span class='label'>Instructor:</span> {instructor}</p>
    </div>


    <hr/>

    <div class='section'>
        <p><span class='label'>Razorpay Payment ID:</span> {paymentId}</p>
        <p><span class='label'>Amount Paid:</span> {amount:C}</p>
    </div>

    

    


   

    <p style='text-align:right;'><em>Issued on: {DateTime.Now:yyyy-MM-dd}</em></p>
</body>
</html>";

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
            PaperSize = PaperKind.A4,
            Orientation = Orientation.Portrait,
            Margins = new MarginSettings { Top = 10, Bottom = 10 }
        },
            Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
        };

        return _converter.Convert(doc);
    }

}

