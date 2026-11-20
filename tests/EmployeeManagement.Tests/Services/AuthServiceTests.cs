using EmployeeManagement.API.Configuration;
using EmployeeManagement.API.DTOs.Auth;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.Services;

public sealed class AuthServiceTests
{
    private static AuthService CreateService(Mock<IUserRepository> users) =>
        new(users.Object, Mock.Of<IEmployeeRepository>(), Options.Create(new JwtSettings(
            "test-secret-for-unit-tests-12345678901234567890", "test-issuer", "test-audience", 30)));

    [Fact]
    public async Task RegisterCreatesEmployeeRoleAndReturnsToken()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.FindByEmailAsync("worker@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        users.Setup(repository => repository.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => { user.Id = "507f1f77bcf86cd799439011"; return user; });

        var response = await CreateService(users).RegisterAsync(
            new RegisterRequest(" Worker@Example.com ", "A-very-strong-password-123!"), CancellationToken.None);

        Assert.Equal("Employee", response.Role);
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        users.Verify(repository => repository.CreateAsync(
            It.Is<User>(user => user.Email == "worker@example.com" && user.Role == "Employee"
                && user.PasswordHash != "A-very-strong-password-123!"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginWithWrongPasswordThrowsInvalidCredentials()
    {
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var user = new User { Id = "507f1f77bcf86cd799439011", Email = "worker@example.com", Role = "Employee" };
        user.PasswordHash = hasher.HashPassword(user, "correct-password");
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.FindByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => CreateService(users)
            .LoginAsync(new LoginRequest(user.Email, "wrong-password"), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAdminAssignsAdminRoleAndHashesPassword()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.FindByEmailAsync("admin@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        users.Setup(repository => repository.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => { user.Id = "507f1f77bcf86cd799439011"; return user; });

        var response = await CreateService(users).CreateAdminAsync(
            new CreateAdminRequest("admin@example.com", "A-different-password-123!"), CancellationToken.None);

        Assert.Equal("Admin", response.Role);
        users.Verify(repository => repository.CreateAsync(
            It.Is<User>(user => user.Role == "Admin" && user.PasswordHash != "A-different-password-123!"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterRejectsDuplicateAccount()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.FindByEmailAsync("person@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "person@example.com" });

        await Assert.ThrowsAsync<DuplicateEmailException>(() => CreateService(users)
            .RegisterAsync(new RegisterRequest("person@example.com", "A-very-strong-password-123!"), CancellationToken.None));
    }
}




