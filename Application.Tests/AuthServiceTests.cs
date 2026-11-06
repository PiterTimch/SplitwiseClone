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

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _users;
    private readonly Mock<IJwtTokenService> _tokens = new();
    private readonly Mock<IUnitOfWork> _work = new();
    private readonly Mock<IUserRepository> _domainUsers = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _users = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _work.SetupGet(x => x.Users).Returns(_domainUsers.Object);
        _service = new AuthService(_users.Object, _tokens.Object, _work.Object);
    }

    [Fact]
    public async Task RegisterAsync_CreatesBothUsersAndReturnsToken()
    {
        _users.Setup(x => x.FindByEmailAsync("a@b.com")).ReturnsAsync((ApplicationUser?)null);
        _users.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "secret")).ReturnsAsync(IdentityResult.Success);
        _users.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { "member" });
        _tokens.Setup(x => x.GenerateToken(It.IsAny<IdentityUser<Guid>>(), It.IsAny<IList<string>>())).Returns("jwt");

        var result = await _service.RegisterAsync(new("Alex", "a@b.com", "secret"));

        Assert.Equal("jwt", result.AccessToken);
        Assert.Equal("Alex", result.Name);
        Assert.Equal("a@b.com", result.Email);
        _domainUsers.Verify(x => x.AddAsync(It.Is<User>(u => u.Name == "Alex" && u.Email == "a@b.com"), default), Times.Once);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_RejectsExistingEmail()
    {
        _users.Setup(x => x.FindByEmailAsync("a@b.com")).ReturnsAsync(new ApplicationUser());

        await Assert.ThrowsAsync<EntityConflictException>(() => _service.RegisterAsync(new("Alex", "a@b.com", "secret")));

        _users.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_RejectsFailedIdentityCreation()
    {
        _users.Setup(x => x.FindByEmailAsync("a@b.com")).ReturnsAsync((ApplicationUser?)null);
        _users.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "secret")).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "weak password" }));

        var exception = await Assert.ThrowsAsync<EntityConflictException>(() => _service.RegisterAsync(new("Alex", "a@b.com", "secret")));

        Assert.Contains("weak password", exception.Message);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokenForValidCredentials()
    {
        var user = new ApplicationUser { Email = "a@b.com", DisplayName = "Alex" };
        _users.Setup(x => x.FindByEmailAsync("a@b.com")).ReturnsAsync(user);
        _users.Setup(x => x.CheckPasswordAsync(user, "secret")).ReturnsAsync(true);
        _users.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "member" });
        _tokens.Setup(x => x.GenerateToken(user, It.IsAny<IList<string>>())).Returns("jwt");

        var result = await _service.LoginAsync(new("a@b.com", "secret"));

        Assert.Equal("jwt", result.AccessToken);
        Assert.Equal("Alex", result.Name);
        Assert.Equal("a@b.com", result.Email);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task LoginAsync_RejectsUnknownUserOrInvalidPassword(bool userExists, bool passwordValid)
    {
        var user = userExists ? new ApplicationUser { Email = "a@b.com" } : null;
        _users.Setup(x => x.FindByEmailAsync("a@b.com")).ReturnsAsync(user);
        if (userExists) _users.Setup(x => x.CheckPasswordAsync(user!, "secret")).ReturnsAsync(passwordValid);

        await Assert.ThrowsAsync<UserUnauthorizedException>(() => _service.LoginAsync(new("a@b.com", "secret")));

        _tokens.Verify(x => x.GenerateToken(It.IsAny<IdentityUser<Guid>>(), It.IsAny<IList<string>>()), Times.Never);
    }
}

