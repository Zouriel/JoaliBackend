namespace JoaliBackend.DTO.ServiceDTOs
{
    public class PlaceServiceOrderDTO
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime? ScheduledFor { get; set; }
    }
}
