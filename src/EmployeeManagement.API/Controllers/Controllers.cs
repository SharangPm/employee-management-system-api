using System.Security.Claims;
using EmployeeManagement.API.DTOs.Employee;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Middleware;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public sealed class EmployeeController(IEmployeeService service) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<EmployeeResponse>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("The token has no account ID.");
        return Ok(Map(await service.GetByUserIdAsync(userId, cancellationToken)));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResponse<EmployeeResponse>>> List(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] bool? active,
        [FromQuery, System.ComponentModel.DataAnnotations.Range(1, 1000000)] int page = 1,
        [FromQuery, System.ComponentModel.DataAnnotations.Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await service.ListAsync(search, department, active, page, pageSize, cancellationToken);
        return Ok(new PagedResponse<EmployeeResponse>(items.Select(Map).ToList(), page, pageSize, total));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeResponse>> Get(string id, CancellationToken cancellationToken)
    {
        var employee = await service.GetAsync(id, cancellationToken);
        EnsureAdminOrOwnProfile(employee);
        return Ok(Map(employee));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeResponse>> Create(EmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await service.CreateAsync(ToEntity(request), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = employee.Id }, Map(employee));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeResponse>> Update(
        string id, EmployeeRequest request, CancellationToken cancellationToken) =>
        Ok(Map(await service.UpdateAsync(id, ToEntity(request), cancellationToken)));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private void EnsureAdminOrOwnProfile(Employee employee)
    {
        if (User.IsInRole("Admin"))
        {
            return;
        }

        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId is null || !string.Equals(accountId, employee.LinkedUserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("You can only access your own employee profile.");
        }
    }

    private static Employee ToEntity(EmployeeRequest request) => new()
    {
        FirstName = request.FirstName.Trim(),
        LastName = request.LastName.Trim(),
        Email = request.Email.Trim(),
        PhoneNumber = request.PhoneNumber?.Trim(),
        Department = request.Department.Trim(),
        Designation = request.Designation.Trim(),
        Salary = request.Salary,
        DateOfJoining = request.DateOfJoining!.Value,
        IsActive = request.IsActive
    };

    private static EmployeeResponse Map(Employee employee) => new(
        employee.Id!, employee.FirstName, employee.LastName, employee.Email, employee.PhoneNumber,
        employee.Department, employee.Designation, employee.Salary, employee.DateOfJoining,
        employee.IsActive, employee.CreatedAt, employee.UpdatedAt);
}

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<EmployeeManagement.API.DTOs.Auth.AuthResponse>> Register(
        EmployeeManagement.API.DTOs.Auth.RegisterRequest request, CancellationToken cancellationToken) =>
        Ok(await service.RegisterAsync(request, cancellationToken));

    [HttpPost("login")]
    public async Task<ActionResult<EmployeeManagement.API.DTOs.Auth.AuthResponse>> Login(
        EmployeeManagement.API.DTOs.Auth.LoginRequest request, CancellationToken cancellationToken) =>
        Ok(await service.LoginAsync(request, cancellationToken));

    [HttpPost("admins")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeManagement.API.DTOs.Auth.AuthResponse>> CreateAdmin(
        EmployeeManagement.API.DTOs.Auth.CreateAdminRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await service.CreateAdminAsync(request, cancellationToken));

    [HttpPost("employees/{employeeId}/link-account")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LinkEmployeeAccount(
        string employeeId,
        EmployeeManagement.API.DTOs.Auth.LinkEmployeeAccountRequest request,
        CancellationToken cancellationToken)
    {
        await service.LinkEmployeeAccountAsync(employeeId, request.Email, cancellationToken);
        return NoContent();
    }
}


