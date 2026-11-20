using System.Security.Claims;
using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.Controllers;

public sealed class EmployeeControllerTests
{
    private const string EmployeeId = "507f1f77bcf86cd799439011";

    [Fact]
    public async Task EmployeeCanReadTheirOwnProfile()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(item => item.GetByUserIdAsync("user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = EmployeeId, Email = "person@example.com", LinkedUserId = "user-123" });
        var controller = CreateController(service, "Employee", "person@example.com");

        var result = await controller.GetMyProfile(CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        service.Verify(item => item.GetByUserIdAsync("user-123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EmployeeCannotReadAnotherEmployeesProfile()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(item => item.GetAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = EmployeeId, Email = "other@example.com", LinkedUserId = "other-user" });
        var controller = CreateController(service, "Employee", "person@example.com");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.Get(EmployeeId, CancellationToken.None));
    }

    [Fact]
    public async Task AdminCanReadAnyEmployeesProfile()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(item => item.GetAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = EmployeeId, Email = "other@example.com", LinkedUserId = "other-user" });
        var controller = CreateController(service, "Admin", "admin@example.com");

        var result = await controller.Get(EmployeeId, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void EmployeeCollectionIsAdminOnly()
    {
        var method = typeof(EmployeeController).GetMethod(nameof(EmployeeController.List))!;
        var authorization = method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single();
        Assert.Equal("Admin", authorization.Roles);
    }

    private static EmployeeController CreateController(Mock<IEmployeeService> service, string role, string email)
    {
        var controller = new EmployeeController(service.Object);
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role), new Claim(ClaimTypes.NameIdentifier, role == "Employee" ? "user-123" : "admin-123"), new Claim(ClaimTypes.Email, email) }, "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }
}



