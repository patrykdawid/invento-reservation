using backend.Database;
using backend.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using Xunit.Abstractions;

namespace backend.Tests;

public class JsonReservationRepositoryTests :
	IClassFixture<WebApplicationFactory<Program>>,
	IAsyncLifetime
{
	private readonly ITestOutputHelper _output;

	private readonly Stopwatch _stopwatch = new();

	private readonly IFlightRepository _flights;
	private readonly IReservationRepository _reservations;

	public JsonReservationRepositoryTests(ITestOutputHelper output)
	{
		_output = output;

		var mapper = TestHelper.CreateMapper();
		_flights = new JsonFlightRepository(mapper, NullLogger<JsonFlightRepository>.Instance);
		_reservations = new JsonReservationRepository(mapper, _flights, NullLogger<JsonReservationRepository>.Instance);

		_reservations.GetAll().ToList().ForEach(_reservations.Remove);
		_reservations.Save();

		_flights.GetAll().ToList().ForEach(_flights.Remove);
		_flights.Save();
	}

	public async Task InitializeAsync()
	{
		_stopwatch.Restart();

		_reservations.Delete();
		_flights.Delete();

		await Task.CompletedTask;
	}

	public Task DisposeAsync()
	{
		_stopwatch.Stop();
		_output.WriteLine($"[TEST DURATION] {GetType().Name} - {DateTime.Now:HH:mm:ss.fff} - {_stopwatch.ElapsedMilliseconds} ms");

		_reservations.Delete();
		_flights.Delete();

		return Task.CompletedTask;
	}

	[Fact]
	public void Add_And_Find_Reservation()
	{
		var flight = new Flight { Number = "LO300", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(2) };
		_flights.Add(flight);
		_flights.Save();

		var reservation = new Reservation
		{
			PassengerName = "Test",
			Class = TicketClass.Economy,
			Flight = flight,
			FlightId = flight.Id
		};

		_reservations.Add(reservation);
		_reservations.Save();

		var found = _reservations.Find(reservation.Id);
		Assert.NotNull(found);
		Assert.Equal("Test", found!.PassengerName);
	}

	[Fact]
	public void Update_Reservation()
	{
		var flight = new Flight { Number = "LO400", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(3) };
		_flights.Add(flight);
		_flights.Save();

		var reservation = new Reservation
		{
			PassengerName = "Initial",
			Class = TicketClass.Business,
			Flight = flight,
			FlightId = flight.Id
		};

		_reservations.Add(reservation);
		_reservations.Save();

		reservation.PassengerName = "Updated";
		_reservations.Update(reservation);
		_reservations.Save();

		var updated = _reservations.Find(reservation.Id);
		Assert.Equal("Updated", updated!.PassengerName);
	}

	[Fact]
	public void Remove_Reservation()
	{
		var flight = new Flight { Number = "LO500", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(4) };
		_flights.Add(flight);
		_flights.Save();

		var reservation = new Reservation
		{
			PassengerName = "ToRemove",
			Class = TicketClass.Economy,
			Flight = flight,
			FlightId = flight.Id
		};

		_reservations.Add(reservation);
		_reservations.Save();

		_reservations.Remove(reservation);
		_reservations.Save();

		var found = _reservations.Find(reservation.Id);
		Assert.Null(found);
	}

	[Fact]
	public void Update_WithSameFlightId_DoesNotChangeFlight()
	{
		var flight = new Flight { Number = "LO101", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(2) };
		_flights.Add(flight);
		_flights.Save();

		var reservation = new Reservation
		{
			PassengerName = "SameFlightUser",
			Class = TicketClass.Economy,
			Flight = flight,
			FlightId = flight.Id
		};
		_reservations.Add(reservation);
		_reservations.Save();

		// Tworzymy kopię z tą samą wartością FlightId
		var updated = new Reservation
		{
			Id = reservation.Id,
			PassengerName = "SameFlightUser Updated",
			Class = TicketClass.Business,
			Flight = flight, // ten sam obiekt Flight
			FlightId = flight.Id
		};

		_reservations.Update(updated);
		_reservations.Save();

		var final = _reservations.Find(reservation.Id);
		Assert.Equal("SameFlightUser Updated", final!.PassengerName);
		Assert.Equal(TicketClass.Business, final.Class);
		Assert.Equal(flight.Id, final.Flight!.Id); // Flight nie został nadpisany z repo
	}

	[Fact]
	public void Update_WithDifferentFlightId_ChangesFlight()
	{
		var originalFlight = new Flight { Number = "LO111", DepartureTime = DateTimeOffset.UtcNow };
		var newFlight = new Flight { Number = "LO222", DepartureTime = DateTimeOffset.UtcNow.AddHours(1) };

		_flights.Add(originalFlight);
		_flights.Add(newFlight);
		_flights.Save();

		var reservation = new Reservation
		{
			PassengerName = "ChangeFlightUser",
			Class = TicketClass.Economy,
			Flight = originalFlight,
			FlightId = originalFlight.Id
		};
		_reservations.Add(reservation);
		_reservations.Save();

		// Aktualizacja z innym FlightId
		var updated = new Reservation
		{
			Id = reservation.Id,
			PassengerName = "ChangeFlightUser",
			Class = TicketClass.Economy,
			Flight = newFlight,
			FlightId = newFlight.Id
		};

		_reservations.Update(updated);
		_reservations.Save();

		var result = _reservations.Find(reservation.Id);
		Assert.Equal(newFlight.Id, result!.Flight!.Id);
	}

	[Fact]
	public void RemoveById_ShouldRemoveReservation_WhenReservationExists()
	{
		var flight = new Flight { Number = "LO999", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(2) };
		_flights.Add(flight);
		_flights.Save();

		var reservation = new Reservation
		{
			PassengerName = "RemoveTest",
			Class = TicketClass.Business,
			Flight = flight,
			FlightId = flight.Id
		};

		_reservations.Add(reservation);
		_reservations.Save();

		_reservations.RemoveById(reservation.Id);
		_reservations.Save();

		var result = _reservations.Find(reservation.Id);
		Assert.Null(result);
	}

	[Fact]
	public void RemoveById_ShouldThrow_WhenReservationDoesNotExist()
	{
		var nonExistingId = Guid.NewGuid();

		var ex = Assert.Throws<InvalidOperationException>(() =>
		{
			_reservations.RemoveById(nonExistingId);
		});

		Assert.Equal("Reservation not found", ex.Message);
	}
}
