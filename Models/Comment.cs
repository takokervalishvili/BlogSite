using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        
        [Required]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
        
        public int? ParentCommentId { get; set; }
        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        
        public virtual ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    }
}