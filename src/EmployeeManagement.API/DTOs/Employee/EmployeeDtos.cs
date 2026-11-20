using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.Employee;

public sealed record EmployeeRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string FirstName,
    [property: Required, StringLength(100, MinimumLength = 1)] string LastName,
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Phone, StringLength(30)] string? PhoneNumber,
    [property: Required, StringLength(100, MinimumLength = 1)] string Department,
    [property: Required, StringLength(100, MinimumLength = 1)] string Designation,
    [property: Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal Salary,
    [property: Required] DateTime? DateOfJoining,
    bool IsActive = true);

public sealed record EmployeeResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Department,
    string Designation,
    decimal Salary,
    DateTime DateOfJoining,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total);

