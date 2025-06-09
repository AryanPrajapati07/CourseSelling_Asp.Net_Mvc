using Demo.Services;
using Microsoft.EntityFrameworkCore;
using DinkToPdf;
using DinkToPdf.Contracts;
using MathNet.Numerics;
using FluentAssertions.Common;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMemoryCache();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();




var context = new CustomAssemblyLoadContext();
string path = Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox", "libwkhtmltox.dll");
context.LoadUnmanagedLibrary(path);

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));



builder.Services.AddScoped<InvoiceService>();
//builder.Services.AddControllersWithViews();
// Add this with your other service registrations
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSession();

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});
//builder.Services.AddTransient<EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app..AddMemoryCache();
app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseSession(); // Moved this line after app is declared

app.UseRouting();


app.UseAuthorization();
app.MapControllers();





// Ensure invoices directory exists
var invoicesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");
if (!Directory.Exists(invoicesDir))
{
    Directory.CreateDirectory(invoicesDir);
}


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
