using Microsoft.AspNetCore.Mvc;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Models.Course;

namespace OnlineCourseManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ILogger<CourseController> logger;
        private readonly ICourseService courseService;

        public CourseController(ICourseService _courseService)
        {
            courseService = _courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await courseService.GetAllAsync();

            logger.LogInformation("Retrieved all courses. Count: {Count}", courses.Count());

            return Ok(courses);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await courseService.CreateAsync(model);
            return Ok();
                
        }
    }
}
