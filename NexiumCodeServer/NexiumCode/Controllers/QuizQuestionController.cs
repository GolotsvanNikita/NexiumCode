using Microsoft.AspNetCore.Mvc;
using NexiumCode.DTO;
using NexiumCode.Repositories;

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

            var question = await _quizQuestionRepository.GetById(questionId);
            if (question == null)
            {
                return NotFound(new { Message = "Quiz question not found." });
            }

            bool isCorrect = request.Answer == question.CorrectAnswer;


            return Ok(new { IsCorrect = isCorrect, CorrectAnswer = isCorrect ? null : question.CorrectAnswer });
        }
    }
}

