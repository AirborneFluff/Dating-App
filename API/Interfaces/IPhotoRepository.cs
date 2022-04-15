using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<List<PhotoForApprovalDto>> GetUnapprovedPhotos(string username = null);
        Task<ICollection<Photo>> GetUserPhotos(string username);
        Task<Photo> GetPhotoById(int id);
        Task<bool> DeletePhoto(Photo photo);
    }
}