using Microsoft.AspNetCore.Mvc;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Models.Student;

namespace OnlineCourseManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ILogger<StudentController> logger;
        private readonly IStudentService studentService;

        public StudentController(IStudentService _studentService, ILogger<StudentController> _logger)
        {
            studentService = _studentService;
            this.logger = _logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await studentService.GetAllAsync();

            logger.LogInformation($"Retrieved all students. Count: {students.Count()}");

            return Ok(students);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await studentService.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return Ok(student);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var studentId = await studentService.CreateAsync(model);
            logger.LogInformation($"Created new student with ID {studentId}");
            return CreatedAtAction(nameof(GetById), new { id = studentId }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent([FromBody] UpdateStudentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var studentId = await studentService.UpdateAsync(model);
                logger.LogInformation($"Updated student with ID {studentId}");
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                await studentService.DeleteAsync(id);
                logger.LogInformation($"Deleted student with ID {id}");
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return NotFound();
            }
        }
    }
}
