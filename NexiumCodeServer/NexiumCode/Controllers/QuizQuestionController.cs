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

        public QuizQuestionController(IQuizQuestionRepository quizQuestionRepository)
        {
            _quizQuestionRepository = quizQuestionRepository;
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

            if (questionId >= 1 && questionId <= 7)
            {
                var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CSharpCourse.json");
                if (!System.IO.File.Exists(jsonPath))
                {
                    return NotFound(new { Message = "Course JSON not found." });
                }

                var jsonString = await System.IO.File.ReadAllTextAsync(jsonPath);
                var course = JsonSerializer.Deserialize<CourseJson>(jsonString);
                var question = course.Lessons
                    .SelectMany(l => l.QuizQuestions ?? new List<QuizQuestionJson>())
                    .FirstOrDefault(q => q.Id == questionId);

                if (question == null)
                {
                    return NotFound(new { Message = "Quiz question not found." });
                }

                bool isCorrect = request.Answer == question.CorrectAnswer;
                return Ok(new { IsCorrect = isCorrect, CorrectAnswer = isCorrect ? null : question.CorrectAnswer });
            }

            var dbQuestion = await _quizQuestionRepository.GetById(questionId);
            if (dbQuestion == null)
            {
                return NotFound(new { Message = "Quiz question not found." });
            }

            bool isCorrectDb = request.Answer == dbQuestion.CorrectAnswer;
            return Ok(new { IsCorrect = isCorrectDb, CorrectAnswer = isCorrectDb ? null : dbQuestion.CorrectAnswer });
        }
    }
}