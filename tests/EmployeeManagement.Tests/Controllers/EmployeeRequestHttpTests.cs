using System.Net;
using System.Net.Http.Json;
using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.Controllers;

public sealed class EmployeeRequestHttpTests
{
    [Fact]
    public async Task InvalidEmployeeRequestReturnsBadRequestInsteadOfServerError()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(EmployeeController).Assembly);
        builder.Services.AddSingleton(Mock.Of<IEmployeeService>());

        await using var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var response = await client.PostAsJsonAsync("/api/employees", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
