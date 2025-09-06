using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Markdig;

namespace BlogApp.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PostController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    TempData["Error"] = "Please log in to create posts.";
                    return RedirectToAction("Login", "Account");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "User not found. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                if (!user.IsMaster)
                {
                    TempData["Error"] = "You don't have permission to create posts.";
                    return RedirectToAction("Index", "Home");
                }

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error accessing create page: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "User not found. Please log in again.";
                return RedirectToAction("Index", "Home");
            }

            if (!user.IsMaster)
            {
                TempData["Error"] = "You don't have permission to create posts.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var post = new Post
                    {
                        Title = model.Title,
                        Content = model.Content,
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        RenderedContent = Markdown.ToHtml(model.Content ?? string.Empty)
                    };

                    _context.Posts.Add(post);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Post created successfully!";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating post: {ex.Message}";
                    return View(model);
                }
            }
            else
            {
                // Add validation errors to TempData for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Validation errors: {string.Join(", ", errors)}";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, error = "User not found" });
            }

            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == id && pl.UserId == user.Id);

            if (existingLike == null)
            {
                _context.PostLikes.Add(new PostLike
                {
                    PostId = id,
                    UserId = user.Id
                });
            }
            else
            {
                _context.PostLikes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || !user.IsMaster)
                {
                    TempData["Error"] = "You don't have permission to delete posts.";
                    return RedirectToAction("Index", "Home");
                }

                var post = await _context.Posts
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.Likes)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.Replies)
                            .ThenInclude(r => r.Likes)
                    .Include(p => p.Likes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null)
                {
                    TempData["Error"] = "Post not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Remove all related data in proper order (due to foreign key constraints)
                // 1. Remove all comment likes (including reply likes)
                var allCommentLikes = post.Comments.SelectMany(c => c.Likes)
                    .Concat(post.Comments.SelectMany(c => c.Replies.SelectMany(r => r.Likes)));
                _context.CommentLikes.RemoveRange(allCommentLikes);

                // 2. Remove all replies
                var allReplies = post.Comments.SelectMany(c => c.Replies);
                _context.Comments.RemoveRange(allReplies);

                // 3. Remove all parent comments
                _context.Comments.RemoveRange(post.Comments);

                // 4. Remove post likes
                _context.PostLikes.RemoveRange(post.Likes);

                // 5. Finally remove the post
                _context.Posts.Remove(post);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Post deleted successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting post: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || !user.IsMaster)
                {
                    TempData["Error"] = "You don't have permission to edit posts.";
                    return RedirectToAction("Index", "Home");
                }

                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    TempData["Error"] = "Post not found.";
                    return RedirectToAction("Index", "Home");
                }

                var viewModel = new CreatePostViewModel
                {
                    Title = post.Title,
                    Content = post.Content
                };

                ViewData["PostId"] = id;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error accessing edit page: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreatePostViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || !user.IsMaster)
                {
                    TempData["Error"] = "You don't have permission to edit posts.";
                    return RedirectToAction("Index", "Home");
                }

                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    TempData["Error"] = "Post not found.";
                    return RedirectToAction("Index", "Home");
                }

                if (ModelState.IsValid)
                {
                    post.Title = model.Title;
                    post.Content = model.Content;
                    post.UpdatedAt = DateTime.UtcNow;
                    post.RenderedContent = Markdown.ToHtml(model.Content ?? string.Empty);

                    _context.Posts.Update(post);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Post updated successfully!";
                    return RedirectToAction("Post", "Home", new { id = post.Id });
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["Error"] = $"Validation errors: {string.Join(", ", errors)}";
                }

                ViewData["PostId"] = id;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating post: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Debug()
        {
            var user = await _userManager.GetUserAsync(User);
            var postCount = await _context.Posts.CountAsync();
            var userCount = await _context.Users.CountAsync();

            var debugInfo = new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                Username = User.Identity?.Name,
                CurrentUser = user != null ? new
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    IsMaster = user.IsMaster
                } : null,
                PostCount = postCount,
                UserCount = userCount,
                DatabaseExists = await _context.Database.CanConnectAsync()
            };

            return Json(debugInfo);
        }
    }
}
