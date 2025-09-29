using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexiumCode.DTO;
using NexiumCode.Models;
using NexiumCode.Repositories;

namespace NexiumCode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumController : ControllerBase
    {
        private readonly IForumThreadRepository _thread;
        private readonly IForumReplyRepository _reply;

        public ForumController(IForumThreadRepository thread, IForumReplyRepository reply)
        {
            _thread = thread;
            _reply = reply;
        }

        [HttpGet("threads")]
        public async Task<IActionResult> GetThreads()
        {
            var threads = await _thread.GetThreadsWithReplies();
            var response = threads.Select(t => new ThreadDTO
            {
                Id = t.Id,
                UserId = t.UserId,
                Title = t.Title,
                Content = t.Content,
                IsResolved = t.IsResolved,
                CreatedAt = t.CreatedAt,
                Username = t.User?.Username,
                ReplyCount = t.Replies?.Count ?? 0
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
                IsResolved = thread.IsResolved,
                CreatedAt = thread.CreatedAt,
                Username = thread.User?.Username,
                ReplyCount = thread.Replies?.Count ?? 0
            };

            response.Replies = thread.Replies?.Select(r => new ReplyDTO
            {
                Id = r.Id,
                ThreadId = r.ThreadId,
                UserId = r.UserId,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                Username = r.User?.Username
            }).ToList();

            return Ok(response);
        }

        [HttpPost("threads")]
        public async Task<IActionResult> CreateThread([FromBody] CreateThreadDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var thread = new ForumThread
            {
                UserId = request.UserId,
                Title = request.Title,
                Content = request.Content,
                IsResolved = false,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _thread.Add(thread);
            await _thread.SaveChanges();

            return Ok(new { ThreadId = thread.Id });
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

            var reply = new ForumReply
            {
                ThreadId = threadId,
                UserId = request.UserId,
                Content = request.Content,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _reply.Add(reply);
            await _reply.SaveChanges();

            return Ok(new { ReplyId = reply.Id });
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

            return Ok(new { Message = "Thread marked as resolved." });
        }
    }
}
