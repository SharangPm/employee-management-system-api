using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using EmployeeManagement.API.Services;

namespace EmployeeManagement.API.Middleware;

public sealed record ApiErrorResponse(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Errors,
    string TraceId);

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "Request cancelled by caller. Method: {Method}, Path: {Path}, TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (status, code, message) = MapException(exception);
            if (status >= HttpStatusCode.InternalServerError)
            {
                logger.LogError(exception, "Unhandled request failure. TraceId: {TraceId}", context.TraceIdentifier);
            }
            else
            {
                logger.LogInformation("Request rejected with {Code}. TraceId: {TraceId}", code, context.TraceIdentifier);
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                new ApiErrorResponse(code, message, null, context.TraceIdentifier),
                JsonOptions));
        }
    }

    private static (HttpStatusCode Status, string Code, string Message) MapException(Exception exception) => exception switch
    {
        InvalidCredentialsException => (HttpStatusCode.Unauthorized, "invalid_credentials", "Email or password is invalid."),
        UnauthorizedAccessException => (HttpStatusCode.Forbidden, "forbidden", "You are not allowed to access this resource."),
        DuplicateEmailException => (HttpStatusCode.Conflict, "duplicate_email", exception.Message),
        EmployeeProfileConflictException => (HttpStatusCode.Conflict, "profile_link_conflict", exception.Message),
        MongoDB.Driver.MongoWriteException writeException
            when writeException.WriteError.Category == MongoDB.Driver.ServerErrorCategory.DuplicateKey =>
                (HttpStatusCode.Conflict, "duplicate_email", "An account or employee with this email already exists."),
        MongoDB.Driver.MongoException =>
            (HttpStatusCode.ServiceUnavailable, "database_unavailable", "The database is temporarily unavailable."),
        TimeoutException or OperationCanceledException =>
            (HttpStatusCode.ServiceUnavailable, "dependency_timeout", "A required service did not respond in time."),
        KeyNotFoundException => (HttpStatusCode.NotFound, "not_found", exception.Message),
        ArgumentException => (HttpStatusCode.BadRequest, "invalid_request", exception.Message),
        ValidationException => (HttpStatusCode.BadRequest, "validation_failed", exception.Message),
        _ => (HttpStatusCode.InternalServerError, "internal_error", "An unexpected error occurred.")
    };
}


