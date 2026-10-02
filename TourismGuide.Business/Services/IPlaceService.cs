using TourismGuide.Business.DTOs;

namespace TourismGuide.Business.Services
{
    public interface IPlaceService
    {
        Task<List<PlaceDto>> GetAllAsync();
        Task<PlaceDto?> GetByIdAsync(int id);
        Task CreateAsync(PlaceDto placeDto);
        Task UpdateAsync(PlaceDto placeDto);
        Task DeleteAsync(int id);
    }
}