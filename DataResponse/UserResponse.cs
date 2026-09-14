namespace PizzaManagement.DataResponse
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string ? UserName { get; set;}
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public string? FullName { get; set; }
        public string ? Role { get; set; }
        public bool isLocked { get; set; } = false;
    }
}
