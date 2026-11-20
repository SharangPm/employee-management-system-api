using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using MongoDB.Bson;

namespace EmployeeManagement.API.Services;

public sealed class EmployeeService(IEmployeeRepository repository) : IEmployeeService
{
    public async Task<Employee> GetAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            throw new ArgumentException("Employee ID must be a valid MongoDB ObjectId.");
        }

        return await repository.GetAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Employee was not found.");
    }

    public async Task<Employee> GetByUserIdAsync(string userId, CancellationToken cancellationToken) =>
        await repository.GetByUserIdAsync(userId, cancellationToken)
        ?? throw new KeyNotFoundException("No employee profile is linked to this account.");

    public Task<(IReadOnlyList<Employee> Items, long Total)> ListAsync(
        string? search, string? department, bool? active, int page, int pageSize,
        CancellationToken cancellationToken) =>
        repository.ListAsync(search, department, active, page, pageSize, cancellationToken);

    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken)
    {
        employee.Email = NormalizeEmail(employee.Email);
        if (await repository.EmailExistsAsync(employee.Email, null, cancellationToken))
        {
            throw new DuplicateEmailException("An employee with this email already exists.");
        }

        employee.Id = null;
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = null;
        return await repository.CreateAsync(employee, cancellationToken);
    }

    public async Task<Employee> UpdateAsync(string id, Employee employee, CancellationToken cancellationToken)
    {
        var existing = await GetAsync(id, cancellationToken);
        employee.Email = NormalizeEmail(employee.Email);
        if (await repository.EmailExistsAsync(employee.Email, id, cancellationToken))
        {
            throw new DuplicateEmailException("An employee with this email already exists.");
        }

        employee.Id = id;
        employee.CreatedAt = existing.CreatedAt;
        employee.UpdatedAt = DateTime.UtcNow;
        if (!await repository.UpdateAsync(id, employee, cancellationToken))
        {
            throw new KeyNotFoundException("Employee was not found.");
        }

        return employee;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await GetAsync(id, cancellationToken);
        if (!await repository.DeleteAsync(id, cancellationToken))
        {
            throw new KeyNotFoundException("Employee was not found.");
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

public sealed class DuplicateEmailException(string message) : Exception(message);

