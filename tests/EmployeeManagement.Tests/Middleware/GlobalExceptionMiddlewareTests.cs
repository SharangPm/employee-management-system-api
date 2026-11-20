using System.Text.Json;
using EmployeeManagement.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EmployeeManagement.Tests.Middleware;

public sealed class GlobalExceptionMiddlewareTests
{
    [Fact]
    public async Task DuplicateEmailReturnsConsistentConflictError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new EmployeeManagement.API.Services.DuplicateEmailException("Email already exists."),
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        var response = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(context.Response.Body);
        Assert.Equal("duplicate_email", response!.Code);
        Assert.Equal("Email already exists.", response.Message);
    }

    [Fact]
    public async Task UnexpectedFailureDoesNotExposeInternalDetails()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new InvalidOperationException("secret connection string"),
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("secret connection string", body);
    }
}
