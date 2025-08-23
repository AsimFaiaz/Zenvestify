using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Zenvestify.Web.Configs;
using Zenvestify.Web.Data;
using Zenvestify.Web.Models;
using Microsoft.AspNetCore.Authorization;
namespace Zenvestify.Web.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly UserRepository _userRepository;
		private readonly PasswordHasher<string> _passwordHasher;
		private readonly JwtOptions _jwtOptions;

		public AuthController(UserRepository userRepository, IOptions<JwtOptions> jwtOptions)
		{
			_userRepository = userRepository;
			_passwordHasher = new PasswordHasher<string>();
			_jwtOptions = jwtOptions.Value;
		}

		//Post: api/auth/register
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
		{
			//check entry with email
			var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);

			if (existingUser != null)
			{
				return BadRequest("User already exists with this email.");
			}

			//hash password
			string hashedPassword = _passwordHasher.HashPassword(null, request.Password);

			//create new user
			var User = new User
			{
				FullName = request.FullName,
				Email = request.Email,
				PasswordHash = hashedPassword
			};
			
			await _userRepository.CreateAsync(User);

			return Ok("User Registered Successfully");
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] UserLoginDto request)
		{
			var user = await _userRepository.GetUserByEmailAsync(request.Email);

			if(user == null)
			{
				return Unauthorized(new { message = "Invalid email or password." });
			}

			var verify = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, request.Password);

			if(verify != PasswordVerificationResult.Success)
			{
				return Unauthorized(new { message = "Invalid email or password." });
			}

			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Name, user.FullName),
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _jwtOptions.Issuer,
				audience: _jwtOptions.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes),
				signingCredentials: creds
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return Ok(new
			{
				Token = tokenString
			});
		}

		[Authorize]
		[HttpGet("me")]
		public IActionResult Me()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var email = User.FindFirstValue(ClaimTypes.Email);
			var name = User.FindFirst("FullName")?.Value;

			return Ok(new {userId, email, name});
		}
	}

	//dto to keep request clean
	public class UserRegisterDto
	{
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}

	//dto for login
	public class UserLoginDto
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}
}