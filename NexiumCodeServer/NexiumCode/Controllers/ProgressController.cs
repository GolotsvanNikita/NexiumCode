using Microsoft.AspNetCore.Mvc;
using NexiumCode.DTO;
using NexiumCode.Repositories;
using NexiumCode.Services;
using System.Threading.Tasks;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressRepository _progressRepository;
        private readonly IXPService _xpService;
        private readonly ILogger<ProgressController> _logger;

        public ProgressController(
            IProgressRepository progressRepository,
            IXPService xpService,
            ILogger<ProgressController> logger)
        {
            _progressRepository = progressRepository;
            _xpService = xpService;
            _logger = logger;
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetProgress(int courseId, [FromQuery] int userId)
        {
            var progress = await _progressRepository.GetProgressByUserAndCourse(userId, courseId);
            if (progress == null)
            {
                return Ok(new { TheoryProgress = 0, PracticeProgress = 0 });
            }
            return Ok(new { progress.TheoryProgress, progress.PracticeProgress });
        }

        [HttpPost("{courseId}/theory")]
        public async Task<IActionResult> UpdateTheoryProgress(int courseId, [FromBody] UpdateProgressDTO request)
        {
            _logger.LogInformation($"UpdateTheoryProgress called: courseId={courseId}, userId={request.UserId}, progress={request.Progress}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Invalid model state: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(ModelState);
            }

            try
            {
                await _progressRepository.UpdateTheoryProgress(request.UserId, courseId, request.Progress);
                await _progressRepository.SaveChanges();

                await _xpService.AddXP(request.UserId, 15, "theory", "Completed theory lesson");
                await _xpService.AddELO(request.UserId, 5, "Completed theory lesson");
                await _xpService.UpdateStreak(request.UserId);

                await _xpService.UpdateSkillProgress(request.UserId, "theory", 20);

                _logger.LogInformation($"Theory progress updated successfully for user {request.UserId}, course {courseId}, progress {request.Progress}%");
                return Ok(new { Message = "Theory progress updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating theory progress for user {request.UserId}");
                return StatusCode(500, new { Message = "Error updating progress.", Details = ex.Message });
            }
        }

        [HttpPost("{courseId}/practice")]
        public async Task<IActionResult> UpdatePracticeProgress(int courseId, [FromBody] UpdateProgressDTO request)
        {
            _logger.LogInformation($"UpdatePracticeProgress called: courseId={courseId}, userId={request.UserId}, progress={request.Progress}");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _progressRepository.UpdatePracticeProgress(request.UserId, courseId, request.Progress);
                await _progressRepository.SaveChanges();

                await _xpService.AddXP(request.UserId, 35, "practice", "Completed practice task");
                await _xpService.AddELO(request.UserId, 15, "Completed practice task");
                await _xpService.UpdateStreak(request.UserId);

                _logger.LogInformation($"Practice progress updated successfully for user {request.UserId}");
                return Ok(new { Message = "Practice progress updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating practice progress for user {request.UserId}");
                return StatusCode(500, new { Message = "Error updating progress.", Details = ex.Message });
            }
        }

        [HttpPost("{courseId}/lesson/{lessonId}")]
        public async Task<IActionResult> UpdateLessonProgress(int courseId, int lessonId, [FromBody] UpdateProgressDTO request)
        {
            _logger.LogInformation($"UpdateLessonProgress called: courseId={courseId}, lessonId={lessonId}, userId={request.UserId}, progress={request.Progress}");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _progressRepository.UpdateLessonProgress(request.UserId, courseId, lessonId, request.Progress);
                await _progressRepository.SaveChanges();

                _logger.LogInformation($"Lesson progress updated successfully");
                return Ok(new { Message = "Lesson progress updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating lesson progress");
                return StatusCode(500, new { Message = "Error updating progress.", Details = ex.Message });
            }
        }
    }
}