using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeManagement.API.Configuration;
using EmployeeManagement.API.DTOs.Auth;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EmployeeManagement.API.Services;

public sealed class AuthService(IUserRepository users, IEmployeeRepository employees, IOptions<JwtSettings> settings) : IAuthService
{
    private readonly PasswordHasher<User> passwordHasher = new();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (await UserExistsAsync(email, cancellationToken))
        {
            throw new DuplicateEmailException("An account with this email already exists.");
        }

        var user = new User { Email = email, Role = "Employee" };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        try
        {
            await users.CreateAsync(user, cancellationToken);
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new DuplicateEmailException("An account with this email already exists.");
        }

        return CreateToken(user);
    }

    public async Task<AuthResponse> CreateAdminAsync(CreateAdminRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (await UserExistsAsync(email, cancellationToken))
        {
            throw new DuplicateEmailException("An account with this email already exists.");
        }

        var user = new User { Email = email, Role = "Admin" };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        try
        {
            await users.CreateAsync(user, cancellationToken);
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new DuplicateEmailException("An account with this email already exists.");
        }

        return CreateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException();
        }

        return CreateToken(user);
    }

    public async Task LinkEmployeeAccountAsync(string employeeId, string email, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(employeeId, out _))
        {
            throw new ArgumentException("Employee ID must be a valid MongoDB ObjectId.");
        }

        var user = await users.FindByEmailAsync(NormalizeEmail(email), cancellationToken)
            ?? throw new KeyNotFoundException("The employee account was not found.");
        if (user.Role != "Employee")
        {
            throw new ArgumentException("Only Employee accounts can be linked to employee profiles.");
        }
        if (!await employees.LinkUserAsync(employeeId, user.Id!, cancellationToken))
        {
            throw new EmployeeProfileConflictException("The profile is already linked, or the employee ID does not exist.");
        }
    }
    public async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken) =>
        await users.FindByEmailAsync(NormalizeEmail(email), cancellationToken) is not null;

    private AuthResponse CreateToken(User user)
    {
        var jwt = settings.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, expires: expiresAt, signingCredentials: credentials);
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), user.Email, user.Role, expiresAt);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

public sealed class InvalidCredentialsException() : Exception("Email or password is invalid.");
public sealed class EmployeeProfileConflictException(string message) : Exception(message);










