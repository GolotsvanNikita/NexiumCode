using Microsoft.AspNetCore.Mvc;
using NexiumCode.DTO;
using NexiumCode.Repositories;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using NexiumCode.JSON;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizQuestionController : ControllerBase
    {
        private readonly IQuizQuestionRepository _quizQuestionRepository;
        private readonly ILogger<QuizQuestionController> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public QuizQuestionController(
            IQuizQuestionRepository quizQuestionRepository,
            ILogger<QuizQuestionController> logger)
        {
            _quizQuestionRepository = quizQuestionRepository;
            _logger = logger;
        }

        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetQuizQuestions(int lessonId)
        {
            var questions = await _quizQuestionRepository.GetQuizQuestionsByLesson(lessonId);
            var response = questions.Select(q => new
            {
                q.Id,
                q.QuestionText,
                q.Options
            });

            return Ok(response);
        }

        [HttpPost("{questionId}/submit")]
        public async Task<IActionResult> SubmitQuizAnswer(int questionId, [FromBody] SubmitQuizAnswerDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (questionId >= 1 && questionId <= 7)
                {
                    var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CSharpCourse.json");
                    _logger.LogInformation($"Looking for JSON at: {jsonPath}");

                    if (!System.IO.File.Exists(jsonPath))
                    {
                        _logger.LogError("Course JSON file not found.");
                        return NotFound(new { Message = "Course JSON not found." });
                    }

                    var jsonString = await System.IO.File.ReadAllTextAsync(jsonPath);
                    _logger.LogInformation($"JSON file read, length: {jsonString.Length}");

                    var course = JsonSerializer.Deserialize<CourseJson>(jsonString, JsonOptions);

                    if (course == null)
                    {
                        _logger.LogError("Course deserialization returned null");
                        return StatusCode(500, new { Message = "Failed to deserialize course data." });
                    }

                    if (course.Lessons == null)
                    {
                        _logger.LogError("Course.Lessons is null");
                        return StatusCode(500, new { Message = "Course has no lessons." });
                    }

                    _logger.LogInformation($"Course deserialized, lessons count: {course.Lessons.Count}");

                    QuizQuestionJson question = null;
                    foreach (var lesson in course.Lessons)
                    {
                        if (lesson.QuizQuestions != null)
                        {
                            question = lesson.QuizQuestions.FirstOrDefault(q => q.Id == questionId);
                            if (question != null)
                            {
                                _logger.LogInformation($"Found question in lesson: {lesson.Title}");
                                break;
                            }
                        }
                    }

                    if (question == null)
                    {
                        _logger.LogWarning($"Question with ID {questionId} not found");
                        return NotFound(new { Message = "Quiz question not found." });
                    }

                    bool isCorrect = request.Answer?.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase) ?? false;
                    _logger.LogInformation($"Answer check: submitted={request.Answer}, correct={question.CorrectAnswer}, isCorrect={isCorrect}");

                    return Ok(new
                    {
                        IsCorrect = isCorrect,
                        CorrectAnswer = isCorrect ? null : question.CorrectAnswer,
                        Explanation = isCorrect ? null : question.Explanation
                    });
                }

                var dbQuestion = await _quizQuestionRepository.GetById(questionId);
                if (dbQuestion == null)
                {
                    return NotFound(new { Message = "Quiz question not found." });
                }

                bool isCorrectDb = request.Answer == dbQuestion.CorrectAnswer;
                return Ok(new { IsCorrect = isCorrectDb, CorrectAnswer = isCorrectDb ? null : dbQuestion.CorrectAnswer });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error");
                return StatusCode(500, new { Message = "Error deserializing course data.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quiz answer");
                return StatusCode(500, new { Message = "Error processing answer.", Details = ex.Message });
            }
        }
    }
}