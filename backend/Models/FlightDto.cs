namespace backend.Models;
public class FlightDto
{
	public Guid Id { get; set; }
	public string Number { get; set; } = string.Empty;
	public DateTimeOffset DepartureTime { get; set; }
	public DateTimeOffset ArrivalTime { get; set; }
	public IList<ReservationDto>? Reservations { get; set; }
}
