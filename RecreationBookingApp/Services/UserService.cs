using RecreationBookingApp.Models;
using RecreationBookingApp.Repositories;
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
        if (user == null || user.PasswordHash != password)
            return null;

        return user;
    }

    public async Task<bool> RegisterUserAsync(User user, string password)
    {
        if (await _userRepository.GetAsync(u => u.Email == user.Email) != null)
            return false;

        user.PasswordHash = password; // Прямое сохранение пароля (замени на хеширование в будущем)
        await _userRepository.AddAsync(user);
        return true;
    }
}