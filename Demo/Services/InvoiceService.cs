using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Reflection.Metadata;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Drawing.Printing;
using Intuit.Ipp.Data;

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
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 40px;
            background-color: #f8f9fa;
            color: #212529;
        }}
        
        .invoice-container {{
            background-color: #fff;
            padding: 30px 40px;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            max-width: 700px;
            margin: auto;
        }}
        h2 {{
            text-align: center;
            color: #28a745;
            margin-bottom: 30px;
        }}
        h1{{
            text-align:center;
            color:#4361EE;
        }}
        .section {{
            margin-bottom: 25px;
        }}
        .section-title {{
            font-size: 1.1rem;
            margin-bottom: 10px;
            border-bottom: 1px solid #dee2e6;
            padding-bottom: 5px;
            color: #343a40;
        }}
        .field {{
            margin: 6px 0;
        }}
        .label {{
            font-weight: 600;
            color: #495057;
        }}
        .value {{
            margin-left: 5px;
        }}
        .footer {{
            text-align: right;
            margin-top: 30px;
            font-style: italic;
            color: #6c757d;
        }}
    </style>
</head>
<body>
        <h1>EduMaster</h1>

    <div class='invoice-container'>

        <h2>Course Enrollment Invoice</h2>

        <div class='section'>
            <div class='section-title'>Student Details</div>
            <div class='field'><span class='label'>Name:</span><span class='value'>{studentName}</span></div>
            <div class='field'><span class='label'>Email:</span><span class='value'>{email}</span></div>
        </div>

        <div class='section'>
            <div class='section-title'>Course Details</div>
            <div class='field'><span class='label'>Course Name:</span><span class='value'>{courseName}</span></div>
            <div class='field'><span class='label'>Duration:</span><span class='value'>{duration} Hours</span></div>
            <div class='field'><span class='label'>Instructor:</span><span class='value'>{instructor}</span></div>
        </div>

        <div class='section'>
            <div class='section-title'>Payment Information</div>
            <div class='field'><span class='label'>Razorpay Payment ID:</span><span class='value'>{paymentId}</span></div>
            <div class='field'><span class='label'>Amount Paid:</span><span class='value'>{amount:C}</span></div>
        </div>

        <div class='footer'>
            Issued on: {DateTime.Now:yyyy-MM-dd}
        </div>
    </div>
</body>
</html>";


        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
            PaperSize = DinkToPdf.PaperKind.A4,
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

