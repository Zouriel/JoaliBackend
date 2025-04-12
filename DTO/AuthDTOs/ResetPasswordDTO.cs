namespace JoaliBackend.DTO.AuthDTOs
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; }
        public string TemporaryKey { get; set; }
        public string NewPassword { get; set; }
    }
}
