using EmployeeManagement.API.Interfaces;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Services;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.Services;

public sealed class EmployeeServiceTests
{
    private const string EmployeeId = "507f1f77bcf86cd799439011";

    [Fact]
    public async Task CreateSetsTimestampsAndPersistsNormalizedEmail()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(item => item.EmailExistsAsync("person@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository.Setup(item => item.CreateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee employee, CancellationToken _) => employee);

        var employee = await new EmployeeService(repository.Object).CreateAsync(
            new Employee { Email = " Person@Example.com " }, CancellationToken.None);

        Assert.Equal("person@example.com", employee.Email);
        Assert.NotEqual(default, employee.CreatedAt);
        Assert.Null(employee.UpdatedAt);
        repository.Verify(item => item.CreateAsync(employee, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReturnsEmployee()
    {
        var expected = new Employee { Id = EmployeeId };
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(item => item.GetAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var actual = await new EmployeeService(repository.Object).GetAsync(EmployeeId, CancellationToken.None);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task UpdatePreservesCreatedAtAndSetsUpdatedAt()
    {
        var created = DateTime.UtcNow.AddDays(-2);
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(item => item.GetAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = EmployeeId, CreatedAt = created });
        repository.Setup(item => item.EmailExistsAsync("person@example.com", EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository.Setup(item => item.UpdateAsync(EmployeeId, It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var employee = await new EmployeeService(repository.Object).UpdateAsync(
            EmployeeId, new Employee { Email = "person@example.com" }, CancellationToken.None);

        Assert.Equal(created, employee.CreatedAt);
        Assert.NotNull(employee.UpdatedAt);
    }

    [Fact]
    public async Task DeleteRemovesExistingEmployee()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(item => item.GetAsync(EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = EmployeeId });
        repository.Setup(item => item.DeleteAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await new EmployeeService(repository.Object).DeleteAsync(EmployeeId, CancellationToken.None);

        repository.Verify(item => item.DeleteAsync(EmployeeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MissingEmployeeThrowsNotFound()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(item => item.GetAsync(EmployeeId, It.IsAny<CancellationToken>())).ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new EmployeeService(repository.Object).GetAsync(EmployeeId, CancellationToken.None));
    }

    [Fact]
    public async Task DuplicateEmailThrowsConflictException()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(item => item.EmailExistsAsync("person@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateEmailException>(() => new EmployeeService(repository.Object)
            .CreateAsync(new Employee { Email = "person@example.com" }, CancellationToken.None));
    }

    [Fact]
    public async Task InvalidObjectIdIsRejected()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => new EmployeeService(Mock.Of<IEmployeeRepository>())
            .GetAsync("bad-id", CancellationToken.None));
    }
}
