using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly  ApplicationDbContext context;

        public UserLoginController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            this.context = context;
        }

            [HttpPost("login")]
            public async Task<IActionResult> Login(LoginDto dto)
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);

                if (user == null)
                    return Unauthorized("Invalid email or password");

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Unauthorized("This account has been deleted or suspended.");
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);

                if (!validPassword)
                    return Unauthorized("Invalid email or password");

                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.Id),
                 new Claim(ClaimTypes.Email, user.Email ?? ""),
                 new Claim("FullName", user.FullName ?? "User"),
                 new Claim("Governorate", user.Governorate ?? "Not Specified")
            };

                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    role = roles.First()
                });
            }
    }
}