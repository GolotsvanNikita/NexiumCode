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
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, IPasswordHash hasher, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _hasher = hasher;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO request)
        {
            try
            {
                _logger.LogInformation("Register request received: {Email}", request.Email);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for register: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return BadRequest(ModelState);
                }

                var existingUser = await _userRepository.GetByEmail(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Email already exists: {Email}", request.Email);
                    return Conflict(new { Message = "Email already exists." });
                }

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _logger.LogInformation("Adding user: {Email}", request.Email);
                await _userRepository.AddUser(user, request.Password);
                _logger.LogInformation("Saving changes for user: {Email}", request.Email);
                await _userRepository.SaveChanges();

                _logger.LogInformation("User registered successfully: {UserId}", user.Id);
                return Ok(new { UserId = user.Id, Username = user.Username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return StatusCode(500, new { Message = "Internal server error during registration.", Details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            try
            {
                _logger.LogInformation("Login request received: {Email}", request.Email);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for login: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return BadRequest(ModelState);
                }

                var user = await _userRepository.GetByEmail(request.Email);
                if (user == null || !_hasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for email: {Email}", request.Email);
                    return Unauthorized(new { Message = "Invalid email or password." });
                }

                HttpContext.Session.SetInt32("UserId", user.Id);
                _logger.LogInformation("User logged in: {UserId}", user.Id);
                return Ok(new { UserId = user.Id, Username = user.Username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return StatusCode(500, new { Message = "Internal server error during login.", Details = ex.Message });
            }
        }

        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            try
            {
                var user = await _userRepository.GetProfile(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for user: {UserId}", userId);
                return StatusCode(500, new { Message = "Internal server error retrieving profile.", Details = ex.Message });
            }
        }

        [HttpPut("rating/{userId}")]
        public async Task<IActionResult> UpdateRating(int userId, [FromBody] UpdateRatingDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for updating rating: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return BadRequest(ModelState);
                }

                if (userId != request.UserId)
                {
                    _logger.LogWarning("Invalid user ID for rating update: {UserId}", userId);
                    return StatusCode(403, new { Message = "Invalid user ID." });
                }

                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for rating update: {UserId}", userId);
                    return NotFound(new { Message = "User not found." });
                }

                await _userRepository.UpdateRating(userId, request.Rating);
                await _userRepository.SaveChanges();

                _logger.LogInformation("Rating updated for user: {UserId}", userId);
                return Ok(new { Message = "Rating updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating for user: {UserId}", userId);
                return StatusCode(500, new { Message = "Internal server error updating rating.", Details = ex.Message });
            }
        }
    }
}
