using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CommentController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int postId, string content, int? parentCommentId = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Comment content is required");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var comment = new Comment
            {
                Content = content,
                PostId = postId,
                UserId = user.Id,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                comment = new
                {
                    id = comment.Id,
                    content = comment.Content,
                    username = user.DisplayName,
                    createdAt = comment.CreatedAt.ToString("MMM dd, yyyy HH:mm")
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, error = "User not found" });
            }

            var existingLike = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == id && cl.UserId == user.Id);

            if (existingLike == null)
            {
                _context.CommentLikes.Add(new CommentLike
                {
                    CommentId = id,
                    UserId = user.Id
                });
            }
            else
            {
                _context.CommentLikes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
