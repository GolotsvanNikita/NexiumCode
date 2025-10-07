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
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IProgressRepository _progressRepository;
        private readonly ILogger<CourseController> _logger;

        public CourseController(
            ICourseRepository courseRepository,
            ILessonRepository lessonRepository,
            IProgressRepository progressRepository,
            ILogger<CourseController> logger)
        {
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _progressRepository = progressRepository;
            _logger = logger;
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourse(int courseId, [FromQuery] int userId)
        {
            if (courseId == 1)
            {
                var currentDir = Directory.GetCurrentDirectory();
                var jsonPath = Path.Combine(currentDir, "wwwroot", "CSharpCourse.json");
                _logger.LogInformation($"Current directory: {currentDir}");
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
                    var course = JsonSerializer.Deserialize<object>(jsonString);

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
                    _logger.LogError($"JSON deserialization failed: {ex.Message}");
                    return StatusCode(500, new { Message = "Error deserializing course data." });
                }
            }

            var dbCourse = await _courseRepository.GetCourseWithLessons(courseId);
            if (dbCourse == null)
            {
                _logger.LogWarning($"Course with ID {courseId} not found in database.");
                return NotFound(new { Message = "Course not found." });
            }

            var progressDb = await _progressRepository.GetProgressByUserAndCourse(userId, courseId);
            var theoryProgressDb = progressDb?.TheoryProgress ?? 0;

            var lessons = await _lessonRepository.GetLessonsByCourse(courseId);
            var response = new
            {
                dbCourse.Id,
                dbCourse.Name,
                dbCourse.Description,
                TheoryProgress = theoryProgressDb,
                IsPracticeUnlocked = theoryProgressDb == 100,
                Lessons = lessons.Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.IsTheory,
                    l.Order,
                    IsAccessible = l.IsTheory || theoryProgressDb == 100
                }).OrderBy(l => l.Order)
            };

            return Ok(response);
        }

        [HttpGet("{courseId}/lesson/{lessonId}")]
        public async Task<IActionResult> GetLesson(int courseId, int lessonId, [FromQuery] int userId)
        {
            if (courseId == 1)
            {
                var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CSharpCourse.json");
                _logger.LogInformation($"Checking file at: {jsonPath}");

                if (!System.IO.File.Exists(jsonPath))
                {
                    _logger.LogError("JSON not found.");
                    return NotFound(new { Message = "JSON not found." });
                }

                try
                {
                    var jsonString = await System.IO.File.ReadAllTextAsync(jsonPath);
                    var course = JsonSerializer.Deserialize<CourseJson>(jsonString);
                    var lesson = course.Lessons.FirstOrDefault(l => l.Id == lessonId);

                    if (lesson == null || lesson.CourseId != courseId)
                    {
                        _logger.LogWarning($"Lesson with ID {lessonId} not found for course {courseId}.");
                        return NotFound(new { Message = "Lesson not found." });
                    }

                    var progress = await _progressRepository.GetProgressByUserAndCourse(userId, courseId);
                    var theoryProgress = progress?.TheoryProgress ?? 0;

                    if (!lesson.IsTheory && theoryProgress < 100)
                    {
                        return StatusCode(403, new { Message = "Practice is locked until theory is 100% complete." });
                    }

                    return Ok(new
                    {
                        lesson.Id,
                        lesson.Title,
                        lesson.Content,
                        lesson.IsTheory,
                        lesson.Order,
                        QuizQuestions = lesson.IsTheory ? lesson.QuizQuestions?.Select(q => new
                        {
                            q.Id,
                            q.QuestionText,
                            q.Options
                        }) : null,
                        PracticeTasks = !lesson.IsTheory ? lesson.PracticeTasks?.Select(t => new
                        {
                            t.Id,
                            t.TaskDescription,
                            t.StarterCode,
                            t.TestCases,
                            t.AverageTimeSeconds
                        }) : null
                    });
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"JSON deserialization failed: {ex.Message}");
                    return StatusCode(500, new { Message = "Error deserializing lesson data." });
                }
            }

            var dbLesson = await _lessonRepository.GetLessonWithDetails(lessonId);
            if (dbLesson == null || dbLesson.CourseId != courseId)
            {
                _logger.LogWarning($"Lesson with ID {lessonId} not found for course {courseId}.");
                return NotFound(new { Message = "Lesson not found." });
            }

            var progressDb = await _progressRepository.GetProgressByUserAndCourse(userId, courseId);
            var theoryProgressDb = progressDb?.TheoryProgress ?? 0;

            if (!dbLesson.IsTheory && theoryProgressDb < 100)
            {
                return StatusCode(403, new { Message = "Practice is locked until theory is 100% complete." });
            }

            var response = new
            {
                dbLesson.Id,
                dbLesson.Title,
                dbLesson.Content,
                dbLesson.IsTheory,
                dbLesson.Order,
                QuizQuestions = dbLesson.IsTheory ? dbLesson.QuizQuestions?.Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.Options
                }) : null,
                PracticeTasks = !dbLesson.IsTheory ? dbLesson.PracticeTasks?.Select(t => new
                {
                    t.Id,
                    t.TaskDescription,
                    t.StarterCode,
                    t.TestCases,
                    t.AverageTimeSeconds
                }) : null
            };

            return Ok(response);
        }

        [HttpPost("{courseId}/progress/theory")]
        public async Task<IActionResult> UpdateTheoryProgress(int courseId, [FromBody] UpdateProgressDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _progressRepository.GetById(request.UserId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var course = await _courseRepository.GetById(courseId);
            if (course == null && courseId != 1)
            {
                return NotFound(new { Message = "Course not found." });
            }

            await _progressRepository.UpdateTheoryProgress(request.UserId, courseId, request.Progress);
            await _progressRepository.SaveChanges();

            return Ok(new { Message = "Theory progress updated." });
        }
    }
}