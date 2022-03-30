using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        public UsersController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // api/users/... id value
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PublicMemeberDto>>> GetMembers()
        {
            var members = await _context.Users.ToListAsync();

            var memberPublicDtos = members.Select(user => {
                return new PublicMemeberDto() {
                    Username = user.UserName,
                };
            });

            return memberPublicDtos.ToList();
        }

        /*
        [HttpGet("user?={username}")]
        public async Task<ActionResult<AppUser>> GetUser(string username)
        {
            return await _context.Users.SingleAsync(x => x.UserName == username);
        }
        */
    }
}