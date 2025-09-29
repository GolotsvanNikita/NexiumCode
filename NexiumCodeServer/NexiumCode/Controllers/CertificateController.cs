using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NexiumCode.DTO;
using NexiumCode.Models;
using NexiumCode.Repositories;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IProgressRepository _progressRepository;
        private readonly ICourseRepository _courseRepository;

        public CertificateController(
        ICertificateRepository certificateRepository,
        IProgressRepository progressRepository,
        ICourseRepository courseRepository,
        IConfiguration configuration)
        {
            _certificateRepository = certificateRepository;
            _progressRepository = progressRepository;
            _courseRepository = courseRepository;
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

            return Ok(new
            {
                Message = "Certificate issued.",
                CertificateId = certificate.Id,
                certificate.CertificateUrl
            });
        }
    }
}
