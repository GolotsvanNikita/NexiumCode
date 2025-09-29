using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NexiumCode.DTO;
using NexiumCode.Models;
using NexiumCode.Repositories;
using NexiumCode.Services;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHash _hasher;

        public UserController(IUserRepository userRepository, IPasswordHash hasher)
        {
            _userRepository = userRepository;
            _hasher = hasher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userRepository.GetByEmail(request.Email);
            if (existingUser != null)
            {
                return Conflict(new { Message = "Email already exists." });
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _userRepository.AddUser(user, request.Password);
            await _userRepository.SaveChanges();

            return Ok(new { UserId = user.Id, Username = user.Username });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userRepository.GetByEmail(request.Email);

            if (user == null || !_hasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            return Ok(new { UserId = user.Id, Username = user.Username });
        }

        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var user = await _userRepository.GetProfile(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Rating,
                Certificates = user.Certificates?.Select(c => new
                {
                    c.Id,
                    c.CourseId,
                    CourseName = c.Course?.Name,
                    c.CertificateUrl,
                    c.IssueDate
                })
            });
        }

        [HttpPut("rating/{userId}")]
        public async Task<IActionResult> UpdateRating(int userId, [FromBody] UpdateRatingDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (userId != request.UserId)
            {
                return StatusCode(403, new { Message = "Invalid user ID." });
            }    

            var user = await _userRepository.GetById(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            await _userRepository.UpdateRating(userId, request.Rating);
            await _userRepository.SaveChanges();

            return Ok(new { Message = "Rating updated." });
        }
    }
}
