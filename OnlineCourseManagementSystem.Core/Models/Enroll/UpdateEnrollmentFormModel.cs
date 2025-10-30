using System.ComponentModel.DataAnnotations;

namespace OnlineCourseManagementSystem.Core.Models.Enroll
{
    public class UpdateEnrollmentFormModel
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Range(0, 100)]
        public int Progress { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}
