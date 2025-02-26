namespace JoaliBackend.models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password_hash { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } 
        public bool IsACtive { get; set; }

    }
}
