using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NexiumCode.DTO;
using NexiumCode.Models;
using NexiumCode.Repositories;
using NexiumCode.Services;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IProgressRepository _progressRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IXPService _xpService;

        public CertificateController(
            ICertificateRepository certificateRepository,
            IProgressRepository progressRepository,
            ICourseRepository courseRepository,
            IXPService xpService,
            IConfiguration configuration)
        {
            _certificateRepository = certificateRepository;
            _progressRepository = progressRepository;
            _courseRepository = courseRepository;
            _xpService = xpService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCertificates([FromQuery] int userId)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId != userId)
            {
                return Unauthorized(new { Message = "Invalid user ID." });
            }

            var certificates = await _certificateRepository.GetCertificatesByUser(userId);
            var response = certificates.Select(c => new
            {
                c.Id,
                c.CourseId,
                CourseName = c.Course?.Name,
                c.CertificateUrl,
                c.IssueDate
            });

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> IssueCertificate([FromBody] IssueCertificateDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var progress = await _progressRepository.GetProgressByUserAndCourse(request.UserId, request.CourseId);
            if (progress == null || progress.TheoryProgress < 100 || progress.PracticeProgress < 100)
            {
                return BadRequest(new { Message = "Course not fully completed." });
            }

            var course = await _courseRepository.GetById(request.CourseId);
            if (course == null)
            {
                return NotFound(new { Message = "Course not found." });
            }

            var existingCertificate = await _certificateRepository.GetCertificateByUserAndCourse(request.UserId, request.CourseId);
            if (existingCertificate != null)
            {
                return Conflict(new { Message = "Certificate already issued." });
            }

            var certificate = new Certificate
            {
                UserId = request.UserId,
                CourseId = request.CourseId,
                CertificateUrl = $"/certificates/course_{request.CourseId}.png",
                IssueDate = DateTimeOffset.UtcNow
            };

            await _certificateRepository.Add(certificate);
            await _certificateRepository.SaveChanges();

            await _xpService.AddXP(request.UserId, 100, "all", "Certificate earned");
            await _xpService.AddELO(request.UserId, 30, "Certificate earned");
            await _xpService.AddAchievement(request.UserId, $"Certificate - {course.Name}");

            return Ok(new
            {
                Message = "Certificate issued.",
                CertificateId = certificate.Id,
                certificate.CertificateUrl
            });
        }
    }
}