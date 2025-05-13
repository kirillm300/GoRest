using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RecreationBookingApp.Data;
using RecreationBookingApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RecreationBookingApp.Repositories;

public class PlaceRepository : IPlaceRepository
{
    private readonly AppDbContext _dbContext;

    public PlaceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Place>> GetAllAsync()
    {
        var places = new List<Place>();
        var connectionString = _dbContext.Database.GetDbConnection().ConnectionString;

        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT place_id, owner_id, name, description, address, latitude, longitude, contact_phone, category_id, status, created_at FROM places";

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var place = new Place
                    {
                        PlaceId = reader.GetString(0),
                        OwnerId = reader.GetString(1),
                        Name = reader.GetString(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Latitude = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                        Longitude = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                        ContactPhone = reader.IsDBNull(7) ? null : reader.GetString(7),
                        CategoryId = reader.GetString(8),
                        Status = reader.GetString(9),
                        CreatedAt = reader.GetDateTime(10)
                    };
                    places.Add(place);
                }
            }
        }

        return places;
    }

    public async Task<Place> GetAsync(Expression<Func<Place, bool>> predicate)
    {
        var allPlaces = await GetAllAsync();
        return allPlaces.AsQueryable().FirstOrDefault(predicate);
    }

    public async Task<Place> GetFullPlaceAsync(string placeId)
    {
        Debug.WriteLine($"GetFullPlaceAsync started for placeId={placeId}");
        var place = await GetAsync(p => p.PlaceId == placeId);
        if (place == null)
        {
            Debug.WriteLine($"Place not found for placeId={placeId}");
            return null;
        }
        Debug.WriteLine($"Place found: {place.Name}, placeId={placeId}");

        var connectionString = _dbContext.Database.GetDbConnection().ConnectionString;
        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            Debug.WriteLine("Database connection opened");

            // Загрузка комнат
            var roomCommand = connection.CreateCommand();
            roomCommand.CommandText = @"
                SELECT room_id, place_id, name, capacity, description, base_price, created_at
                FROM rooms
                WHERE place_id = $placeId";
            roomCommand.Parameters.AddWithValue("$placeId", placeId);

            using (var reader = await roomCommand.ExecuteReaderAsync())
            {
                Debug.WriteLine("Executing room query...");
                int roomCount = 0;
                while (await reader.ReadAsync())
                {
                    var room = new Room
                    {
                        RoomId = reader.GetString(0),
                        PlaceId = reader.GetString(1),
                        Name = reader.GetString(2),
                        Capacity = reader.GetInt32(3),
                        Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                        BasePrice = reader.GetDecimal(5),
                        CreatedAt = reader.GetDateTime(6)
                    };
                    place.Rooms.Add(room);
                    roomCount++;
                    Debug.WriteLine($"Room loaded: {room.Name}, roomId={room.RoomId}");
                }
                Debug.WriteLine($"Loaded {roomCount} rooms for placeId={placeId}");
            }

            // Загрузка особенностей для каждой комнаты
            foreach (var room in place.Rooms)
            {
                var featureCommand = connection.CreateCommand();
                featureCommand.CommandText = @"
                    SELECT feature_id, room_id, feature_name
                    FROM room_features
                    WHERE room_id = $roomId";
                featureCommand.Parameters.AddWithValue("$roomId", room.RoomId);

                using (var reader = await featureCommand.ExecuteReaderAsync())
                {
                    int featureCount = 0;
                    while (await reader.ReadAsync())
                    {
                        var feature = new RoomFeature
                        {
                            RoomFeatureId = reader.GetString(0),
                            RoomId = reader.GetString(1),
                            FeatureName = reader.IsDBNull(2) ? null : reader.GetString(2)
                        };
                        room.Features.Add(feature);
                        featureCount++;
                    }
                    Debug.WriteLine($"Loaded {featureCount} features for roomId={room.RoomId}");
                }
            }

            // Загрузка изображений
            var imageCommand = connection.CreateCommand();
            imageCommand.CommandText = @"
                SELECT url
                FROM images
                WHERE place_id = $placeId";
            imageCommand.Parameters.AddWithValue("$placeId", placeId);

            using (var reader = await imageCommand.ExecuteReaderAsync())
            {
                int imageCount = 0;
                while (await reader.ReadAsync())
                {
                    place.Images.Add(reader.GetString(0));
                    imageCount++;
                }
                Debug.WriteLine($"Loaded {imageCount} images for placeId={placeId}");
            }
        }

        Debug.WriteLine($"GetFullPlaceAsync completed for placeId={placeId}");
        return place;
    }

    public async Task AddAsync(Place entity)
    {
        var connectionString = _dbContext.Database.GetDbConnection().ConnectionString;

        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO places (place_id, owner_id, name, description, address, latitude, longitude, contact_phone, category_id, status, created_at)
                VALUES ($place_id, $owner_id, $name, $description, $address, $latitude, $longitude, $contact_phone, $category_id, $status, $created_at)";

            command.Parameters.AddWithValue("$place_id", entity.PlaceId);
            command.Parameters.AddWithValue("$owner_id", entity.OwnerId);
            command.Parameters.AddWithValue("$name", entity.Name);
            command.Parameters.AddWithValue("$description", (object)entity.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$address", (object)entity.Address ?? DBNull.Value);
            command.Parameters.AddWithValue("$latitude", entity.Latitude);
            command.Parameters.AddWithValue("$longitude", entity.Longitude);
            command.Parameters.AddWithValue("$contact_phone", (object)entity.ContactPhone ?? DBNull.Value);
            command.Parameters.AddWithValue("$category_id", entity.CategoryId);
            command.Parameters.AddWithValue("$status", entity.Status);
            command.Parameters.AddWithValue("$created_at", entity.CreatedAt);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task UpdateAsync(Place entity)
    {
        var connectionString = _dbContext.Database.GetDbConnection().ConnectionString;

        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE places
                SET owner_id = $owner_id, name = $name, description = $description, address = $address,
                    latitude = $latitude, longitude = $longitude, contact_phone = $contact_phone,
                    category_id = $category_id, status = $status, created_at = $created_at
                WHERE place_id = $place_id";

            command.Parameters.AddWithValue("$place_id", entity.PlaceId);
            command.Parameters.AddWithValue("$owner_id", entity.OwnerId);
            command.Parameters.AddWithValue("$name", entity.Name);
            command.Parameters.AddWithValue("$description", (object)entity.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$address", (object)entity.Address ?? DBNull.Value);
            command.Parameters.AddWithValue("$latitude", entity.Latitude);
            command.Parameters.AddWithValue("$longitude", entity.Longitude);
            command.Parameters.AddWithValue("$contact_phone", (object)entity.ContactPhone ?? DBNull.Value);
            command.Parameters.AddWithValue("$category_id", entity.CategoryId);
            command.Parameters.AddWithValue("$status", entity.Status);
            command.Parameters.AddWithValue("$created_at", entity.CreatedAt);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task DeleteAsync(Place entity)
    {
        var connectionString = _dbContext.Database.GetDbConnection().ConnectionString;

        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM places WHERE place_id = $place_id";
            command.Parameters.AddWithValue("$place_id", entity.PlaceId);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        using var connection = _dbContext.Database.GetDbConnection() as SqliteConnection;
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM categories";
        var categories = new List<Category>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categories.Add(new Category
            {
                CategoryId = reader.GetString(0),
                Name = reader.GetString(1),
                IconUrl = reader.IsDBNull(2) ? null : reader.GetString(2)
            });
        }
        return categories;
    }
}