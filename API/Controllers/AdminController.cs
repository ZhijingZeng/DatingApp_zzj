using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {   
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IPhotoService _photoService;
        public AdminController(UserManager<AppUser> userManager, IMapper mapper, IUnitOfWork uow, IPhotoService photoService)
        {
            _photoService = photoService;
            _userManager = userManager;
            _uow =uow;
            _mapper = mapper;
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                            .OrderBy(u =>u.UserName)
                            .Select(u => new
                            {
                                u.Id,
                                Username = u.UserName,
                                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                            })
                            .ToListAsync();
            return Ok(users);
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username,[FromQuery]string roles)
        {
            if(string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");
            var selectedRoles = roles.Split(",").ToArray();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound();
            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.AddToRolesAsync(user,selectedRoles.Except(userRoles));
            if(!result.Succeeded) return BadRequest("Failed to add to roles");
            result = await _userManager.RemoveFromRolesAsync(user,userRoles.Except(selectedRoles));
            if(!result.Succeeded) return BadRequest("Failed to remove from roles");
            return Ok(await _userManager.GetRolesAsync(user));

        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForModeration(){
            
            var photos=await _uow.PhotoRepository.GetUnapprovedPhotosAsync();
            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpDelete("reject-photo/{id}")]
        public async Task<ActionResult> RejectaPhotoByid(int id){
            Photo photo = await  _uow.PhotoRepository.GetPhotoByIdAsync(id);
            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }
            _uow.PhotoRepository.RomovePhoto(photo);
            if(await _uow.Complete()) return Ok();
            return BadRequest("Problem deleting photo");
        }
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPut("approve-photo/{id}")]
        public async Task<ActionResult> ApprovePhoto (int id)
        {
            var photo= await _uow.PhotoRepository.GetPhotoByIdAsync(id);
            if(photo == null) return NotFound();
            if(photo.IsApproved) return BadRequest("this has already been approved");
            var user = await _uow.UserRepository.GetUserByUsernameAsync(photo.AppUser.UserName);
            photo.IsApproved = true;
            
            if(user.Photos.Where(x=>x.IsMain).FirstOrDefault() == null) photo.IsMain = true; //not sure if filter works
            
            if(await _uow.Complete()) return NoContent();
            return BadRequest("Problem approving photo");
        }

    }

}