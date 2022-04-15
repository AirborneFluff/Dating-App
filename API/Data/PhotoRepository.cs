using System.Linq.Expressions;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public PhotoRepository(DataContext context, IMapper mapper, IPhotoService photoService)
        {
            this._photoService = photoService;
            this._mapper = mapper;
            this._context = context;
        }

        public async Task<bool> DeletePhoto(Photo photo)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return false;
            _context.Photos.Remove(photo);
            return true;
        }

        public async Task<Photo> GetPhotoById(int id)
        {
            return await _context.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<PhotoForApprovalDto>> GetUnapprovedPhotos(string username = null)
        {
            var query = _context.Photos
                .IgnoreQueryFilters()
                .Where(p => p.IsApproved == false)
                .Select(u => new PhotoForApprovalDto
                {
                    Id = u.Id,
                    Username = u.AppUser.UserName,
                    Url = u.Url,
                    IsApproved = u.IsApproved
                });

            if (username != null) query = query.Where(p => p.Username == username);

            return await query.ToListAsync();
        }

        public async Task<ICollection<Photo>> GetUserPhotos(string username)
        {
            var user = await _context.Users.Include(p => p.Photos)
                .IgnoreQueryFilters()
                .Where(u => u.UserName.ToUpper() == username.ToUpper()).FirstOrDefaultAsync();

            return user.Photos;
        }
    }
}