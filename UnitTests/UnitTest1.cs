using Xunit;
using Moq;
using RecreationBookingApp.Services;
using RecreationBookingApp.Models;
using RecreationBookingApp.Repositories;
using System.Threading.Tasks;

public class UserServiceTests
{
    private readonly UserService _userService;
    private readonly Mock<IRepository<User>> _userRepositoryMock;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _userService = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var user = new User { Email = email, PasswordHash = password };
        _userRepositoryMock.Setup(r => r.GetAsync(u => u.Email == email)).ReturnsAsync(user);

        // Act
        var result = await _userService.LoginAsync(email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "password123";
        _userRepositoryMock.Setup(r => r.GetAsync(u => u.Email == email)).ReturnsAsync((User)null);

        // Act
        var result = await _userService.LoginAsync(email, password);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var email = "test@example.com";
        var correctPassword = "password123";
        var wrongPassword = "wrongpassword";
        var user = new User { Email = email, PasswordHash = correctPassword };
        _userRepositoryMock.Setup(r => r.GetAsync(u => u.Email == email)).ReturnsAsync(user);

        // Act
        var result = await _userService.LoginAsync(email, wrongPassword);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_EmptyCredentials_ReturnsNull()
    {
        // Arrange
        var email = "";
        var password = "";

        // Act
        var result = await _userService.LoginAsync(email, password);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterUserAsync_NewUser_ReturnsTrue()
    {
        // Arrange
        var email = "newuser@example.com";
        var password = "password123";
        var user = new User { Email = email };
        _userRepositoryMock.Setup(r => r.GetAsync(u => u.Email == email)).ReturnsAsync((User)null);

        // Act
        var result = await _userService.RegisterUserAsync(user, password);

        // Assert
        Assert.True(result);
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == email && u.PasswordHash == password)), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_ExistingUser_ReturnsFalse()
    {
        // Arrange
        var email = "existing@example.com";
        var password = "password123";
        var existingUser = new User { Email = email };
        _userRepositoryMock.Setup(r => r.GetAsync(u => u.Email == email)).ReturnsAsync(existingUser);

        // Act
        var result = await _userService.RegisterUserAsync(existingUser, password);

        // Assert
        Assert.False(result);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }
}