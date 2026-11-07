using Microsoft.AspNetCore.Identity;
using Moq;
using SplitwiseClone.Application.DTOs.Auth;
using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.DTOs.Groups;
using SplitwiseClone.Application.DTOs.Users;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Application.Services;
using SplitwiseClone.Domain.Entities;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _work = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _work.SetupGet(x => x.Users).Returns(_users.Object);
        _service = new UserService(_work.Object);
    }

    [Fact]
    public async Task GetAllAsync_MapsUsers()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Alex", Email = "a@b.com" };
        _users.Setup(x => x.GetAllAsync(default)).ReturnsAsync(new[] { user });

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(user.Id, result[0].Id);
        Assert.Equal(user.Name, result[0].Name);
        Assert.Equal(user.Email, result[0].Email);
        Assert.Equal(user.CreatedAt, result[0].CreatedAt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByIdAsync_ReturnsMappedUserOrNull(bool exists)
    {
        var id = Guid.NewGuid();
        var user = exists ? new User { Id = id, Name = "Alex", Email = "a@b.com" } : null;
        _users.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(user);

        var result = await _service.GetByIdAsync(id);

        if (exists)
        {
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Alex", result.Name);
        }
        else Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsUser()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Old", Email = "old@b.com" };
        _users.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await _service.UpdateAsync(user.Id, new UpdateUserDto("New", "new@b.com"));

        Assert.Equal("New", user.Name);
        Assert.Equal("new@b.com", user.Email);
        Assert.Equal(user.Id, result.Id);
        _users.Verify(x => x.Update(user), Times.Once);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenUserMissing()
    {
        var id = Guid.NewGuid();
        _users.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateAsync(id, new("Name", "a@b.com")));

        _users.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeletesUser()
    {
        var user = new User { Id = Guid.NewGuid() };
        _users.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        await _service.DeleteAsync(user.Id);

        _users.Verify(x => x.Delete(user), Times.Once);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenUserMissing()
    {
        var id = Guid.NewGuid();
        _users.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteAsync(id));

        _users.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }
}

