using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Conduit.Models;
using Conduit.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Conduit.Controllers
{
    [Produces("application/json")]
    [Route("api/users")]
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
        [HttpPost("login")]
        [AuthenticateUserValidator]
        public async Task<IActionResult> Authenticate([FromBody] ApplicationUser user)
        {
            ApplicationUser signingInUser = await _userManager.FindByNameAsync(user.UserName);
            var result = await _signInManager.PasswordSignInAsync(signingInUser, user.Password, false, false);
            if(result.Succeeded)
            {
                user.Token = BuildToken(user);
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
        [HttpPost]
        [RegisterUserValidator]
        public async Task<IActionResult> Post([FromBody] ApplicationUser user)
        {
            var result = await _userManager.CreateAsync(user, user.Password);
            if(result.Succeeded)
            {
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
    }
}