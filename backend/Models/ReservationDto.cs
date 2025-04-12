namespace backend.Models
{
	public class ReservationDto
	{
		public Guid Id { get; set; }
		public string PassengerName { get; set; } = string.Empty;
		public Guid FlightId { get; set; }
		public TicketClass Class { get; set; }
	}
}
