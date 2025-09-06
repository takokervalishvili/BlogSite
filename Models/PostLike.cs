namespace BlogApp.Models
{
    public class PostLike
    {
        public int Id { get; set; }
        
        public string UserId { get; set; }
        public virtual User User { get; set; }
        
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
