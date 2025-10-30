using Microsoft.AspNetCore.Mvc;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Models.Student;
using OnlineCourseManagementSystem.Infrastructure.Data.Models;

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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await studentService.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return Ok(student);
        }

        [HttpGet("{studentId:int}/courses")]
        public async Task<IActionResult> GetStudentCourses([FromRoute] int studentId)
        {
            var courses = await studentService.GetStudentCoursesAsync(studentId);

            if (courses == null)
            {
                return NotFound();
            }

            logger.LogInformation($"Retrieved courses for student ID {studentId}. Count: {courses.Count()}");
            return Ok(courses);
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
        public async Task<IActionResult> UpdateStudent([FromRoute] int id,[FromBody] CreateStudentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var studentId = await studentService.UpdateAsync(id,model);
            logger.LogInformation($"Updated student with ID {studentId}");

            return Ok(new { Message = "Student updated successfully", Id = studentId });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStudentCourseEnrollment([FromBody] StudentCourseEnrollmentUpdateFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
 
            var studentId = await studentService.CourseEnrollmentUpdate(model);
            logger.LogInformation($"Updated student's course with ID {studentId}");
            return Ok(new { Message = "Updated student's course successfully", Id = studentId });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {

            await studentService.DeleteAsync(id);
            logger.LogInformation($"Deleted student with ID {id}");
            return NoContent();
        }
    }
}
