using System;
using RecreationBookingApp.Models;
namespace RecreationBookingApp.Repositories
{
    public interface IPlaceRepository : IRepository<Place>
    {
        Task<Place> GetFullPlaceAsync(string placeId); // Метод для полной загрузки
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<double> GetAverageRatingAsync(string placeId);
        Task<string> GetMainImageUrlAsync(string placeId);
    }
}

