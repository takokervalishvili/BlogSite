using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class CreatePostViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
