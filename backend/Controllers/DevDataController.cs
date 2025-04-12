using backend.Database;
using backend.Models;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace backend.Controllers;

[ExcludeFromCodeCoverage]
[ApiController]
[Route("api/[controller]")]
#if DEBUG
public class DevDataController : ControllerBase
{
	private readonly IFlightRepository _flights;
	private readonly IReservationRepository _reservations;

	public DevDataController(IFlightRepository flights, IReservationRepository reservations)
	{
		_flights = flights;
		_reservations = reservations;
	}

	[HttpPost("generate")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public IActionResult Generate([FromQuery] int flights = 500, [FromQuery] int reservations = 10000)
	{
		var timeZones = new[]
		{
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? "Central European Standard Time"
				: "Europe/Warsaw",
			"UTC",
			"Pacific Standard Time",
			"Tokyo Standard Time",
			"E. Australia Standard Time"
		};

		var flightFaker = new Faker<Flight>()
			.RuleFor(f => f.Id, f => Guid.NewGuid())
			.RuleFor(f => f.Number, f => $"LO{f.Random.Int(100, 999)}")
			.RuleFor(f => f.DepartureTime, f =>
			{
				var tzId = f.PickRandom(timeZones);
				var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
				var utcBase = DateTime.UtcNow.AddDays(f.Random.Int(1, 30)).AddHours(f.Random.Int(0, 23));
				var local = TimeZoneInfo.ConvertTimeFromUtc(utcBase, tz);
				var offset = tz.GetUtcOffset(local);
				return new DateTimeOffset(local, offset);
			})
			.RuleFor(f => f.ArrivalTime, (f, flight) =>
			{
				var hours = f.Random.Int(1, 12);
				return flight.DepartureTime.AddHours(hours);
			});

		var flightsList = flightFaker.Generate(flights);
		flightsList.ForEach(_flights.Add);
		_flights.Save();

		var flightPool = flightsList.ToList();

		var reservationFaker = new Faker<Reservation>()
			.RuleFor(r => r.Id, f => Guid.NewGuid())
			.RuleFor(r => r.PassengerName, f => f.Name.FullName())
			.RuleFor(r => r.Class, f => f.PickRandom<TicketClass>())
			.RuleFor(r => r.Flight, f => f.PickRandom(flightPool))
			.RuleFor(r => r.FlightId, (f, r) => r.Flight.Id);

		var reservationsList = reservationFaker.Generate(reservations);
		reservationsList.ForEach(_reservations.Add);
		_reservations.Save();

		return NoContent();
	}

	[HttpPost("clear")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public IActionResult Clear()
	{
		_flights.Delete();
		_reservations.Delete();

		return NoContent();
	}
}
#endif
