
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    public class AccountController: BaseApiController
    {

        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;


        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }
        [HttpPost("register")] //POST: api/account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)

        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");
            var user =_mapper.Map<AppUser>(registerDto);
            //using var hmac = new HMACSHA512();
            user.UserName = registerDto.Username.ToLower();
            //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            //user.PasswordSalt = hmac.Key;

            // var user = new AppUser
            // {
            //     UserName = registerDto.Username.ToLower(),
            //     PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            //     PasswordSalt = hmac.Key
            // };

            var result = await _userManager.CreateAsync(user,registerDto.Password);
            if(!result.Succeeded) return BadRequest(result.Errors);
            var roleResult = await _userManager.AddToRoleAsync(user,"Member");
            if(!roleResult.Succeeded) return BadRequest(result.Errors);
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                //PhotoUrl = user.Photos.FirstOrDefault(x =>x.IsMain)?.Url
                Gender = user.Gender
            };
        }
        [HttpPost("login")] 
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            var user = await _userManager.Users
                        .Include(p=>p.Photos)
                        .SingleOrDefaultAsync(x =>x.UserName == loginDto.Username);
            
            
            //var user = await _context.Users.SingleOrDefaultAsync(x=>x.UserName ==loginDto.Username.ToLower());
            
            //var user = await _userRepository.GetUserByUsernameAsync(loginDto.Username.ToLower());
            if (user==null) return Unauthorized("invalid username");
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if(!result) return Unauthorized("Invalid password");
            // using var hmac = new HMACSHA512(user.PasswordSalt);
            // var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            // if (computedHash.Length != user.PasswordHash.Length) 
            //     return Unauthorized("invalid password (length)");
            // for(int i=0; i<computedHash.Length; i++)
            // {
            //     if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("invalid password");
            // }
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x =>x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };

        }
        private async Task<bool> UserExists (string username)
        {
            return await _userManager.Users.AnyAsync(user=>user.UserName==username.ToLower());
        }
    }
}