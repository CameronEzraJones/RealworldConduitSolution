using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Conduit.Contexts;
using Conduit.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Controllers
{
    [Produces("application/json")]
    [Route("api/Profiles")]
    public class ProfilesController : Controller
    {
        UserManager<ApplicationUser> _userManager;
        ConduitDbContext _context;

        public ProfilesController(UserManager<ApplicationUser> userManager,
            ConduitDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        // Get a profile
        [HttpGet("/api/profiles/{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            ApplicationUser requestedUser = await _userManager.FindByNameAsync(username);
            Boolean isFollowing = false;
            if(null == requestedUser)
            {
                this.HttpContext.Response.StatusCode = 404;
                var errorResponse = new ErrorResponse();
                errorResponse.addErrorKey("No user found with the specified username");
                return Json(errorResponse);
            }
            var authedUser = HttpContext.User;
            if (authedUser.HasClaim(c =>
             c.Type == ClaimTypes.NameIdentifier))
            {
                string authedUsername = authedUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                ApplicationUser user = await _userManager.FindByNameAsync(authedUsername);
                if(null == user)
                {
                    this.HttpContext.Response.StatusCode = 404;
                    var errorResponse = new ErrorResponse();
                    errorResponse.addErrorKey("No user making the request doesn't exist");
                    return Json(errorResponse);
                }
                string requestedUserId = requestedUser.Id;
                string authedUserId = user.Id;
                var isFollowingCheck = _context.UserIsFollowing
                    .FirstOrDefault(m => m.UserId == authedUserId && m.IsFollowingId == requestedUserId);
                if(null != isFollowingCheck)
                {
                    isFollowing = true;
                }
            }
            Profile profile = (Profile)requestedUser;
            profile.IsFollowing = isFollowing;
            return Ok(profile);
        }
    }
}