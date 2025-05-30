using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RecreationBookingApp.Models;
using RecreationBookingApp.Repositories;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RecreationBookingApp.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = await _userRepository.GetAsync(u => u.Email == email);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        // Хешируем введённый пароль
        string hashedInputPassword = HashPassword(password);
        if (hashedInputPassword != user.PasswordHash)
            return null;

        return user;
    }

    public async Task<bool> RegisterUserAsync(User user, string password)
    {
        if (await _userRepository.GetAsync(u => u.Email == user.Email) != null)
            return false;

        // Хешируем пароль перед сохранением
        user.PasswordHash = HashPassword(password);
        await _userRepository.AddAsync(user);
        return true;
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    public async Task UpdatePasswordAsync(string email, string newPassword)
    {
        var user = await _userRepository.GetAsync(u => u.Email == email);
        if (user == null)
        {
            throw new Exception("Пользователь с таким email не найден.");
        }

        // Хешируем новый пароль
        user.PasswordHash = HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
    }
}