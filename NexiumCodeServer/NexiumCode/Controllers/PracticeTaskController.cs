using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.IdentityModel.Tokens;
using NexiumCode.DTO;
using NexiumCode.Repositories;
using System.Reflection;

namespace NexiumCode.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeTaskController : ControllerBase
    {
        private readonly IPracticeTaskRepository _practiceTaskRepository;
        private readonly IProgressRepository _progressRepository;

        public PracticeTaskController(
            IPracticeTaskRepository practiceTaskRepository,
            IProgressRepository progressRepository)
        {
            _practiceTaskRepository = practiceTaskRepository;
            _progressRepository = progressRepository;
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetPracticeTask(int taskId, [FromQuery] int userId)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId != userId)
            {
                return Unauthorized(new { Message = "Invalid user ID." });
            }

            var task = await _practiceTaskRepository.GetById(taskId);
            if (task == null)
            {
                return NotFound(new { Message = "Practice task not found." });
            }

            var lesson = await _practiceTaskRepository.GetLesson(task.LessonId);
            var progress = await _progressRepository.GetProgressByUserAndCourse(userId, lesson.CourseId);
            if (progress == null || progress.TheoryProgress < 100)
            {
                return StatusCode(403, new { Message = "Practice is locked until theory is 100% complete." });
            }

            return Ok(new
            {
                task.Id,
                task.TaskDescription,
                task.StarterCode,
                task.TestCases,
                task.AverageTimeSeconds
            });
        }

        [HttpPost("{taskId}/submit")]
        public async Task<IActionResult> SubmitPracticeTask(int taskId, [FromBody] SubmitPracticeTaskDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId != request.UserId)
            {
                return Unauthorized(new { Message = "Invalid user ID." });
            }

            var task = await _practiceTaskRepository.GetById(taskId);
            if (task == null)
            {
                return NotFound(new { Message = "Practice task not found." });
            }

            var lesson = await _practiceTaskRepository.GetLesson(task.LessonId);
            var progress = await _progressRepository.GetProgressByUserAndCourse(request.UserId, lesson.CourseId);
            if (progress == null || progress.TheoryProgress < 100)
            {
                return StatusCode(403, new { Message = "Practice is locked until theory is 100% complete." });
            }

            var (success, output, error) = ExecuteCode(request.Code, task.TestCases);

            if (!success)
            {
                return BadRequest(new { Message = "Code execution failed.", Error = error, Output = output });
            }

            var newProgress = Math.Min(progress.PracticeProgress + 10, 100);
            await _progressRepository.UpdatePracticeProgress(request.UserId, lesson.CourseId, newProgress);
            await _progressRepository.SaveChanges();

            return Ok(new { Message = "Code executed successfully.", Output = output, PracticeProgress = newProgress });
        }

        private (bool Success, string Output, string Error) ExecuteCode(string userCode, string testCases)
        {
            try
            {
                var code = $@"
                using System;
                using System.Collections.Generic;
                using System.Linq;

                public class Program
                {{
                    public static void Main()
                    {{
                        {testCases}
                    }}

                    {userCode}
                }}";

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var assemblyName = Path.GetRandomFileName();
                var references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
                };

                var compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: [syntaxTree],
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

                using (var ms = new MemoryStream())
                {
                    var result = compilation.Emit(ms);

                    if (!result.Success)
                    {
                        var errors = string.Join("\n", result.Diagnostics
                            .Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error)
                            .Select(d => d.GetMessage()));
                        return (false, string.Empty, errors);
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    var assembly = Assembly.Load(ms.ToArray());

                    var type = assembly.GetType("Program");
                    var method = type.GetMethod("Main");

                    using (var sw = new StringWriter())
                    {
                        Console.SetOut(sw);
                        method.Invoke(null, null);
                        return (true, sw.ToString().Trim(), string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, string.Empty, ex.Message);
            }
        }
    }
}
