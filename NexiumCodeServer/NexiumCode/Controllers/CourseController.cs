using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexiumCode.DTO;
using NexiumCode.Repositories;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IProgressRepository _progressRepository;

        public CourseController(
            ICourseRepository courseRepository,
            ILessonRepository lessonRepository,
            IProgressRepository progressRepository)
        {
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _progressRepository = progressRepository;
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourse(int courseId, [FromQuery] int userId)
        {
            var course = await _courseRepository.GetCourseWithLessons(courseId);
            if (course == null)
            {
                return NotFound(new { Message = "Course not found." });
            }

            var progress = await _progressRepository.GetProgressByUserAndCourse(userId, courseId);
            var theoryProgress = progress?.TheoryProgress ?? 0;

            var lessons = await _lessonRepository.GetLessonsByCourse(courseId);
            var response = new
            {
                course.Id,
                course.Name,
                course.Description,
                TheoryProgress = theoryProgress,
                IsPracticeUnlocked = theoryProgress == 100,
                Lessons = lessons.Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.IsTheory,
                    l.Order,
                    IsAccessible = l.IsTheory || theoryProgress == 100
                }).OrderBy(l => l.Order)
            };

            return Ok(response);
        }

        [HttpGet("{courseId}/lesson/{lessonId}")]
        public async Task<IActionResult> GetLesson(int courseId, int lessonId, [FromQuery] int userId)
        {
            var progress = await _progressRepository.GetProgressByUserAndCourse(userId, courseId);
            var theoryProgress = progress?.TheoryProgress ?? 0;

            var lesson = await _lessonRepository.GetLessonWithDetails(lessonId);
            if (lesson == null || lesson.CourseId != courseId)
            {
                return NotFound(new { Message = "Lesson not found." });
            }

            if (!lesson.IsTheory && theoryProgress < 100)
            {
                return StatusCode(403, new { Message = "Practice is locked until theory is 100% complete." });
            }

            var response = new
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
            if (course == null)
            {
                return NotFound(new { Message = "Course not found." });
            }

            await _progressRepository.UpdateTheoryProgress(request.UserId, courseId, request.Progress);
            await _progressRepository.SaveChanges();

            return Ok(new { Message = "Theory progress updated." });
        }
    }
}
