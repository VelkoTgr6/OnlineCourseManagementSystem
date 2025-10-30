# Online Course Management System

A **.NET 8** Web API built with **ASP.NET Core** for managing **Courses**, **Students**, and **Enrollments**. This project utilizes **Entity Framework Core** for data management, follows the **Repository Pattern**, and includes unit testing with **NUnit** and **Moq**. It also integrates structured **logging** using **Microsoft.Extensions.Logging** for detailed traceability.

## Tech Stack

- **.NET 8**, **C# 12**
- **ASP.NET Core Web API**
- **Entity Framework Core** (EF Core)
- **NUnit** and **Moq** (Unit Testing)
- **Swagger/OpenAPI** (API documentation)
- **Microsoft.Extensions.Logging** (Logging)

## Features

- **Course Management**: Create, update, delete, and retrieve courses.
- **Student Management**: Manage student enrollment and personal information.
- **Enrollment Management**: Students can enroll in courses while respecting business rules like enrollment caps and valid dates.
- **Soft Deletes**: Entities support "soft delete" functionality via the `IsDeleted` field.
- **Logging**: Information and warning level logs are captured for key operations and errors.

## Project Structure

```plaintext
OnlineCourseManagementSystem.Api/
  Controllers/
    - CourseController.cs
    - StudentController.cs

OnlineCourseManagementSystem.Core/
  Services/
    - CourseService.cs
    - EnrollmentService.cs
  Models/
    Enroll/
      - CreateEnrollmentFormModel.cs
      - EnrollStudentFormModel.cs
      - UpdateEnrollmentFormModel.cs
      - EnrollViewModel.cs

OnlineCourseManagementSystem.Infrastructure/
  Data/
    Models/
      - Student.cs
      - Course.cs
      - Enrollment.cs

UnitTests/
  - CourseServiceTests.cs
  - StudentServiceTests.cs
  - EnrollmentServiceTests.cs
