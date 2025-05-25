using Xunit;
using Moq;
using RecreationBookingApp.ViewModels;
using RecreationBookingApp.Services;
using RecreationBookingApp.Models;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;

namespace RecreationBookingApp.Tests
{
    public class LoginViewModelTests
    {
        private readonly LoginViewModel _viewModel;
        private readonly Mock<IUserService> _userServiceMock;

        public LoginViewModelTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _viewModel = new LoginViewModel(_userServiceMock.Object);
        }

        [Fact]
        public async Task LoginAsync_SuccessfulLogin_ClearsErrorAndSetsBusy()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new User { UserId = "123", Email = email };
            _userServiceMock.Setup(s => s.LoginAsync(email, password)).ReturnsAsync(user);
            _viewModel.Email = email;
            _viewModel.Password = password;

            // Act
            await _viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            Assert.False(_viewModel.IsBusy);
            Assert.Equal(string.Empty, _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_SetsErrorMessage()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";
            _userServiceMock.Setup(s => s.LoginAsync(email, password)).ReturnsAsync((User)null);
            _viewModel.Email = email;
            _viewModel.Password = password;

            // Act
            await _viewModel.LoginCommand.ExecuteAsync(null);

            // Assert
            Assert.False(_viewModel.IsBusy);
            Assert.Equal("Неправильный email или пароль.", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task RegisterAsync_SuccessfulRegistration_ClearsError()
        {
            // Arrange
            var email = "newuser@example.com";
            var password = "password123";
            var fullName = "JTest test";
            var phone = "1234567890";
            _userServiceMock.Setup(s => s.RegisterUserAsync(It.IsAny<User>(), password)).ReturnsAsync(true);
            _viewModel.IsRegisterMode = true;
            _viewModel.Email = email;
            _viewModel.Password = password;
            _viewModel.FullName = fullName;
            _viewModel.Phone = phone;

            // Act
            await _viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.False(_viewModel.IsBusy);
            Assert.Equal(string.Empty, _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task RegisterAsync_ExistingUser_SetsErrorMessage()
        {
            // Arrange
            var email = "existing@example.com";
            var password = "password123";
            _userServiceMock.Setup(s => s.RegisterUserAsync(It.IsAny<User>(), password)).ReturnsAsync(false);
            _viewModel.IsRegisterMode = true;
            _viewModel.Email = email;
            _viewModel.Password = password;
            _viewModel.FullName = "Test test";
            _viewModel.Phone = "1234567890";

            // Act
            await _viewModel.RegisterCommand.ExecuteAsync(null);

            // Assert
            Assert.False(_viewModel.IsBusy);
            Assert.Equal("Пользователь с таким email уже существует.", _viewModel.ErrorMessage);
        }

        [Fact]
        public void ToggleMode_SwitchesModeAndResetsProperties()
        {
            // Arrange
            _viewModel.IsRegisterMode = false;
            _viewModel.Email = "test";
            _viewModel.Password = "test";
            _viewModel.FullName = "test";
            _viewModel.Phone = "test";

            // Act
            _viewModel.ToggleModeCommand.Execute(null);

            // Assert
            Assert.True(_viewModel.IsRegisterMode);
            Assert.Equal(string.Empty, _viewModel.Email);
            Assert.Equal(string.Empty, _viewModel.Password);
            Assert.Equal(string.Empty, _viewModel.FullName);
            Assert.Equal(string.Empty, _viewModel.Phone);
            Assert.Equal(string.Empty, _viewModel.ErrorMessage);
        }

        [Theory]
        [InlineData("", "", false, false)] // Пустые поля
        [InlineData("test@example.com", "password", false, true)] // Заполненные поля
        [InlineData("test@example.com", "password", true, false)] // IsBusy = true
        public void CanExecuteLogin_ReturnsExpected(string email, string password, bool isBusy, bool expected)
        {
            // Arrange
            _viewModel.Email = email;
            _viewModel.Password = password;
            _viewModel.IsBusy = isBusy;

            // Act
            var canExecute = _viewModel.LoginCommand.CanExecute(null);

            // Assert
            Assert.Equal(expected, canExecute);
        }

        [Theory]
        [InlineData("", "", "", "", false, false)] // Пустые поля
        [InlineData("test@example.com", "password", "John", "123", false, true)] // Заполненные поля
        [InlineData("test@example.com", "password", "John", "123", true, false)] // IsBusy = true
        public void CanExecuteRegister_ReturnsExpected(string email, string password, string fullName, string phone, bool isBusy, bool expected)
        {
            // Arrange
            _viewModel.Email = email;
            _viewModel.Password = password;
            _viewModel.FullName = fullName;
            _viewModel.Phone = phone;
            _viewModel.IsBusy = isBusy;

            // Act
            var canExecute = _viewModel.RegisterCommand.CanExecute(null);

            // Assert
            Assert.Equal(expected, canExecute);
        }
    }
}