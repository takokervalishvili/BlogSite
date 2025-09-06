using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Markdig;

namespace BlogApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, UserManager<User> userManager, ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"Found {posts.Count} posts in database");

                // Debug: Check current user
                var currentUser = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Current user: {currentUser?.Email}, IsMaster: {currentUser?.IsMaster}");

                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts");
                TempData["Error"] = $"Error loading posts: {ex.Message}";
                return View(new List<Post>());
            }
        }

        public async Task<IActionResult> Post(int id)
        {
            try
            {
                _logger.LogInformation($"Attempting to retrieve post with ID: {id}");

                var post = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.Likes)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.Replies)
                            .ThenInclude(r => r.User)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.Replies)
                            .ThenInclude(r => r.Likes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null)
                {
                    _logger.LogWarning($"Post with ID {id} not found");
                    TempData["Error"] = $"Post with ID {id} not found";
                    return RedirectToAction("Index");
                }

                _logger.LogInformation($"Successfully retrieved post: {post.Title}");
                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving post with ID {id}");
                TempData["Error"] = $"Error loading post: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Debug()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var postCount = await _context.Posts.CountAsync();
            var userCount = await _context.Users.CountAsync();

            var debugInfo = new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                Username = User.Identity?.Name,
                CurrentUser = currentUser != null ? new
                {
                    Id = currentUser.Id,
                    Email = currentUser.Email,
                    DisplayName = currentUser.DisplayName,
                    IsMaster = currentUser.IsMaster
                } : null,
                PostCount = postCount,
                UserCount = userCount,
                DatabaseExists = await _context.Database.CanConnectAsync()
            };

            return Json(debugInfo);
        }
    }
}
