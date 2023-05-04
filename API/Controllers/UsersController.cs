using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IPhotoService _photoService;
        public UsersController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
        {
            _uow = uow;
            _photoService = photoService;
            _mapper = mapper;
        }
        //[AllowAnonymous]
        //[Authorize(Roles = "Admin")] //to overwite the Authorize that at the top of the class
        //a member trying to go through --401 forbidden not 403 unauthorized.
        //Becasue we have authenticated just not authorized to do this job
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        //client will send userParams from query string, we need to specify where api will need to look 
        {
            var gender = await _uow.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender =="male" ? "female":"male";
            }

            var users = await _uow.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize,users.totalCount,users.TotalPages));
            //var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
            return Ok(users);

        }
        // add another method for getting indivitual user
        //[Authorize(Roles = "Member,Admin")]
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _uow.UserRepository.GetMemberAsync(username,User.GetUsername());
        }
        [HttpPut] //update
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUsername(); //username inside token, User system securityClaimPrinciple have access to token, GetUsername see extension method
            var user= await _uow.UserRepository.GetUserByUsernameAsync(username);
            if(user ==null) return NotFound();
            _mapper.Map(memberUpdateDto,user);//updating all properties in memberUpdateDto=>user
            if(await _uow.Complete()) //update to the database
                return NoContent(); //everything is ok but I have nothing to send back to you 204
            return BadRequest("Failed to update user");//if no changes then it is a bad request
        }


        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto (IFormFile file)
        {
            var user= await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if(user == null) return NotFound();
            var result = await  _photoService.AddPhotoAsync(file);
            if(result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo{
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            //if(user.Photos.Count ==0) photo.IsMain = true;
            user.Photos.Add(photo);
            if(await _uow.Complete()) {
                return CreatedAtAction(nameof(GetUser), //can call endpoint as Actions, in the header
                new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
                //return back how can we get this newly created photo
            }
            //return _mapper.Map<PhotoDto>(photo);
            return BadRequest("Problem adding photo");

        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto (int photoId)
        {
            var user= await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if(user == null) return NotFound();
            var photo = user.Photos.FirstOrDefault(x=> x.Id == photoId);
            if(photo == null) return NotFound();
            if(photo.IsMain) return BadRequest("this is already your main photo");
            var currentMain = user.Photos.FirstOrDefault(x=>x.IsMain);
            if(currentMain != null) currentMain.IsMain =false;
            photo.IsMain = true;
            if(await _uow.Complete()) return NoContent();
            return BadRequest("Problem setting the main photo");

        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto (int photoId)
        {
            //var user= await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            //if(user == null) return NotFound();
            var photo = await _uow.PhotoRepository.GetPhotoByIdAsync(photoId);
            if(photo == null) return NotFound();
            if(photo.IsMain) return BadRequest("You cannot delete your main photo");
            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }
            _uow.PhotoRepository.RomovePhoto(photo);
            if(await _uow.Complete()) return Ok();
            return BadRequest("Problem deleting photo");
        }   
    }
}

// 1.A client application requests a list of members from a web API endpoint.

// 2.The web API controller invokes a method on the repository interface to retrieve the list of members.

// 3.The repository class retrieves the member entities from the data storage system (e.g., database), and maps them to a list of MemberDTOs.

// 4.The repository class returns the list of MemberDTOs to the web API controller.

// 5.The web API controller maps the list of MemberDTOs to a JSON response, and returns it to the client application.


//http code
//https://www.restapitutorial.com/httpstatuscodes.html
//2** good request
//200 ok
//201 (post request) created, return the address where to get the new resource
//204 No content(everything went well, nothing need to be returned. such as delete, put)

//3** 
//304 not modified (when you call the api, has this been changed since yesterday. Then we don't need to download anything, pull it from the cach you have)

//4** client error
//400 bad request Your information is bad. Such as name and email only given name
//401 unauthorized. No Idea who you are
//403 forbidden know who you are but you donot have the permission(basic user sent an api key but do not have the permission to the admin credencial)
//404 not found there is no api you are looking for


//5** server error





