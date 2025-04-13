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

	private readonly Faker<Flight> _flightFaker;
	private readonly Faker<Reservation> _reservationFaker;

	public DevDataController(IFlightRepository flights, IReservationRepository reservations)
	{
		_flights = flights;
		_reservations = reservations;

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
		_flightFaker = new Faker<Flight>()
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

		_reservationFaker = new Faker<Reservation>()
			.RuleFor(r => r.Id, f => Guid.NewGuid())
			.RuleFor(r => r.PassengerName, f => f.Name.FullName())
			.RuleFor(r => r.Class, f => f.PickRandom<TicketClass>())
			.Ignore(r => r.Flight)
			.Ignore(r => r.FlightId);
	}

	[HttpPost("generate")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public IActionResult Generate([FromQuery] int flights = 150, [FromQuery] int reservations = 10000)
	{
		var flightsList = _flightFaker.Generate(flights);
		flightsList.ForEach(_flights.Add);
		_flights.Save();

		var reservationsList = GenerateReservations(flightsList, reservations);
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

	private List<Reservation> GenerateReservations(List<Flight> flights, int reservationCount)
	{
		var reservations = new List<Reservation>();
		if (flights.Count == 0 || reservationCount == 0)
			return reservations;

		var flightsList = flights.ToList();

		int noResCount = (int)(flights.Count * 0.05);
		int singleResCount = (int)(flights.Count * 0.2);
		var noReservationFlights = flightsList.Take(noResCount).ToList();
		var singleReservationFlights = flightsList.Skip(noResCount).Take(singleResCount).ToList();
		var multiReservationFlights = flightsList.Skip(noResCount + singleReservationFlights.Count).ToList();

		var remainingReservations = reservationCount;

		var existingNames = new List<string>();

		// Jedna rezerwacja na lot
		foreach (var flight in singleReservationFlights)
		{
			if (remainingReservations == 0) break;

			var reservation = _reservationFaker.Generate();
			reservation.PassengerName = MaybeRepeatName(reservation.PassengerName, existingNames);
			reservation.FlightId = flight.Id;
			reservation.Flight = flight;

			reservations.Add(reservation);
			remainingReservations--;
		}

		// Wiele rezerwacji
		foreach (var flight in multiReservationFlights)
		{
			if (remainingReservations == 0) break;

			var upperBound = Math.Max(3, multiReservationFlights.Count);
			upperBound = Math.Min(upperBound, 200);
			var howMany = Random.Shared.Next(2, upperBound); // od 2 do n-1 rezerwacji, gdzie n to liczba lotów, ale max 200
			howMany = Math.Min(howMany, remainingReservations);
			for (int i = 0; i < howMany && remainingReservations > 0; i++)
			{
				var reservation = _reservationFaker.Generate();
				reservation.PassengerName = MaybeRepeatName(reservation.PassengerName, existingNames);
				reservation.FlightId = flight.Id;
				reservation.Flight = flight;

				reservations.Add(reservation);
				remainingReservations--;
			}
		}

		// Jeśli zostały rezerwacje do rozdania – losowo dopasuj (do drugiej ich połowy, pierwsza zostaje z 'małymi' liczbami rezerwacji)
		while (remainingReservations > 0)
		{
			var halfMultiResCount = multiReservationFlights.Count / 2;
			var flight = multiReservationFlights
				.Skip(halfMultiResCount)
				.Take(halfMultiResCount)
				.ToList()
				[Random.Shared.Next(halfMultiResCount)];

			var reservation = _reservationFaker.Generate();
			reservation.PassengerName = MaybeRepeatName(reservation.PassengerName, existingNames);
			reservation.FlightId = flight.Id;
			reservation.Flight = flight;

			reservations.Add(reservation);
			remainingReservations--;
		}

		return reservations;
	}

	private static string MaybeRepeatName(string generatedName, List<string> existingNames)
	{
		if (existingNames.Count > 0 && Random.Shared.NextDouble() < 0.10)
		{
			return existingNames[Random.Shared.Next(existingNames.Count)];
		}
		else
		{
			existingNames.Add(generatedName);
			return generatedName;
		}
	}
}
#endif
