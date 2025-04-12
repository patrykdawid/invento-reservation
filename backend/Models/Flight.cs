using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
	public class Flight
	{
		public Guid Id { get; set; }

		[Required]
		public string Number { get; set; } = string.Empty;

		[Required]
		public DateTimeOffset DepartureTime { get; set; }

		[Required]
		public DateTimeOffset ArrivalTime { get; set; }
	}
}
