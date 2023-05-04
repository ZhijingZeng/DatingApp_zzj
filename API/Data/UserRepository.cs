using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper= mapper;
            
        }

        public async Task<MemberDto> GetMemberAsync(string username, string currentUsername)
        {
            var query = _context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);//help to eagerly load the related entites
            if (username == currentUsername)
            {
                query = query.IgnoreQueryFilters();
            }
            return await query.SingleOrDefaultAsync(); 
                
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams) //pagination
        {

            var query = _context.Users.AsQueryable();
            query = query.Where(u=>u.UserName != userParams.CurrentUsername);
            query = query.Where(u=>u.Gender == userParams.Gender);
            
            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge-1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(u=>u.DateOfBirth>minDob && u.DateOfBirth<=maxDob);
             
             
            query = userParams.OrderBy switch //switch in entity framework
            {
                "created"=>query.OrderByDescending(u=>u.Created),
                _=>query.OrderByDescending(u=>u.LastActive)
            };
            return await PagedList<MemberDto>.CreateAsync(query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider),userParams.PageNumber,userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p=>p.Photos)
                .SingleOrDefaultAsync(x=>x.UserName == username);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
                        .Where(x=>x.UserName == username)
                        .Select(x => x.Gender).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
           return await _context.Users
           .Include(p=>p.Photos)
           .ToListAsync();
        }


        public void Update(AppUser user)
        {
           _context.Entry(user).State = EntityState.Modified; 
        }
    }
}
// 1.A client application requests a list of members from a web API endpoint.

// 2.The web API controller invokes a method on the repository interface to retrieve the list of members.

// 3.The repository class retrieves the member entities from the data storage system (e.g., database), and maps them to a list of MemberDTOs.

// 4.The repository class returns the list of MemberDTOs to the web API controller.

// 5.The web API controller maps the list of MemberDTOs to a JSON response, and returns it to the client application.