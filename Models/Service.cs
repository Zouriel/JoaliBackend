namespace JoaliBackend.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrgId { get; set; }
        public ServiceType ServiceType { get; set; }
        public int Price { get; set; }
        public int? DurationInMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? Capacity { get; set; }
        public bool IsActive { get; set; }

    }
    public class ServiceType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ServiceOrder
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int UserId { get; set; }
        public int OrgId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ScheduledFor { get; set; }
        public OrderType OrderType { get; set; }
        public OrderStatus Status { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
        Completed = 3
    }
    public enum OrderType
    {
        Booking = 0,
        Purchase = 1
    }
}
