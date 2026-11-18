using Microsoft.AspNetCore.Identity;
using Moq;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Application.Services;
using SplitwiseClone.Domain.Entities;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsApplicationUserProfile()
    {
        var unit = new Mock<IUnitOfWork>();
        var users = new Mock<IUserRepository>();
        var manager = CreateManager();
        var createdAt = DateTime.UtcNow;
        users.Setup(repository => repository.GetByIdAsync(5, default))
            .ReturnsAsync(new ApplicationUser { Id = 5, FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com", CreatedAt = createdAt });
        unit.SetupGet(value => value.Users).Returns(users.Object);
        var service = new UserService(unit.Object, manager.Object);

        var result = await service.GetByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal("Ada", result.FirstName);
        Assert.Equal("Lovelace", result.LastName);
        Assert.Equal("ada@example.com", result.Email);
    }

    private static Mock<UserManager<ApplicationUser>> CreateManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }
}
