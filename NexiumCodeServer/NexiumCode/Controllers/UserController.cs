using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NexiumCode.DTO;
using NexiumCode.Models;
using NexiumCode.Repositories;
using NexiumCode.Services;
using System.Linq;
using System.Text.Json;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICertificateRepository _certificateRepository;
        private readonly IPasswordHash _hasher;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            ICertificateRepository certificateRepository,
            IPasswordHash hasher,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _certificateRepository = certificateRepository;
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
                    AvatarUrl = "/images/avatars/default-avatar.png",
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
                return Ok(new { UserId = user.Id, Username = user.Username, AvatarUrl = user.AvatarUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return StatusCode(500, new { Message = "Internal server error during login.", Details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _userRepository.GetById(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", id);
                    return NotFound(new { Message = "User not found" });
                }

                return Ok(new
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Rating = user.Rating,
                    AvatarUrl = user.AvatarUrl,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user: {UserId}", id);
                return StatusCode(500, new { Message = "Internal server error", Details = ex.Message });
            }
        }

        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            try
            {
                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return NotFound(new { Message = "User not found." });
                }

                var certificates = await _certificateRepository.GetCertificatesByUser(userId);

                List<string> achievements;
                if (string.IsNullOrWhiteSpace(user.Achievements))
                {
                    achievements = [];
                }
                else
                {
                    try
                    {
                        achievements = JsonSerializer.Deserialize<List<string>>(user.Achievements) ?? [];
                    }
                    catch (JsonException)
                    {
                        achievements = [];
                        _logger.LogWarning("Failed to deserialize achievements for user {UserId}, returning empty list", userId);
                    }
                }

                int xpForNextLevel = (int)(100 * user.Level * 1.15);

                return Ok(new
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Rating = user.Rating,
                    Level = user.Level,
                    CurrentXP = user.CurrentXP,
                    XPToNextLevel = xpForNextLevel,
                    TotalXP = user.TotalXP,
                    AvatarUrl = user.AvatarUrl,
                    Streak = user.CurrentStreak,
                    SkillTree = new
                    {
                        TheoryMaster = user.TheoryMasterProgress,
                        TheoryMasterRank = user.TheoryMasterRank,
                        PracticePro = user.PracticeProProgress,
                        PracticeProRank = user.PracticeProRank,
                        QuizChampion = user.QuizChampionProgress,
                        QuizChampionRank = user.QuizChampionRank,
                        CommunityStar = user.CommunityStarProgress,
                        CommunityStarRank = user.CommunityStarRank
                    },
                    Achievements = achievements,
                    Certificates = certificates.Select(c => new
                    {
                        c.Id,
                        c.CourseId,
                        CourseName = c.Course?.Name ?? "Unknown Course",
                        c.CertificateUrl,
                        IssueDate = c.IssueDate
                    }).ToList()
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

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar, [FromQuery] int userId)
        {
            if (avatar == null || avatar.Length == 0)
            {
                return BadRequest(new { Message = "No file uploaded" });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(avatar.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { Message = "Invalid file type. Only jpg, jpeg, png, gif allowed" });
            }

            if (avatar.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { Message = "File too large. Max 5MB" });
            }

            var user = await _userRepository.GetById(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var avatarsPath = Path.Combine("wwwroot", "images", "avatars");
            if (!Directory.Exists(avatarsPath))
            {
                Directory.CreateDirectory(avatarsPath);
            }

            if (!string.IsNullOrEmpty(user.AvatarUrl) && user.AvatarUrl != "/images/avatars/default-avatar.png")
            {
                var oldPath = Path.Combine("wwwroot", user.AvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(avatarsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            user.AvatarUrl = $"/images/avatars/{fileName}";
            await _userRepository.Update(user);
            await _userRepository.SaveChanges();

            return Ok(new { AvatarUrl = user.AvatarUrl });
        }
    }
}