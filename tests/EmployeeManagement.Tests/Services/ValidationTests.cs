using System.ComponentModel.DataAnnotations;
using System.Reflection;
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

        foreach (var parameter in value.GetType().GetConstructors().Single().GetParameters())
        {
            var property = value.GetType().GetProperty(
                parameter.Name!,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var memberName = property?.Name ?? parameter.Name!;
            var memberValue = property?.GetValue(value);
            var context = new ValidationContext(value) { MemberName = memberName };

            foreach (var attribute in parameter.GetCustomAttributes<ValidationAttribute>())
            {
                var result = attribute.GetValidationResult(memberValue, context);
                if (result is not null && result != ValidationResult.Success)
                {
                    results.Add(result);
                }
            }
        }

        return results;
    }
}
