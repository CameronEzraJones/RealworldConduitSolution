using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Conduit.Contexts;
using Conduit.Models;
using Conduit.Models.HTTPTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Controllers
{
    [Produces("application/json")]
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
            ProfileHTTPTransferObject profileHTTPTransferObject = new ProfileHTTPTransferObject();
            profileHTTPTransferObject.Profile = profile;
            return Ok(profile);
        }

        //Follow a user
        [HttpPost("/api/profiles/{username}/follow")]
        public async Task<IActionResult> followUser(string username)
        {
            var authedUser = HttpContext.User;
            if (!authedUser.HasClaim(c =>
             c.Type == ClaimTypes.NameIdentifier))
            {
                this.HttpContext.Response.StatusCode = 401;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Missing authentication");
                return Json(authErrorResponse);
            }
            string authUsername = authedUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser authedUserInDB = await _userManager.FindByNameAsync(authUsername);
            if (null == authedUserInDB)
            {
                this.HttpContext.Response.StatusCode = 404;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Authenticated user not in database");
                return Json(authErrorResponse);
            }
            ApplicationUser requestUser = await _userManager.FindByNameAsync(username);
            if(null == requestUser)
            {
                this.HttpContext.Response.StatusCode = 404;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Requested user not in database");
                return Json(authErrorResponse);
            }
            string authedUserId = authedUserInDB.Id;
            string requestUserId = requestUser.Id;
            try
            {
                UserIsFollowing userIsFollowing = new UserIsFollowing();
                userIsFollowing.UserId = authedUserId;
                userIsFollowing.IsFollowingId = requestUserId;
                _context.UserIsFollowing.Add(userIsFollowing);
                _context.SaveChanges();
            } catch (DbUpdateException ex)
            {
                this.HttpContext.Response.StatusCode = 422;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey($"{authUsername} is already following {username}");
                return Json(authErrorResponse);
            }
            Profile profile = (Profile)requestUser;
            profile.IsFollowing = true;
            ProfileHTTPTransferObject profileHTTPTransferObject = new ProfileHTTPTransferObject();
            profileHTTPTransferObject.Profile = profile;
            return Ok(profile);
        }

        //Unollow a user
        [HttpDelete("/api/profiles/{username}/follow")]
        public async Task<IActionResult> unfollowUser(string username)
        {
            var authedUser = HttpContext.User;
            if (!authedUser.HasClaim(c =>
             c.Type == ClaimTypes.NameIdentifier))
            {
                this.HttpContext.Response.StatusCode = 401;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Missing authentication");
                return Json(authErrorResponse);
            }
            string authUsername = authedUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser authedUserInDB = await _userManager.FindByNameAsync(authUsername);
            if (null == authedUserInDB)
            {
                this.HttpContext.Response.StatusCode = 404;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Authenticated user not in database");
                return Json(authErrorResponse);
            }
            ApplicationUser requestUser = await _userManager.FindByNameAsync(username);
            if (null == requestUser)
            {
                this.HttpContext.Response.StatusCode = 404;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Requested user not in database");
                return Json(authErrorResponse);
            }
            string authedUserId = authedUserInDB.Id;
            string requestUserId = requestUser.Id;
            try
            {
                UserIsFollowing userIsFollowing = new UserIsFollowing();
                userIsFollowing.UserId = authedUserId;
                userIsFollowing.IsFollowingId = requestUserId;
                _context.UserIsFollowing.Remove(userIsFollowing);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                this.HttpContext.Response.StatusCode = 422;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey($"{authUsername} is already not following {username}");
                return Json(authErrorResponse);
            }
            Profile profile = (Profile)requestUser;
            profile.IsFollowing = false;
            ProfileHTTPTransferObject profileHTTPTransferObject = new ProfileHTTPTransferObject();
            profileHTTPTransferObject.Profile = profile;
            return Ok(profile);
        }
    }
}