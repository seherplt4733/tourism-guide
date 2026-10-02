using Microsoft.EntityFrameworkCore;
using TourismGuide.Business.DTOs;
using TourismGuide.DataAccess;
using TourismGuide.Entity;

namespace TourismGuide.Business.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly ApplicationDbContext _context;

        public PlaceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlaceDto>> GetAllAsync()
        {
            return await _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .Select(p => new PlaceDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    ShortDescription = p.ShortDescription,
                    FullDescription = p.FullDescription,
                    Address = p.Address,
                    Price = p.Price,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    ImageUrl = p.ImageUrl,
                    Rating = p.Rating,
                    CityId = p.CityId,
                    CityName = p.City != null ? p.City.Name : "",
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : ""
                })
                .ToListAsync();
        }

        public async Task<PlaceDto?> GetByIdAsync(int id)
        {
            var p = await _context.Places
                .Include(p => p.City)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null;

            return new PlaceDto
            {
                Id = p.Id,
                Title = p.Title,
                ShortDescription = p.ShortDescription,
                FullDescription = p.FullDescription,
                Address = p.Address,
                Price = p.Price,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                ImageUrl = p.ImageUrl,
                Rating = p.Rating,
                CityId = p.CityId,
                CityName = p.City?.Name ?? "",
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? ""
            };
        }

        public async Task CreateAsync(PlaceDto dto)
        {
            var entity = new Place
            {
                Title = dto.Title,
                ShortDescription = dto.ShortDescription,
                FullDescription = dto.FullDescription,
                Address = dto.Address,
                Price = dto.Price,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                ImageUrl = dto.ImageUrl,
                Rating = dto.Rating,
                CityId = dto.CityId,
                CategoryId = dto.CategoryId
            };

            await _context.Places.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PlaceDto dto)
        {
            var entity = await _context.Places.FindAsync(dto.Id);
            if (entity != null)
            {
                entity.Title = dto.Title;
                entity.ShortDescription = dto.ShortDescription;
                entity.FullDescription = dto.FullDescription;
                entity.Address = dto.Address;
                entity.Price = dto.Price;
                entity.Latitude = dto.Latitude;
                entity.Longitude = dto.Longitude;
                entity.ImageUrl = dto.ImageUrl;
                entity.Rating = dto.Rating;
                entity.CityId = dto.CityId;
                entity.CategoryId = dto.CategoryId;

                _context.Places.Update(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Places.FindAsync(id);
            if (entity != null)
            {
                _context.Places.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}