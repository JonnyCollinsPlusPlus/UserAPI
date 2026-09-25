using Moq;
using UserAPI;
using UserAPI.DTOs;
using UserAPI.Services;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public async Task AddUserAsync_HashesPassword()
    {
        // Arrange
        var mockTokenService = new Mock<ITokenService>();
        var mockRepo = new Mock<IUserRepository>();
        User? capturedUser = null;

        mockRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        var dto = new UserRequestDTO
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "plaintext123",
            Role = "User"
        };

        // Act
        await service.AddUserAsync(dto);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.NotEqual("plaintext123", capturedUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("plaintext123", capturedUser.PasswordHash));
    }


    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsWrong()
    {
        var existingUser = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            Role = "User"
        };

        var mockTokenService = new Mock<ITokenService>();
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        var result = await service.LoginAsync(new LoginDTO
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        });

        Assert.Null(result);
    }
    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenEmailDoesNotExist()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByEmailAsync("nobody@example.com"))
            .ReturnsAsync((User?)null);

        var mockTokenService = new Mock<ITokenService>();
        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        var result = await service.LoginAsync(new LoginDTO
        {
            Email = "nobody@example.com",
            Password = "whatever"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser_WhenFound()
    {
        var existingUser = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "somehash",
            Role = "User"
        };

        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);

        var mockTokenService = new Mock<ITokenService>();
        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        var result = await service.GetUserByIdAsync(1);

        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetUserByIdAsync_ThrowsKeyNotFound_WhenMissing()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        var mockTokenService = new Mock<ITokenService>();
        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetUserByIdAsync(999));
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesFields_WhenUserExists()
    {
        var existingUser = new User
        {
            Id = 1,
            Email = "old@example.com",
            Username = "olduser",
            PasswordHash = "somehash",
            Role = "User"
        };

        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var mockTokenService = new Mock<ITokenService>();
        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        var updateDto = new UserRequestDTO
        {
            Email = "new@example.com",
            Username = "newuser",
            Password = "newpassword123",
            Role = "User"
        };

        await service.UpdateUserAsync(1, updateDto);

        Assert.Equal("new@example.com", existingUser.Email);
        Assert.Equal("newuser", existingUser.Username);
        mockRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ThrowsKeyNotFound_WhenMissing()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        var mockTokenService = new Mock<ITokenService>();
        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        var updateDto = new UserRequestDTO
        {
            Email = "x@example.com",
            Username = "x",
            Password = "x",
            Role = "User"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateUserAsync(999, updateDto));
    }

    [Fact]
    public async Task DeleteUserAsync_DeletesUser_WhenExists()
    {
        var existingUser = new User { Id = 1, Email = "test@example.com" };

        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
        mockRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        var mockTokenService = new Mock<ITokenService>();
        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        await service.DeleteUserAsync(1);

        mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ThrowsKeyNotFound_WhenMissing()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        var mockTokenService = new Mock<ITokenService>();
        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteUserAsync(999));
    }
    [Fact]
    public async Task AddUserAsync_ThrowsInvalidOperation_WhenEmailAlreadyExists()
    {
        var existingUser = new User { Id = 1, Email = "test@example.com" };

        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ThrowsAsync(new InvalidOperationException("A user with this email already exists."));

        var mockTokenService = new Mock<ITokenService>();
        var service = new UserService(mockRepo.Object, mockTokenService.Object);

        var dto = new UserRequestDTO
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "plaintext123",
            Role = "User"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddUserAsync(dto));
    }

}