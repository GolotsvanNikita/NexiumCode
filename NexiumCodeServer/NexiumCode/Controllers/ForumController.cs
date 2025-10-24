using Microsoft.AspNetCore.Mvc;
using NexiumCode.DTO;
using NexiumCode.Models;
using NexiumCode.Repositories;
using NexiumCode.Services;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumController : ControllerBase
    {
        private readonly IForumThreadRepository _thread;
        private readonly IForumReplyRepository _reply;
        private readonly IXPService _xpService;

        public ForumController(
            IForumThreadRepository thread,
            IForumReplyRepository reply,
            IXPService xpService)
        {
            _thread = thread;
            _reply = reply;
            _xpService = xpService;
        }

        [HttpGet("threads")]
        public async Task<IActionResult> GetThreads([FromQuery] string category = null, [FromQuery] string search = null)
        {
            var threads = await _thread.GetThreadsWithReplies();

            if (!string.IsNullOrEmpty(category))
            {
                threads = threads.Where(t => t.Category == category).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                threads = threads.Where(t =>
                    t.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.Content.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            var response = threads.Select(t => new ThreadDTO
            {
                Id = t.Id,
                UserId = t.UserId,
                Title = t.Title,
                Content = t.Content,
                Category = t.Category,
                IsResolved = t.IsResolved,
                CreatedAt = t.CreatedAt,
                Username = t.User?.Username,
                AvatarUrl = t.User?.AvatarUrl,
                ReplyCount = t.Replies?.Count(r => r.ParentReplyId == null) ?? 0
            });

            return Ok(response);
        }

        [HttpGet("threads/{threadId}")]
        public async Task<IActionResult> GetThread(int threadId)
        {
            var thread = await _thread.GetThreadWithReplies(threadId);
            if (thread == null || thread.IsDeleted)
            {
                return NotFound(new { Message = "Thread not found or deleted." });
            }

            var response = new ThreadDTO
            {
                Id = thread.Id,
                UserId = thread.UserId,
                Title = thread.Title,
                Content = thread.Content,
                Category = thread.Category,
                IsResolved = thread.IsResolved,
                CreatedAt = thread.CreatedAt,
                Username = thread.User?.Username,
                AvatarUrl = thread.User?.AvatarUrl,
                ReplyCount = thread.Replies?.Count ?? 0
            };

            response.Replies = BuildReplyTree(thread.Replies?.ToList());

            return Ok(response);
        }

        [HttpPost("threads")]
        public async Task<IActionResult> CreateThread([FromBody] CreateThreadDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var thread = new ForumThread
            {
                UserId = request.UserId,
                Title = request.Title,
                Content = request.Content,
                Category = request.Category,
                IsResolved = false,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _thread.Add(thread);
            await _thread.SaveChanges();

            await _xpService.AddXP(request.UserId, 5, "community", "Created forum thread");
            await _xpService.AddELO(request.UserId, 2, "Created forum thread");
            await _xpService.UpdateStreak(request.UserId);
            await _xpService.AddAchievement(request.UserId, "First Steps - Created first thread");

            return Ok(new { ThreadId = thread.Id, Message = "Thread created successfully." });
        }

        [HttpPost("threads/{threadId}/replies")]
        public async Task<IActionResult> CreateReply(int threadId, [FromBody] CreateReplyDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var thread = await _thread.GetById(threadId);
            if (thread == null || thread.IsDeleted)
            {
                return NotFound(new { Message = "Thread not found or deleted." });
            }

            if (request.ParentReplyId.HasValue)
            {
                var parentReply = await _reply.GetById(request.ParentReplyId.Value);
                if (parentReply == null || parentReply.ThreadId != threadId)
                {
                    return BadRequest(new { Message = "Invalid parent reply." });
                }
            }

            var reply = new ForumReply
            {
                ThreadId = threadId,
                UserId = request.UserId,
                Content = request.Content,
                ParentReplyId = request.ParentReplyId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _reply.Add(reply);
            await _reply.SaveChanges();

            await _xpService.AddXP(request.UserId, 10, "community", "Posted forum reply");
            await _xpService.AddELO(request.UserId, 3, "Posted forum reply");
            await _xpService.UpdateStreak(request.UserId);

            return Ok(new { ReplyId = reply.Id, Message = "Reply added successfully." });
        }

        [HttpDelete("threads/{threadId}")]
        public async Task<IActionResult> SoftDeleteThread(int threadId, [FromQuery] int userId)
        {
            var thread = await _thread.GetById(threadId);
            if (thread == null || thread.IsDeleted)
            {
                return NotFound(new { Message = "Thread not found or already deleted." });
            }

            if (thread.UserId != userId)
            {
                return StatusCode(403, new { Message = "You are not authorized to delete this thread." });
            }

            await _thread.SoftDelete(threadId);
            await _thread.SaveChanges();

            return Ok(new { Message = "Thread deleted successfully." });
        }

        [HttpPut("threads/{threadId}/resolve")]
        public async Task<IActionResult> MarkAsResolved(int threadId, [FromQuery] int userId)
        {
            var thread = await _thread.GetById(threadId);
            if (thread == null || thread.IsDeleted)
            {
                return NotFound(new { Message = "Thread not found or deleted." });
            }

            if (thread.UserId != userId)
            {
                return StatusCode(403, new { Message = "You are not authorized to resolve this thread." });
            }

            await _thread.MarkAsResolved(threadId);
            await _thread.SaveChanges();

            await _xpService.AddXP(userId, 25, "community", "Thread marked as solved");
            await _xpService.AddELO(userId, 10, "Thread marked as solved");

            return Ok(new { Message = "Thread marked as resolved." });
        }

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var categories = new List<string>
            {
                "JavaScript",
                "C#",
                "HTML/CSS",
                "Python"
            };

            return Ok(categories);
        }

        private List<ReplyDTO> BuildReplyTree(List<ForumReply> allReplies)
        {
            if (allReplies == null || !allReplies.Any())
            {
                return [];
            }

            var rootReplies = allReplies
                .Where(r => r.ParentReplyId == null)
                .OrderBy(r => r.CreatedAt)
                .Select(r => MapToReplyDTO(r, allReplies))
                .ToList();

            return rootReplies;
        }

        private ReplyDTO MapToReplyDTO(ForumReply reply, List<ForumReply> allReplies)
        {
            var dto = new ReplyDTO
            {
                Id = reply.Id,
                ThreadId = reply.ThreadId,
                UserId = reply.UserId,
                Content = reply.Content,
                ParentReplyId = reply.ParentReplyId,
                CreatedAt = reply.CreatedAt,
                Username = reply.User?.Username,
                AvatarUrl = reply.User?.AvatarUrl,
                ChildReplies = new List<ReplyDTO>()
            };

            var children = allReplies
                .Where(r => r.ParentReplyId == reply.Id)
                .OrderBy(r => r.CreatedAt)
                .Select(r => MapToReplyDTO(r, allReplies))
                .ToList();

            dto.ChildReplies = children;

            return dto;
        }
    }
}