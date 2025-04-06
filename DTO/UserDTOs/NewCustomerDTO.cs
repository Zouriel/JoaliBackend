namespace JoaliBackend.DTO.UserDTOs
{
    public class NewCustomerDTO
    {
        
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm {  get; set; }
        


    }
    public enum UserType
    {
        Admin = 0,
        Staff = 1,
        Customer = 2,
    }
}
