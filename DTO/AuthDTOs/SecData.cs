namespace JoaliBackend.DTO.AuthDTOs
{
    public class SecData
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
