Online Course Management System

An ASP.NET Core Web API application built with .NET 8 and C# 12 to manage Courses, Students, and Enrollments. This project follows the Repository Pattern, uses Entity Framework Core for data access, and includes unit tests with NUnit and Moq. It also integrates structured logging using Microsoft.Extensions.Logging.

Tech Stack

.NET 8, C# 12

ASP.NET Core Web API

Entity Framework Core

NUnit, Moq

Microsoft.Extensions.Logging

Swagger/OpenAPI for API documentation

Project Structure
OnlineCourseManagementSystem.Api/
  Controllers/
    CourseController.cs
    StudentController.cs

OnlineCourseManagementSystem.Core/
  Services/
    CourseService.cs
    EnrollmentService.cs
  Models/
    Enroll/
      CreateEnrollmentFormModel.cs
      EnrollStudentFormModel.cs
      UpdateEnrollmentFormModel.cs
      EnrollViewModel.cs

OnlineCourseManagementSystem.Infrastructure/
  Data/
    Models/
      Student.cs
      Course.cs
      Enrollment.cs

UnitTests/
  CourseServiceTests.cs
  StudentServiceTests.cs
  EnrollmentServiceTests.cs

Domain Highlights

Soft Deletes: Entities (e.g., Student, Course, Enrollment) support soft deletes via the IsDeleted flag.

Enrollment Rules:

Enrollment Cap: Enforce maximum student capacity for courses.

Enrollment Date Validation: Reject enrollments if the enrollment date is after the course start date.

Progress Tracking: Student progress is tracked as a percentage (0-100). The course is marked as Completed when progress reaches 100%.

Logging:

Information level logs on successful operations (e.g., create, update, delete, enroll).

Warning level logs for invalid or missing entities.

Getting Started
Prerequisites

Visual Studio 2022 or newer

.NET SDK 8.0.x

Restore and Build

CLI:

Restore dependencies:

dotnet restore


Build the project:

dotnet build -c Debug


Visual Studio:

Open the solution and select Build > Build Solution.

Run the API

Visual Studio:

Set OnlineCourseManagementSystem.Api as the startup project.

Choose Debug > Start Debugging (or press F5).

CLI:

Run the API:

dotnet run --project OnlineCourseManagementSystem.Api


Swagger UI is available at http://localhost:5000/swagger when the API is running locally.

Running Tests

CLI:

dotnet test


Visual Studio:

Open the Test Explorer and select Test > Run All Tests.

Unit tests use the EF Core InMemory provider for fast, isolated execution and also validate logging behavior.

Architecture Overview

Core.Services: Contains business logic services, such as CourseService and EnrollmentService, that interact with repositories through an IRepository abstraction.

Infrastructure: Contains the EF Core entity models (Student, Course, Enrollment) and the data access layer.

Api.Controllers: Defines the API endpoints exposed to clients (refer to Swagger for available operations).

Logging

Information Logs: Captures all important create, update, delete, and enrollment actions.

Warning Logs: Logs warning messages for invalid operations (e.g., missing or deleted entities).

Repository

GitHub Repository
