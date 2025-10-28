namespace OnlineCourseManagementSystem.Core.Models.Enroll
{
    public class EnrollViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public int Progress { get; set; }
        public bool IsCompleted { get; set; }
    }
}
