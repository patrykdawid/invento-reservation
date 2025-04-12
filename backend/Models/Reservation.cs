using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
	public class Reservation
	{
		public Guid Id { get; set; }

		[Required]
		public string PassengerName { get; set; } = string.Empty;

		[Required]
		public Guid FlightId { get; set; }

		[Required]
		public Flight Flight { get; set; } = default!;

		[Required]
		public TicketClass Class { get; set; }
	}
}
