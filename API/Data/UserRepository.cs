using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;

        }
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            var normalizedUsername = username.ToUpper();
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.NormalizedUserName == normalizedUsername);
        }
        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }
        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
        public async Task<MemberDto> GetMemberByUsernameAsync(string username)
        {
            var normalizedUsername = username.ToUpper();
            return await _context.Users
                .Where(x => x.NormalizedUserName == normalizedUsername)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }
        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();

            query = query.Where(x => x.NormalizedUserName != userParams.CurrentUsername.ToUpper());
            query = query.Where(x => x.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
            
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
                _ => query.OrderByDescending(x => x.LastActive)
            };

            var _query = query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking();

            return await PagedList<MemberDto>.CreateAsync(_query, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users.Where(x => x.UserName == username)
                .Select(x => x.Gender).FirstOrDefaultAsync();
        }
    }
}