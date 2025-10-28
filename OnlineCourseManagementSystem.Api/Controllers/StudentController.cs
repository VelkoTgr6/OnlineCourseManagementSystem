using Microsoft.AspNetCore.Mvc;
using OnlineCourseManagementSystem.Core.Contracts;

namespace OnlineCourseManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ILogger<CourseController> logger;
        private readonly IStudentService studentService;

        public StudentController(IStudentService _studentService)
        {
            studentService = _studentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await studentService.GetAllAsync();

            logger.LogInformation("Retrieved all students. Count: {Count}", students.Count());

            return Ok(students);
        }
    }
}
