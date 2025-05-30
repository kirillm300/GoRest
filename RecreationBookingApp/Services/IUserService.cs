using System.Threading.Tasks;
using RecreationBookingApp.Models;

namespace RecreationBookingApp.Services;

public interface IUserService
{
    Task<User> LoginAsync(string email, string password);
    Task<bool> RegisterUserAsync(User user, string password);
    Task UpdatePasswordAsync(string email, string newPassword);
}