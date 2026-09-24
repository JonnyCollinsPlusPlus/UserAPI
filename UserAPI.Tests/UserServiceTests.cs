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

}