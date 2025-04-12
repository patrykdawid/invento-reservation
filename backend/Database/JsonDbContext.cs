using System.Diagnostics.CodeAnalysis;

namespace backend.Database;

[ExcludeFromCodeCoverage]
public class JsonDbContext
{
	public IFlightRepository Flights { get; }
	public IReservationRepository Reservations { get; }

	public JsonDbContext(IFlightRepository flightRepo, IReservationRepository reservationRepo)
	{
		Flights = flightRepo;
		Reservations = reservationRepo;
	}
}
