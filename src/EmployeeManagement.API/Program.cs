using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EmployeeManagement.API.Configuration;
using EmployeeManagement.API.DTOs.Auth;
using EmployeeManagement.API.Extensions;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Middleware;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;
var corsOrigins = (builder.Configuration["CORS_ALLOWED_ORIGINS"] ?? string.Empty)
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
if (environment.IsProduction() && corsOrigins.Length == 0)
{
    throw new InvalidOperationException("CORS_ALLOWED_ORIGINS must contain the production frontend origin.");
}

if (environment.IsProduction() && corsOrigins.Any(origin => !Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || uri.AbsolutePath != "/"))
{
    throw new InvalidOperationException("Production CORS origins must be exact HTTPS origins without paths.");
}

var mongoUri = builder.Configuration["MONGODB_URI"] ?? string.Empty;
if (environment.IsProduction() && !(mongoUri.StartsWith("mongodb+srv://", StringComparison.OrdinalIgnoreCase) || mongoUri.Contains("tls=true", StringComparison.OrdinalIgnoreCase) || mongoUri.Contains("ssl=true", StringComparison.OrdinalIgnoreCase)))
{
    throw new InvalidOperationException("Production MongoDB connections must use TLS (use an Atlas mongodb+srv URI).");
}
var initialAdminEmail = builder.Configuration["INITIAL_ADMIN_EMAIL"];
var initialAdminPassword = builder.Configuration["INITIAL_ADMIN_PASSWORD"];
if (environment.IsProduction() && (!string.IsNullOrEmpty(initialAdminEmail) || !string.IsNullOrEmpty(initialAdminPassword)))
{
    throw new InvalidOperationException("Initial Admin bootstrap credentials must never be configured in production.");
}
if (environment.IsDevelopment() &&
    (string.IsNullOrWhiteSpace(initialAdminEmail) != string.IsNullOrWhiteSpace(initialAdminPassword)))
{
    throw new InvalidOperationException("Set both INITIAL_ADMIN_EMAIL and INITIAL_ADMIN_PASSWORD for development bootstrap.");
}
if (environment.IsDevelopment() && !string.IsNullOrWhiteSpace(initialAdminPassword) && (initialAdminPassword.Contains("CHANGE_ME", StringComparison.Ordinal) || !new EmailAddressAttribute().IsValid(initialAdminEmail) || initialAdminPassword.Length < 12 || !Regex.IsMatch(initialAdminPassword, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{12,128}$")))
{
    throw new InvalidOperationException("INITIAL_ADMIN_PASSWORD must be 12 to 128 characters and include upper, lower, numeric, and special characters.");
}

if (!int.TryParse(builder.Configuration["JWT_EXPIRY_MINUTES"], out var jwtExpiryMinutes))
{
    throw new InvalidOperationException("JWT_EXPIRY_MINUTES must be configured as a number.");
}
var jwt = new JwtSettings(builder.Configuration["JWT_SECRET"]!, builder.Configuration["JWT_ISSUER"]!, builder.Configuration["JWT_AUDIENCE"]!, jwtExpiryMinutes);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Email
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiErrorResponse(
                    "unauthorized", "Authentication is required or the bearer token is invalid.", null,
                    context.HttpContext.TraceIdentifier)));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiErrorResponse(
                    "forbidden", "You are not allowed to access this resource.", null,
                    context.HttpContext.TraceIdentifier)));
            }
        };
    });
var trustedProxyIps = (builder.Configuration["TRUSTED_PROXY_IPS"] ?? string.Empty)
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(IPAddress.Parse)
    .ToArray();
if (environment.IsProduction() && trustedProxyIps.Length == 0)
{
    throw new InvalidOperationException("Configure TRUSTED_PROXY_IPS with exact trusted ingress IPs in Production.");
}
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = trustedProxyIps.Length == 0
? ForwardedHeaders.None
: ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    foreach (var proxyIp in trustedProxyIps)
    {
        options.KnownProxies.Add(proxyIp);
    }
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    if (corsOrigins.Length > 0)
    {
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
    }
}));
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The value is invalid." : error.ErrorMessage).ToArray());
        return new ObjectResult(new ApiErrorResponse(
            "validation_failed", "One or more validation errors occurred.", errors, context.HttpContext.TraceIdentifier))
        { StatusCode = StatusCodes.Status400BadRequest };
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Employee Management API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    options.AddSecurityRequirement(new()
    {
        [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
});

var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseForwardedHeaders();
app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;
    var (code, message) = response.StatusCode switch
    {
        StatusCodes.Status401Unauthorized => ("unauthorized", "Authentication is required."),
        StatusCodes.Status403Forbidden => ("forbidden", "You are not allowed to access this resource."),
        StatusCodes.Status404NotFound => ("not_found", "The requested resource was not found."),
        StatusCodes.Status405MethodNotAllowed => ("method_not_allowed", "The HTTP method is not supported for this resource."),
        _ => ("request_failed", "The request could not be completed.")
    };
    response.ContentType = "application/json";
    await response.WriteAsync(JsonSerializer.Serialize(new ApiErrorResponse(
        code, message, null, statusContext.HttpContext.TraceIdentifier)));
});
if (environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseCors("Frontend");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

var database = app.Services.GetRequiredService<IMongoDatabase>();
try
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1), cancellationToken: timeout.Token);
    await database.GetCollection<Employee>("employees").Indexes.CreateOneAsync(
        new CreateIndexModel<Employee>(Builders<Employee>.IndexKeys.Ascending(employee => employee.Email), new CreateIndexOptions { Unique = true, Name = "ux_employee_email" }),
        cancellationToken: timeout.Token);
    await database.GetCollection<User>("users").Indexes.CreateOneAsync(
        new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(user => user.Email), new CreateIndexOptions { Unique = true, Name = "ux_user_email" }),
        cancellationToken: timeout.Token);
    await database.GetCollection<Employee>("employees").Indexes.CreateOneAsync(
        new CreateIndexModel<Employee>(Builders<Employee>.IndexKeys.Ascending(employee => employee.LinkedUserId), new CreateIndexOptions { Unique = true, Sparse = true, Name = "ux_employee_linked_user" }),
        cancellationToken: timeout.Token);
}
catch (Exception)
{
    app.Logger.LogCritical("MongoDB is unavailable or required indexes could not be initialized.");
    throw new InvalidOperationException("Could not connect to MongoDB or initialize required indexes. Verify MONGODB_URI, network access, and database permissions.");
}

if (args.Contains("--provision-admin", StringComparer.Ordinal))
{
    if (!environment.IsProduction())
    {
        throw new InvalidOperationException("The Admin provisioning command is only available in Production.");
    }

    var email = builder.Configuration["ADMIN_PROVISIONING_EMAIL"]?.Trim().ToLowerInvariant();
    var password = builder.Configuration["ADMIN_PROVISIONING_PASSWORD"];
    if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email) || string.IsNullOrWhiteSpace(password) || password.Contains("CHANGE_ME", StringComparison.Ordinal) ||
        password.Length < 12 || !Regex.IsMatch(password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{12,128}$"))
    {
        throw new InvalidOperationException("Provide ADMIN_PROVISIONING_EMAIL and a strong ADMIN_PROVISIONING_PASSWORD for this one-time command.");
    }

    var userCollection = database.GetCollection<User>("users");
    if (await userCollection.Find(user => user.Role == "Admin").AnyAsync())
    {
        throw new InvalidOperationException("An Admin already exists; provisioning is only for the first Admin account.");
    }

    var lockCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("system_bootstrap");
    try
    {
        await lockCollection.InsertOneAsync(new MongoDB.Bson.BsonDocument("_id", "initial-admin-provisioned"));
    }
    catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
    {
        throw new InvalidOperationException("Initial Admin provisioning has already been claimed.");
    }

    try
    {
        var user = new User { Email = email, Role = "Admin" };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        await userCollection.InsertOneAsync(user);
        app.Logger.LogInformation("Initial production Admin provisioned for {AdminEmail}.", email);
        return;
    }
    catch
    {
        await lockCollection.DeleteOneAsync(document => document["_id"] == "initial-admin-provisioned");
        throw;
    }
}
if (environment.IsDevelopment() && !string.IsNullOrWhiteSpace(initialAdminEmail))
{
    using var scope = app.Services.CreateScope();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    if (!await authService.UserExistsAsync(initialAdminEmail, CancellationToken.None))
    {
        await authService.CreateAdminAsync(new CreateAdminRequest(initialAdminEmail, initialAdminPassword!), CancellationToken.None);
        app.Logger.LogInformation("Development Admin account bootstrapped for {AdminEmail}.", initialAdminEmail);
    }
}

app.MapControllers();
app.Run();
























