using System.Reflection.Metadata.Ecma335;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikeRepository _likeRepository;
        public LikesController(IUserRepository userRepository, ILikeRepository likeRepository)
        {
            this._likeRepository = likeRepository;
            this._userRepository = userRepository;
        }

        [Authorize]
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var currentUser = await _likeRepository.GetUserWithLikesAsync(User.GetUserId());
            var likedUser = await _userRepository.GetUserByUsernameAsync(username);

            if (likedUser == null) return NotFound();
            if (currentUser.Id == likedUser.Id) return BadRequest("You cannot like yourself");

            var userLike = await _likeRepository.GetUserLikeAsync(currentUser.Id, likedUser.Id);

            if (userLike != null) return BadRequest("You have already liked this user");

            userLike = new UserLike
            {
                SourceUserId = currentUser.Id,
                LikedUserId = likedUser.Id
            };

            currentUser.LikedUsers.Add(userLike);

            if(await _likeRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikeParams likeParams)
        {
            likeParams.UserId = User.GetUserId();
            var users = await _likeRepository.GetUserLikesAsync(likeParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }
    }
}