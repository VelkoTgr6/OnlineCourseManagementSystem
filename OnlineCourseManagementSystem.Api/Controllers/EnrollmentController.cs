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

        [HttpGet("{id}")]
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
        public async Task<IActionResult> CreateEnrollment([FromBody] CreateEnrollmentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var enrollmentId = await enrollmentService.CreateAsync(model);

            logger.LogInformation($"Created new course with ID {enrollmentId}");

            return CreatedAtAction(nameof(GetById), new { id = enrollmentId }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrollement([FromBody] UpdateEnrollmentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var enrollmentId = await enrollmentService.UpdateEnrollmentAsync(model);
                logger.LogInformation($"Updated course with ID {enrollmentId}");
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return NotFound();
            }
        }

        [HttpPut("{id}/progress")]
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


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            try
            {
                await enrollmentService.DeleteAsync(id);
                logger.LogInformation($"Deleted course with ID {id}");
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
