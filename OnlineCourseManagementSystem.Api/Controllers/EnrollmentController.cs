using Microsoft.AspNetCore.Mvc;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Models.Enroll;

namespace OnlineCourseManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly ILogger<EnrollmentController> logger;
        private readonly IEnrollmentService enrollmentService;

        public EnrollmentController(ILogger<EnrollmentController> _logger, IEnrollmentService _enrollmentService)
        {
            logger = _logger;
            enrollmentService = _enrollmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() 
        {
            var enrolls = await enrollmentService.GetAllAsync();

            logger.LogInformation($"Retrieved {enrolls.Count()} courses");

            return Ok(enrolls);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var enrollment = await enrollmentService.GetByIdAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }
            return Ok(enrollment);
        }

        [HttpPost] 
        public async Task<IActionResult> CreateEnrollment([FromBody] EnrollStudentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var enrollmentId = await enrollmentService.EnrollStudentAsync(model);

            logger.LogInformation($"Created new enrollment with ID {enrollmentId}");

            return CreatedAtAction(nameof(GetById), new { id = enrollmentId }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEnrollement([FromRoute] int id,[FromBody] UpdateEnrollmentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var enrollmentId = await enrollmentService.UpdateEnrollmentAsync(id,model);
            logger.LogInformation($"Updated enrollment with ID {enrollmentId}");
                
            return Ok(new { Message = "Enrollment updated succesfully", Id = enrollmentId});
        }

        [HttpPut("{id:int}/progress")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] int progress)
        {
            try
            {
                await enrollmentService.UpdateProgressAsync(id, progress);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return NotFound();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            await enrollmentService.DeleteAsync(id);
            logger.LogInformation($"Deleted enrollment with ID {id}");
            return NoContent();
        }
    }
}
