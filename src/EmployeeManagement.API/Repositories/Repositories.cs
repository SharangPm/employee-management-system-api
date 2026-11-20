using System.Text.RegularExpressions;
using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EmployeeManagement.API.Repositories;

public sealed class EmployeeRepository(IMongoDatabase database) : IEmployeeRepository
{
    private readonly IMongoCollection<Employee> collection = database.GetCollection<Employee>("employees");

    public async Task<Employee?> GetAsync(string id, CancellationToken cancellationToken) =>
        (Employee?)await collection.Find(employee => employee.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task<Employee?> GetByUserIdAsync(string userId, CancellationToken cancellationToken) =>
        (Employee?)await collection.Find(employee => employee.LinkedUserId == userId).FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> LinkUserAsync(string employeeId, string userId, CancellationToken cancellationToken)
    {
        var filter = Builders<Employee>.Filter.Eq(employee => employee.Id, employeeId) &
            Builders<Employee>.Filter.Exists(employee => employee.LinkedUserId, false);
        var update = Builders<Employee>.Update.Set(employee => employee.LinkedUserId, userId);
        return (await collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken)).ModifiedCount == 1;
    }

    public async Task<(IReadOnlyList<Employee> Items, long Total)> ListAsync(
        string? search, string? department, bool? active, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var filter = Builders<Employee>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var safeSearch = new BsonRegularExpression(Regex.Escape(search.Trim()), "i");
            filter &= Builders<Employee>.Filter.Or(
                Builders<Employee>.Filter.Regex(employee => employee.FirstName, safeSearch),
                Builders<Employee>.Filter.Regex(employee => employee.LastName, safeSearch));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            filter &= Builders<Employee>.Filter.Eq(employee => employee.Department, department.Trim());
        }

        if (active.HasValue)
        {
            filter &= Builders<Employee>.Filter.Eq(employee => employee.IsActive, active.Value);
        }

        var total = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await collection.Find(filter)
            .SortBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken)
    {
        await collection.InsertOneAsync(employee, cancellationToken: cancellationToken);
        return employee;
    }

    public async Task<bool> UpdateAsync(string id, Employee employee, CancellationToken cancellationToken)
    {
        employee.Id = id;
        return (await collection.ReplaceOneAsync(
            current => current.Id == id, employee, cancellationToken: cancellationToken)).MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) =>
        (await collection.DeleteOneAsync(employee => employee.Id == id, cancellationToken)).DeletedCount > 0;

    public Task<bool> EmailExistsAsync(string email, string? exceptId, CancellationToken cancellationToken) =>
        collection.Find(employee => employee.Email == email && employee.Id != exceptId).AnyAsync(cancellationToken);
}

public sealed class UserRepository(IMongoDatabase database) : IUserRepository
{
    private readonly IMongoCollection<User> collection = database.GetCollection<User>("users");

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        (User?)await collection.Find(user => user.Email == email).FirstOrDefaultAsync(cancellationToken);

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken)
    {
        await collection.InsertOneAsync(user, cancellationToken: cancellationToken);
        return user;
    }
}




