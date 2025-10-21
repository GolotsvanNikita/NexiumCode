using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexiumCode.DTO;
using NexiumCode.Repositories;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexiumCode.JSON;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IProgressRepository _progressRepository;
        private readonly ILogger<CourseController> _logger;
        private readonly IWebHostEnvironment _env;

        public CourseController(
            ICourseRepository courseRepository,
            ILessonRepository lessonRepository,
            IProgressRepository progressRepository,
            ILogger<CourseController> logger,
            IWebHostEnvironment env)
        {
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _progressRepository = progressRepository;
            _logger = logger;
            _env = env;
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourse(int courseId, [FromQuery] int userId)
        {
            if (courseId == 1)
            {
                var jsonPath = Path.Combine(_env.WebRootPath, "CSharpCourse.json");
                _logger.LogInformation($"Checking file at: {jsonPath}");

                if (!System.IO.File.Exists(jsonPath))
                {
                    _logger.LogError("Course JSON file not found.");
                    return NotFound(new { Message = "Course JSON not found. Path checked: " + jsonPath });
                }

                try
                {
                    var jsonString = await System.IO.File.ReadAllTextAsync(jsonPath);
                    _logger.LogInformation("Successfully read JSON file.");

                    var course = JsonSerializer.Deserialize<CourseJson>(jsonString, JsonOptions);

                    var progress = await _progressRepository.GetProgressByUserAndCourse(userId, courseId);
                    var theoryProgress = progress?.TheoryProgress ?? 0;

                    return Ok(new
                    {
                        course,
                        TheoryProgress = theoryProgress,
                        IsPracticeUnlocked = theoryProgress == 100
                    });
                }
                catch (JsonException ex)
                {
                    _logger.LogError("Deserialization failed: {ex.Message}", ex);
                    return StatusCode(500, new { Message = "Error deserializing course data." });
                }
            }

            return NotFound();
        }

        [HttpGet("{courseId}/lesson/{lessonId}")]
        public async Task<IActionResult> GetLesson(int courseId, int lessonId, [FromQuery] int userId)
        {
            _logger.LogInformation($"GetLesson called: courseId={courseId}, lessonId={lessonId}, userId={userId}");

            var jsonPath = Path.Combine(_env.WebRootPath, "CSharpCourse.json");
            _logger.LogInformation($"Checking file at: {jsonPath}");

            if (!System.IO.File.Exists(jsonPath))
            {
                _logger.LogError("Course JSON file not found.");
                return NotFound(new { Message = "Course JSON not found. Path checked: " + jsonPath });
            }

            try
            {
                var jsonString = await System.IO.File.ReadAllTextAsync(jsonPath);
                _logger.LogInformation($"JSON file read, length: {jsonString.Length}");

                var course = JsonSerializer.Deserialize<CourseJson>(jsonString, JsonOptions);

                if (course == null)
                {
                    _logger.LogError("Course deserialization returned null");
                    return StatusCode(500, new { Message = "Failed to deserialize course data." });
                }

                if (course.Lessons == null || !course.Lessons.Any())
                {
                    _logger.LogError("Course.Lessons is null or empty");
                    return StatusCode(500, new { Message = "Course has no lessons." });
                }

                _logger.LogInformation($"Course deserialized, lessons count: {course.Lessons.Count}");

                var lesson = course.Lessons.FirstOrDefault(l => l.Id == lessonId);
                if (lesson == null)
                {
                    _logger.LogWarning($"Lesson with ID {lessonId} not found for course {courseId}.");
                    return NotFound(new { Message = "Lesson not found." });
                }

                _logger.LogInformation($"Lesson found: {lesson.Title}");

                var progress = await _progressRepository.GetProgressByUserAndCourse(userId, courseId);
                var theoryProgress = progress?.TheoryProgress ?? 0;

                return Ok(new
                {
                    lesson.Id,
                    lesson.Title,
                    lesson.Content,
                    lesson.IsTheory,
                    lesson.Order,
                    lesson.StarterCode,
                    QuizQuestions = lesson.QuizQuestions?.Select(q => new { q.Id, q.QuestionText, q.Options, q.CorrectAnswer, q.Explanation }),
                    TheoryProgress = theoryProgress,
                    IsPracticeUnlocked = theoryProgress == 100
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Deserialization failed: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { Message = "Error deserializing lesson data.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { Message = "Unexpected error.", Details = ex.Message });
            }
        }
    }
}