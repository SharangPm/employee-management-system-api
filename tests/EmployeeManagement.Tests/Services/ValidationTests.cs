using System.ComponentModel.DataAnnotations;
using EmployeeManagement.API.DTOs.Auth;
using EmployeeManagement.API.DTOs.Employee;
using Xunit;

namespace EmployeeManagement.Tests.Services;

public sealed class ValidationTests
{
    [Fact]
    public void EmployeeRequestRejectsInvalidEmailAndNegativeSalary()
    {
        var request = new EmployeeRequest("Sam", "Example", "invalid-email", null,
            "Engineering", "Developer", -1m, DateTime.UtcNow);
        var results = Validate(request);
        Assert.Contains(results, result => result.MemberNames.Contains("Email"));
        Assert.Contains(results, result => result.MemberNames.Contains("Salary"));
    }

    [Fact]
    public void RegisterRequestRejectsInvalidEmail()
    {
        var results = Validate(new RegisterRequest("bad-email", "Strong-password-123!"));
        Assert.Contains(results, result => result.MemberNames.Contains("Email"));
    }

    private static List<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }
}
