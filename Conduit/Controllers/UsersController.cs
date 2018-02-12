using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Conduit.Models;
using Conduit.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Conduit.Controllers
{
    [Produces("application/json")]
    public class UsersController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        // Authenticate a user
        // POST /api/user/login
        [HttpPost("/api/users/login")]
        public async Task<IActionResult> Authenticate([FromBody] ApplicationUser user)
        {
            ApplicationUser signingInUser = await _userManager.FindByNameAsync(user.UserName);
            var result = await _signInManager.PasswordSignInAsync(signingInUser, user.Password, false, false);
            if(result.Succeeded)
            {
                user.Token = BuildToken(user);
                user.Password = null;
                return Ok(user);
            }
            this.HttpContext.Response.StatusCode = 422;
            var errorResponse = new ErrorResponse();
            errorResponse.addErrorKey($"An error occured trying to sign in {user.UserName}");
            return Json(errorResponse);
        }

        private string BuildToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["JWTIssuer"],
                _config["JWTIssuer"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Register a user
        // POST /api/user
        [HttpPost("/api/users")]
        [RegisterUserValidator]
        public async Task<IActionResult> Register([FromBody] ApplicationUser user)
        {
            var result = await _userManager.CreateAsync(user, user.Password);
            if(result.Succeeded)
            {
                user.Password = null; // Don't return password
                return Ok(user);
            }
            this.HttpContext.Response.StatusCode = 422;
            var errorResponse = new ErrorResponse();
            foreach (var error in result.Errors)
            {
                errorResponse.addErrorKey(error.Description);
            }
            return Json(errorResponse);
        }

        // Get a user
        // GET /api/user
        [HttpGet("/api/user")]
        public async Task<IActionResult> GetUser()
        {
            var user = HttpContext.User;
            if(!user.HasClaim(c => 
            c.Type == ClaimTypes.NameIdentifier))
            {
                this.HttpContext.Response.StatusCode = 401;
                var errorResponse = new ErrorResponse();
                errorResponse.addErrorKey("Missing authentication");
                return Json(errorResponse);
            }
            string authUsername = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _userManager.FindByNameAsync(authUsername);
            if(null == result.UserName)
            {
                this.HttpContext.Response.StatusCode = 404;
                var errorResponse = new ErrorResponse();
                errorResponse.addErrorKey("No user found with the specified username");
                return Json(errorResponse);
            }
            return Ok(result);

        }

        // Edit a user
        // PUT /api/user
        [HttpPut("/api/user")]
        public async Task<IActionResult> updateUser([FromBody]ApplicationUser user)
        {
            var tokenUser = HttpContext.User;
            if (!tokenUser.HasClaim(c =>
             c.Type == ClaimTypes.NameIdentifier))
            {
                this.HttpContext.Response.StatusCode = 401;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Missing authentication");
                return Json(authErrorResponse);
            }
            string authUsername = tokenUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var findUserResult = await _userManager.FindByNameAsync(authUsername);
            if(null == findUserResult.UserName)
            {
                this.HttpContext.Response.StatusCode = 404;
                var noUserErrorResponse = new ErrorResponse();
                noUserErrorResponse.addErrorKey("No user found with the specified username");
                return Json(noUserErrorResponse);
            }
            findUserResult.Image = !String.IsNullOrWhiteSpace(user.Image) ? user.Image : findUserResult.Image;
            findUserResult.Bio = !String.IsNullOrWhiteSpace(user.Bio) ? user.Bio : findUserResult.Bio;
            findUserResult.Email = !String.IsNullOrWhiteSpace(user.Email) ? user.Email : findUserResult.Email;
            var updateUserResult = await _userManager.UpdateAsync(findUserResult);
            if(updateUserResult.Succeeded)
            {
                user = findUserResult;
                user.Password = null;
                return Ok(user);
            }
            this.HttpContext.Response.StatusCode = 422;
            var updateUserErrorResponse = new ErrorResponse();
            foreach (var error in updateUserResult.Errors)
            {
                updateUserErrorResponse.addErrorKey(error.Description);
            }
            return Json(updateUserErrorResponse);
        }
    }
}