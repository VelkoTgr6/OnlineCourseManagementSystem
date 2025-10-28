namespace OnlineCourseManagementSystem.Api.Middleware
{
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string ErrorType { get; set; } = string.Empty;
    }
}
