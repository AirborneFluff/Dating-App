using API.Entities;
using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UnitOfWork(DataContext context, IMapper mapper, IPhotoService photoService)
        {
            this._photoService = photoService;
            this._mapper = mapper;
            this._context = context;
        }

        public IUserRepository UserRepository => new UserRepository(_context, _mapper);
        public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);
        public IPhotoRepository PhotoRepository => new PhotoRepository(_context, _mapper, _photoService);
        public ILikeRepository LikeRepository => new LikeRepository(_context);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}