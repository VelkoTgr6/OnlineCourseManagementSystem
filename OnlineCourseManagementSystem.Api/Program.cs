
using OnlineCourseManagementSystem.Api.Middleware;
using OnlineCourseManagementSystem.Core.Contracts;
using OnlineCourseManagementSystem.Core.Services;
using OnlineCourseManagementSystem.Infrastructure.Data.Common;

namespace OnlineCourseManagementSystem.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IRepository, Repository>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            //builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();


            var app = builder.Build();

            app.UseMiddleware<ErrorHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
