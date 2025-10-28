using Microsoft.AspNetCore.Mvc;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Models.Course;

namespace OnlineCourseManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService courseService;
        private readonly ILogger<CourseController> logger;

        public CourseController(ICourseService _courseService, ILogger<CourseController> _logger)
        {
            courseService = _courseService;
            logger = _logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await courseService.GetAllAsync();

            logger.LogInformation($"Retrieved {courses.Count()} courses");

            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await courseService.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var courseId = await courseService.CreateAsync(model);

            logger.LogInformation($"Created new course with ID {courseId}");

            return CreatedAtAction(nameof(GetById), new { id = courseId }, model);
        }
    }
}
