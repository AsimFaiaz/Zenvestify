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

			string hashedPassword = _passwordHasher.HashPassword(null, request.Password);

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
			Console.WriteLine($"[AuthController.Login] Login attempt: {request.Email}");

			var user = await _userRepository.GetUserByEmailAsync(request.Email);

			Console.WriteLine($"[AuthController.Login] User found? {user != null}");


			if (user == null)
			{
				return Unauthorized(new { message = "Invalid email or password." });
			}

			var verify = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, request.Password);

			Console.WriteLine($"[AuthController.Login] Password check: {verify}");

			if (verify != PasswordVerificationResult.Success)
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


			Console.WriteLine($"[AuthController.Login] Token issued: {token}...");


			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return Ok(new
			{
				Token = tokenString
			});
		}

		// POST /api/auth/forgotpassword
		[HttpPost("forgotpassword")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
		{
			var user = await _userRepository.GetUserByEmailAsync(dto.Email);
			// Always return OK
			if (user == null) return Ok();

			// Generate a secure random token
			var tokenBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
			var token = Convert.ToBase64String(tokenBytes);

			var expires = DateTime.UtcNow.AddMinutes(30);
			await _userRepository.InsertPasswordResetTokenAsync(user.Id, token, expires);

			// TODO: send email with link:
			// e.g., https://yourapp/resetpassword?token=URLENCODE(token)

			return Ok();
		}


		// POST /api/auth/resetpassword
		[HttpPost("resetpassword")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
		{
			var (userId, valid) = await _userRepository.ValidatePasswordResetTokenAsync(dto.Token);
			if (!valid) return BadRequest("Invalid or expired token.");

			// hash the new password
			var hashed = _passwordHasher.HashPassword(null, dto.NewPassword);

			await _userRepository.UpdatePasswordHashAsync(userId, hashed);
			await _userRepository.MarkPasswordResetTokenUsedAsync(dto.Token);

			return Ok("Password updated.");
		}

		[HttpGet("me")]
		[Authorize] 
		public async Task<IActionResult> Me()
		{

			Console.WriteLine("[AuthController.Me] Hit /api/auth/me");

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var email = User.FindFirst(ClaimTypes.Email)?.Value;
			var name = User.FindFirst(ClaimTypes.Name)?.Value;

			Console.WriteLine($"[AuthController.Me] Claims => Id={userId}, Email={email}, Name={name}");

			if (string.IsNullOrEmpty(userId))
				return Unauthorized();

			var user = await _userRepository.GetUserByIdAsync(Guid.Parse(userId));

			Console.WriteLine($"[AuthController.Me] DB User => {user?.FullName}");

			if (user == null || !user.isActive) return NotFound();

			Console.WriteLine("JWT NameIdentifier: " + userId);

			return Ok(new
			{
				user.Id,
				user.FullName,
				user.Email
			});


			
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

	//dto forgot and reset password
	public class ForgotPasswordDto 
	{ 
		public string Email { get; set; } = ""; 
	}
	public class ResetPasswordDto 
	{ 
		public string Token { get; set; } = "";
		public string NewPassword { get; set; } = "";
	}

}