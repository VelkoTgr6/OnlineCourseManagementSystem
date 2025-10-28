using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static OnlineCourseManagementSystem.Infrastructure.Constants.ModelConstants;

namespace OnlineCourseManagementSystem.Infrastructure.Data.Models
{
    public class Enrollment
    {
        [Required]
        [Comment("Primary key for the Enrollment entity")]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        [Comment("Navigation property to the associated Student entity")]
        public Student Student { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        [Comment("Navigation property to the associated Course entity")]
        public Course Course { get; set; } = null!;

        [Required]
        [Comment("Date and time when the enrollment was created")]
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        [Range(EnrollmentProgressMin,EnrollmentProgressMax)]
        [Comment("Current progress of the student in the course, as a percentage (0–100)")]
        public int Progress { get; set; } = 0;

        [Comment("Marks whether the student has completed the course")]
        public bool IsCompleted { get; set; } = false;

        [Comment("Show if the enrollment is deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}
