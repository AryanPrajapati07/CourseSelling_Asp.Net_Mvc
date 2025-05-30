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

    public byte[] GenerateInvoice(string paymentId, decimal amount, string courseName, string duration, string instructor, string studentName, string email)
    {
        var html = $@"
    <h2>Course Enrollment Invoice</h2>
    <p><strong>Razorpay Payment ID:</strong> {paymentId}</p>
    <p><strong>Amount Paid:</strong> {amount:C}</p>
    <hr/>
    <p><strong>Course Name:</strong> {courseName}</p>
    <p><strong>Duration:</strong> {duration}</p>
    <p><strong>Instructor:</strong> {instructor}</p>
    <hr/>
    <p><strong>Student Name:</strong> {studentName}</p>
    <p><strong>Email:</strong> {email}</p>

    <p><em>Date: {DateTime.Now:yyyy-MM-dd}</em></p>
    ";

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = { PaperSize = PaperKind.A4, Orientation = Orientation.Portrait },
            Objects = { new ObjectSettings { HtmlContent = html } }
        };

        return _converter.Convert(doc);
    }
}

