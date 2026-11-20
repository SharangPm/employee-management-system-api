using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.Controllers;

public sealed class EmployeeControllerTests
{
    private const string EmployeeId = "507f1f77bcf86cd799439011";

    [Fact]
    public async Task AnonymousVisitorCanReadEmployeeById()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(item => item.GetAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = EmployeeId, Email = "person@example.com" });
        var controller = new EmployeeController(service.Object);

        var result = await controller.Get(EmployeeId, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        service.Verify(item => item.GetAsync(EmployeeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void EmployeeEndpointsHaveNoAuthorizationRequirements()
    {
        var controllerType = typeof(EmployeeController);
        Assert.Empty(controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));

        foreach (var action in new[] { "List", "Get", "Create", "Update", "Delete" })
        {
            var method = controllerType.GetMethod(action)!;
            Assert.Empty(method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
        }
    }
}



