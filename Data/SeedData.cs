using BlogApp.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if master user exists
            var masterUser = await userManager.FindByEmailAsync("admin@blog.com");
            if (masterUser == null)
            {
                masterUser = new User
                {
                    UserName = "admin@blog.com",
                    Email = "admin@blog.com",
                    DisplayName = "Blog Admin",
                    IsMaster = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(masterUser, "Admin123!");
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create master user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Create sample post if no posts exist
            if (!context.Posts.Any())
            {
                var samplePost = new Post
                {
                    Title = "Welcome to My Personal Blog!",
                    Content = @"# Welcome!

This is my first blog post written in **Markdown**. 

## Features

This blog supports:
- Markdown rendering
- User comments and replies
- Like functionality
- Responsive design

### Code Example

```csharp
public class BlogPost 
{
    public string Title { get; set; }
    public string Content { get; set; }
}
```

> Feel free to explore and interact with the content!

Happy reading! 🎉",
                    UserId = masterUser.Id,
                    CreatedAt = DateTime.UtcNow
                };

                // Render markdown
                samplePost.RenderedContent = Markdig.Markdown.ToHtml(samplePost.Content);

                context.Posts.Add(samplePost);
                await context.SaveChangesAsync();
            }
        }
    }
}
