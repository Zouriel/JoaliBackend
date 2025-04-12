namespace JoaliBackend.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string token { get; set; }
        public int userId { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; }
        public string IPAddress { get; set; }
        public string? Device { get; set; }
        public string? Os { get; set; }
        public string? Client { get; set; }
        public bool isActive { get; set; }

    }
}
