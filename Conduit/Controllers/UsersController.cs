using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Conduit.Models;
using Conduit.Models.HTTPTransferObjects;
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
        public async Task<IActionResult> Authenticate([FromBody] UserHTTPTransferObject userHTTPTransferObject)
        {
            ApplicationUser user = userHTTPTransferObject.User;
            ApplicationUser signingInUser = await _userManager.FindByEmailAsync(user.Email);
            var result = await _signInManager.PasswordSignInAsync(signingInUser, user.Password, false, false);
            if(result.Succeeded)
            {
                signingInUser.Token = BuildToken(signingInUser);
                signingInUser.Password = null;
                userHTTPTransferObject.User = signingInUser;
                return Ok(userHTTPTransferObject);
            }
            this.HttpContext.Response.StatusCode = 422;
            var errorResponse = new ErrorResponse();
            errorResponse.addErrorKey($"An error occured trying to sign in {signingInUser.Email}");
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
        public async Task<IActionResult> Register([FromBody] UserHTTPTransferObject userHTTPTransferObject)
        {
            var user = userHTTPTransferObject.User;
            var result = await _userManager.CreateAsync(user, user.Password);
            if(result.Succeeded)
            {
                this.HttpContext.Response.StatusCode = 201;
                user.Password = null; // Don't return password
                userHTTPTransferObject.User = user;
                return Json(userHTTPTransferObject);
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
            var authedUser = HttpContext.User;
            if(!authedUser.HasClaim(c => 
            c.Type == ClaimTypes.NameIdentifier))
            {
                this.HttpContext.Response.StatusCode = 401;
                var errorResponse = new ErrorResponse();
                errorResponse.addErrorKey("Missing authentication");
                return Json(errorResponse);
            }
            string authUsername = authedUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByNameAsync(authUsername);
            if(null == user.UserName)
            {
                this.HttpContext.Response.StatusCode = 404;
                var errorResponse = new ErrorResponse();
                errorResponse.addErrorKey("No user found with the specified username");
                return Json(errorResponse);
            }
            UserHTTPTransferObject userHTTPTransferObject = new UserHTTPTransferObject();
            userHTTPTransferObject.User = user;
            return Ok(userHTTPTransferObject);

        }

        // Edit a user
        // PUT /api/user
        [HttpPut("/api/user")]
        public async Task<IActionResult> updateUser([FromBody] UserHTTPTransferObject userHTTPTransferObject)
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
            string authedUsername = authedUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var authedUserInDatabase = await _userManager.FindByNameAsync(authedUsername);
            if(null == authedUserInDatabase.UserName)
            {
                this.HttpContext.Response.StatusCode = 404;
                var noUserErrorResponse = new ErrorResponse();
                noUserErrorResponse.addErrorKey("No user found with the specified username");
                return Json(noUserErrorResponse);
            }
            ApplicationUser user = userHTTPTransferObject.User;
            authedUserInDatabase.Image = !String.IsNullOrWhiteSpace(user.Image) ? user.Image : authedUserInDatabase.Image;
            authedUserInDatabase.Bio = !String.IsNullOrWhiteSpace(user.Bio) ? user.Bio : authedUserInDatabase.Bio;
            authedUserInDatabase.Email = !String.IsNullOrWhiteSpace(user.Email) ? user.Email : authedUserInDatabase.Email;
            var updateUserResult = await _userManager.UpdateAsync(authedUserInDatabase);
            if(updateUserResult.Succeeded)
            {
                user = authedUserInDatabase;
                user.Password = null;
                userHTTPTransferObject.User = user;
                return Ok(userHTTPTransferObject);
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