using Microsoft.AspNetCore.Mvc;
using Demo.Models;
using Microsoft.EntityFrameworkCore;
using Demo.Services;

namespace Demo.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesApiController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment env;

        public CoursesApiController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            this.context = context;
            this.env = env;
        }

        [HttpPost]
        [RequestSizeLimit(50_000_000)] // 50 MB limit (adjust as needed)
        public async Task<IActionResult> AddCourse([FromForm] Course course)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Image Upload
            if (course.ImageFile != null && course.ImageFile.Length > 0)
            {
                var imageFileName = Guid.NewGuid() + Path.GetExtension(course.ImageFile.FileName);
                var imagePath = Path.Combine(env.WebRootPath, "uploads/images", imageFileName);

                Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await course.ImageFile.CopyToAsync(stream);
                }

                course.ImagePath = "/uploads/images/" + imageFileName;
            }

            // Video Upload
            if (course.Video != null && course.Video.Length > 0)
            {
                var videoFileName = Guid.NewGuid() + Path.GetExtension(course.Video.FileName);
                var videoPath = Path.Combine(env.WebRootPath, "uploads/videos", videoFileName);

                Directory.CreateDirectory(Path.GetDirectoryName(videoPath)!);

                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    await course.Video.CopyToAsync(stream);
                }

                course.VideoPath = "/uploads/videos/" + videoFileName;
            }

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            return Ok(new { message = "Course created successfully", course.Id });
        }
    }
}
