using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.Employee;

public sealed record EmployeeRequest(
    [param: Required, StringLength(100, MinimumLength = 1)] string FirstName,
    [param: Required, StringLength(100, MinimumLength = 1)] string LastName,
    [param: Required, EmailAddress, StringLength(254)] string Email,
    [param: Phone, StringLength(30)] string? PhoneNumber,
    [param: Required, StringLength(100, MinimumLength = 1)] string Department,
    [param: Required, StringLength(100, MinimumLength = 1)] string Designation,
    [param: Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal Salary,
    [param: Required] DateTime? DateOfJoining,
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

