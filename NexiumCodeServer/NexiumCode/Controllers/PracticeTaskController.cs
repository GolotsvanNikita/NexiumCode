using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NexiumCode.DTO;
using NexiumCode.Models;
using NexiumCode.Repositories;
using NexiumCode.Services;
using System.Reflection;
using System.Text.Json;

namespace NexiumCode.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeTaskController : ControllerBase
    {
        private readonly IPracticeTaskRepository _practiceTaskRepository;
        private readonly IProgressRepository _progressRepository;
        private readonly IXPService _xpService;
        private readonly ILogger<PracticeTaskController> _logger;
        private readonly IWebHostEnvironment _env;

        public PracticeTaskController(
            IPracticeTaskRepository practiceTaskRepository,
            IProgressRepository progressRepository,
            IXPService xpService,
            ILogger<PracticeTaskController> logger,
            IWebHostEnvironment env)
        {
            _practiceTaskRepository = practiceTaskRepository;
            _progressRepository = progressRepository;
            _xpService = xpService;
            _logger = logger;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetPracticeTasks([FromQuery] int courseId, [FromQuery] int userId)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId != userId)
            {
                return Unauthorized(new { Message = "Invalid user ID." });
            }

            var progress = await _progressRepository.GetProgressByUserAndCourse(userId, courseId);
            if (progress == null || progress.TheoryProgress < 100)
            {
                return StatusCode(403, new { Message = "Practice is locked until theory is 100% complete." });
            }

            var tasks = await _practiceTaskRepository.GetTasksByCourse(courseId);
            if (tasks == null || !tasks.Any())
            {
                return NotFound(new { Message = "No practice tasks found for this course." });
            }

            return Ok(tasks.Select(t => new
            {
                t.Id,
                t.TaskDescription,
                t.StarterCode,
                t.TestCases,
                t.AverageTimeSeconds
            }));
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
            if (lesson == null)
            {
                return NotFound(new { Message = "Lesson not found." });
            }

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
            if (lesson == null)
            {
                return NotFound(new { Message = "Lesson not found." });
            }

            var progress = await _progressRepository.GetProgressByUserAndCourse(request.UserId, lesson.CourseId);
            if (progress == null || progress.TheoryProgress < 100)
            {
                return StatusCode(403, new { Message = "Practice is locked until theory is 100% complete." });
            }

            var (success, output, error) = ExecuteCode(request.Code, task.TestCases);

            if (!success)
            {
                return BadRequest(new { Message = "Code execution failed.", Error = error });
            }

            try
            {
                var totalTasks = (await _practiceTaskRepository.GetTasksByCourse(lesson.CourseId))?.Count() ?? 0;
                if (totalTasks == 0)
                {
                    return StatusCode(500, new { Message = "No practice tasks found for this course." });
                }

                var increment = 100 / totalTasks;
                var newProgress = Math.Min(progress.PracticeProgress + increment, 100);
                progress.PracticeProgress = newProgress;
                await _progressRepository.Update(progress);
                await _progressRepository.SaveChanges();

                if (_xpService != null)
                {
                    await _xpService.UpdateSkillProgress(request.UserId, "practice", 25);
                }

                return Ok(new { Message = "Code executed successfully.", Output = output, PracticeProgress = newProgress });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating progress for practice task");
                return StatusCode(500, new { Message = "Error updating progress.", Details = ex.Message });
            }
        }

        private (bool Success, string Output, string Error) ExecuteCode(string userCode, string testCases)
        {
            try
            {
                var (userSuccess, userOutput, userError) = CompileAndRun(userCode);
                if (!userSuccess)
                {
                    return (false, string.Empty, userError);
                }

                var (testSuccess, expectedOutput, testError) = CompileAndRun($@"
                    using System;

                    class TestProgram
                    {{
                        static void Main()
                        {{
                            {testCases}
                        }}
                    }}");
                if (!testSuccess)
                {
                    return (false, string.Empty, "Error in test cases: " + testError);
                }

                userOutput = userOutput.Trim().Replace("\r\n", "\n");
                expectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");

                if (userOutput != expectedOutput)
                {
                    return (false, userOutput, "Incorrect output. Expected: " + expectedOutput);
                }

                return (true, userOutput, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, string.Empty, "An error occurred: " + ex.Message);
            }
        }

        private (bool Success, string Output, string Error) CompileAndRun(string code)
        {
            try
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var assemblyName = Path.GetRandomFileName();

                var references = new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(decimal).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Private.CoreLib").Location)
                }.Distinct().ToArray();

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
                        return (false, string.Empty, "Compilation error: " + errors);
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    var assembly = Assembly.Load(ms.ToArray());

                    var programType = assembly.GetTypes().FirstOrDefault(t => t.Name == "Program" || t.Name == "TestProgram");
                    if (programType == null)
                    {
                        return (false, string.Empty, "No Program or TestProgram class found.");
                    }

                    var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (mainMethod == null)
                    {
                        return (false, string.Empty, "No Main method found.");
                    }

                    using (var sw = new StringWriter())
                    {
                        Console.SetOut(sw);
                        var parameters = mainMethod.GetParameters();
                        object[] args = parameters.Length == 0 ? null : new object[] { new string[0] };
                        mainMethod.Invoke(null, args);
                        var output = sw.ToString().Trim();
                        return (true, output, string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, string.Empty, "Execution error: " + ex.Message);
            }
        }
    }
}