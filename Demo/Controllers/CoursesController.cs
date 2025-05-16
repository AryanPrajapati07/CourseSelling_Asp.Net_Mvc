using Microsoft.AspNetCore.Mvc;
using Demo.Models;
using Demo.Services;

namespace Demo.Controllers
{
    public class CoursesController : Controller
    {
        // private readonly ApplicationDbContext context;
        //public CoursesController(ApplicationDbContext context)
        //{
        //    this.context = context;
            
        //}
        public IActionResult Index()
        {
            return View();
        }
    }
}
