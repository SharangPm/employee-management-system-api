using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetAsync(string id, CancellationToken cancellationToken);
    Task<Employee?> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<bool> LinkUserAsync(string employeeId, string userId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Employee> Items, long Total)> ListAsync(
        string? search, string? department, bool? active, int page, int pageSize,
        CancellationToken cancellationToken);
    Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(string id, Employee employee, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, string? exceptId, CancellationToken cancellationToken);
}

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken);
}

public interface IEmployeeService
{
    Task<Employee> GetAsync(string id, CancellationToken cancellationToken);
    Task<Employee> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Employee> Items, long Total)> ListAsync(
        string? search, string? department, bool? active, int page, int pageSize,
        CancellationToken cancellationToken);
    Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken);
    Task<Employee> UpdateAsync(string id, Employee employee, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
}

public interface IAuthService
{
    Task<EmployeeManagement.API.DTOs.Auth.AuthResponse> RegisterAsync(
        EmployeeManagement.API.DTOs.Auth.RegisterRequest request, CancellationToken cancellationToken);
    Task<EmployeeManagement.API.DTOs.Auth.AuthResponse> LoginAsync(
        EmployeeManagement.API.DTOs.Auth.LoginRequest request, CancellationToken cancellationToken);
    Task<EmployeeManagement.API.DTOs.Auth.AuthResponse> CreateAdminAsync(
        EmployeeManagement.API.DTOs.Auth.CreateAdminRequest request, CancellationToken cancellationToken);
    Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken);
    Task LinkEmployeeAccountAsync(string employeeId, string email, CancellationToken cancellationToken);
}

