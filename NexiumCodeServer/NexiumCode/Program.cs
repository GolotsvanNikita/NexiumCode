using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.JSON;
using NexiumCode.Models;
using NexiumCode.Repositories;
using NexiumCode.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers();

string? connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IPasswordHash, PasswordHash>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<IPracticeTaskRepository, PracticeTaskRepository>();
builder.Services.AddScoped<IForumThreadRepository, ForumThreadRepository>();
builder.Services.AddScoped<IForumReplyRepository, ForumReplyRepository>();
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
builder.Services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
builder.Services.AddScoped<IXPService, XPService>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Starting database seeding...");
        await SeedData.Initialize(context);
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database seeding: {Message}", ex.Message);
        throw;
    }
}

app.UseCors("AllowReact");
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:5173");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
    }
});
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();

app.Run();

public static class SeedData
{
    public static async Task Initialize(AppDbContext context)
    {
        if (!context.Courses.Any())
        {
            context.Courses.Add(new Course
            {
                Name = "C# Basics",
                Description = "Learn the fundamentals of C# programming, including syntax, variables, control structures, and object-oriented programming through theory and practical tasks.",
                CreatedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        if (!context.Lessons.Any())
        {
            var lessons = new[]
            {
                new Lesson { CourseId = 1, Title = "Introduction to C#", Content = "C# is a modern...", IsTheory = true, Order = 1 },
                new Lesson { CourseId = 1, Title = "Variables and Data Types", Content = "In C#, variables...", IsTheory = true, Order = 2 },
                new Lesson { CourseId = 1, Title = "Operators and Expressions", Content = "Operators perform...", IsTheory = true, Order = 3 },
                new Lesson { CourseId = 1, Title = "Control Structures: If-Else", Content = "Control structures...", IsTheory = true, Order = 4 },
                new Lesson { CourseId = 1, Title = "Loops: For and While", Content = "Loops repeat...", IsTheory = true, Order = 5 },
                new Lesson { CourseId = 1, Title = "Arrays", Content = "Arrays store...", IsTheory = true, Order = 6 },
                new Lesson { CourseId = 1, Title = "Methods (Functions)", Content = "Methods help organize code...", IsTheory = true, Order = 7 },
                new Lesson { CourseId = 1, Title = "Introduction to Classes and Objects", Content = "OOP introduces...", IsTheory = true, Order = 8 },
                new Lesson { CourseId = 1, Title = "Course Summary and Next Steps", Content = "Congratulations!", IsTheory = true, Order = 9 }
            };

            context.Lessons.AddRange(lessons);
            await context.SaveChangesAsync();
        }


        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CSharpCourse.json");
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"JSON file not found: {jsonPath}");
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var courseJson = JsonSerializer.Deserialize<CourseJson>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (courseJson != null && courseJson.Lessons != null)
        {
            foreach (var lessonJson in courseJson.Lessons)
            {
                var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.Title == lessonJson.Title && l.Order == lessonJson.Order);
                if (lesson != null)
                {
                    foreach (var taskJson in lessonJson.PracticeTasks ?? new List<PracticeTaskJson>())
                    {
                        if (!context.PracticeTasks.Any(t => t.TaskDescription == taskJson.TaskDescription && t.LessonId == lesson.Id))
                        {
                            context.PracticeTasks.Add(new PracticeTask
                            {
                                LessonId = lesson.Id,
                                TaskDescription = taskJson.TaskDescription,
                                StarterCode = taskJson.StarterCode,
                                TestCases = taskJson.TestCases,
                                AverageTimeSeconds = taskJson.AverageTimeSeconds
                            });
                        }
                    }
                }
            }
            await context.SaveChangesAsync();
        }
    }
}