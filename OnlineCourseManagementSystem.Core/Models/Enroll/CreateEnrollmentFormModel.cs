using System.ComponentModel.DataAnnotations;

namespace OnlineCourseManagementSystem.Core.Models.Enroll
{
    public class CreateEnrollmentFormModel
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

    }
}
