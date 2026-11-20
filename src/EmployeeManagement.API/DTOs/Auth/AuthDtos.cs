using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs.Auth;

public sealed record RegisterRequest(
    [param: Required, EmailAddress, MaxLength(254)] string Email,
    [param: Required, RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{12,128}$", ErrorMessage = "Password must be 12 to 128 characters and include upper, lower, numeric, and special characters.")] string Password);

public sealed record LoginRequest(
    [param: Required, EmailAddress, MaxLength(254)] string Email,
    [param: Required, MinLength(1), MaxLength(128)] string Password);

public sealed record CreateAdminRequest(
    [param: Required, EmailAddress, MaxLength(254)] string Email,
    [param: Required, RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{12,128}$", ErrorMessage = "Password must be 12 to 128 characters and include upper, lower, numeric, and special characters.")] string Password);

public sealed record LinkEmployeeAccountRequest(
    [param: Required, EmailAddress, MaxLength(254)] string Email);

public sealed record AuthResponse(string AccessToken, string Email, string Role, DateTime ExpiresAt);
