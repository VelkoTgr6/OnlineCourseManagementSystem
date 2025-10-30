using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OnlineCourseManagementSystem.Core.Models.Student
{
    public class StudentCourseEnrollmentUpdateFormModel
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }

        [Range(0, 100)]
        [Comment("Progress percentage from 0 to 100")]
        public int ProgressPercentage { get; set; }
    }
}
