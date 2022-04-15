using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            this._userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
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
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var normalizedUsername = username.ToUpper();
            var selectedRoles = roles.Split(',').ToArray();

            var user = await _userManager.FindByNameAsync(normalizedUsername);
            if (user == null) return NotFound("No user found");

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest(result.Errors);

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult<PhotoForApprovalDto[]>> GetPhotosForModeration()
        {
            var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
            if (photos.Count == 0) NotFound("No photos to moderate");

            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{id}")]
        public async Task<ActionResult> ApprovePhoto(int id)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(id);

            if (photo == null) return NotFound("No photo found with that Id");
            if (photo.IsApproved) return BadRequest("This photo has already been approved");
            photo.IsApproved = true;

            var userMainPhoto = await _unitOfWork.UserRepository.GetUserMainPhoto(photo.AppUserId);

            if (userMainPhoto == null) photo.IsMain = true;

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Couldn't approve photo");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{Id}")]
        public async Task<ActionResult> RejectPhoto(int id)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(id);

            if (photo == null) return NotFound("No photo found with that Id");
            if (photo.IsApproved) return BadRequest("This photo has been approved. Try deleting instead");

            if (await _unitOfWork.PhotoRepository.DeletePhoto(photo) == false) return BadRequest("Issue deleting photo from cloud storage");

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Couldn't delete photo");
        }
    }
}