using System.Net;
using System.Net.Http.Json;
using backend.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using Xunit.Abstractions;

namespace backend.Tests;

public class ReservationsControllerTests :
	IClassFixture<WebApplicationFactory<Program>>,
	IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly ITestOutputHelper _output;

	private readonly Stopwatch _stopwatch = new();

	public ReservationsControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
	{
		_client = factory.CreateClient();

		_output = output;
	}

	public async Task InitializeAsync()
	{
		_stopwatch.Restart();

		await _client.PostAsync("/api/devdata/clear", null);
		await _client.PostAsync("/api/devdata/generate?flights=3&reservations=5", null);
	}

	public Task DisposeAsync()
	{
		_stopwatch.Stop();
		_output.WriteLine($"[TEST DURATION] {GetType().Name} - {DateTime.Now:HH:mm:ss.fff} - {_stopwatch.ElapsedMilliseconds} ms");

		return _client.PostAsync("/api/devdata/clear", null);
	}

	[Fact]
	public async Task GetAll_ShouldReturnOk()
	{
		var response = await _client.GetAsync("/api/reservations");
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task Post_InvalidEnum_ShouldReturnBadRequest()
	{
		var invalidDto = new
		{
			passengerName = "Jan",
			flightId = Guid.NewGuid(),
			@class = -1
		};

		var response = await _client.PostAsJsonAsync("/api/reservations", invalidDto);
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Post_InvalidFlight_ShouldReturnBadRequest()
	{
		var dto = new ReservationDto
		{
			PassengerName = "Test",
			Class = TicketClass.Economy,
			FlightId = Guid.NewGuid()
		};

		var response = await _client.PostAsJsonAsync("/api/reservations", dto);
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Post_Then_GetById_Then_Delete_ShouldSucceed()
	{
		await _client.PostAsync("/api/devdata/clear", null);
		await _client.PostAsync("/api/devdata/generate?flights=1&reservations=0", null);

		var flights = await _client.GetFromJsonAsync<FlightDto[]>("/api/flights");
		flights.Should().NotBeNull("Should return flight array");

		flights!.Length.Should().BeGreaterThan(0, "Should have generated at least 1 flight");

		var flight = flights.First();

		var dto = new ReservationDto
		{
			PassengerName = "Test",
			Class = TicketClass.Business,
			FlightId = flight.Id
		};

		var postResponse = await _client.PostAsJsonAsync("/api/reservations", dto);
		postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

		var created = await postResponse.Content.ReadFromJsonAsync<ReservationDto>();

		var getResponse = await _client.GetAsync($"/api/reservations/{created!.Id}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var deleteResponse = await _client.DeleteAsync($"/api/reservations/{created.Id}");
		deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task Put_InvalidFlight_ShouldReturnBadRequest()
	{
		await _client.PostAsync("/api/devdata/clear", null);
		await _client.PostAsync("/api/devdata/generate?flights=1&reservations=1", null);

		var reservations = await _client.GetFromJsonAsync<ReservationDto[]>("/api/reservations");
		reservations.Should().NotBeNull("Should return reservation array");

		reservations!.Length.Should().BeGreaterThan(0, "Should have generated at least 1 reservation");

		var existing = reservations.First();

		var update = new ReservationDto
		{
			Id = existing.Id,
			PassengerName = "Changed",
			Class = existing.Class,
			FlightId = Guid.NewGuid() // nieistniejący
		};

		var result = await _client.PutAsJsonAsync($"/api/reservations/{existing.Id}", update);
		result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Put_NotFound_ShouldReturn404()
	{
		var update = new ReservationDto
		{
			Id = Guid.NewGuid(),
			PassengerName = "Changed",
			Class = TicketClass.Economy,
			FlightId = Guid.NewGuid()
		};

		var result = await _client.PutAsJsonAsync($"/api/reservations/{update.Id}", update);
		result.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Delete_NotFound_ShouldReturn404()
	{
		var result = await _client.DeleteAsync($"/api/reservations/{Guid.NewGuid()}");
		result.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetById_NotFound_ShouldReturn404()
	{
		var result = await _client.GetAsync($"/api/reservations/{Guid.NewGuid()}");
		result.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Put_ValidUpdate_ShouldReturnOk()
	{
		await _client.PostAsync("/api/devdata/clear", null);
		await _client.PostAsync("/api/devdata/generate?flights=1&reservations=1", null);

		var reservations = await _client.GetFromJsonAsync<ReservationDto[]>("/api/reservations");
		reservations.Should().NotBeNull();
		reservations!.Should().NotBeEmpty();

		var reservation = reservations.First();

		var update = new ReservationDto
		{
			Id = reservation.Id,
			PassengerName = "Updated Name",
			Class = reservation.Class,
			FlightId = reservation.FlightId
		};

		var response = await _client.PutAsJsonAsync($"/api/reservations/{reservation.Id}", update);
		response.StatusCode.Should().Be(HttpStatusCode.NoContent);

		var updated = await _client.GetFromJsonAsync<ReservationDto>($"/api/reservations/{reservation.Id}");
		updated!.PassengerName.Should().Be("Updated Name");
	}

	[Fact]
	public async Task Post_WithUnknownFlight_ShouldReturnBadRequest()
	{
		var dto = new ReservationDto
		{
			FlightId = Guid.NewGuid(), // nieistniejący
			PassengerName = "Nowy Pasażer",
		};

		var response = await _client.PostAsJsonAsync("/api/reservations", dto);
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Put_WithUnknownFlight_ShouldReturnBadRequest()
	{
		var reservationId = Guid.NewGuid();
		var dto = new ReservationDto
		{
			FlightId = Guid.NewGuid(), // nieistniejący
			PassengerName = "Zmieniony Pasażer",
		};

		var response = await _client.PutAsJsonAsync($"/api/reservations/{reservationId}", dto);
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	//[Fact]
	//public async Task Post_WithMissingPassengerName_ShouldReturnBadRequest()
	//{
	//	var flights = await _client.GetFromJsonAsync<FlightDto[]>("/api/flights");
	//	flights.Should().NotBeNullOrEmpty();
	//	var validFlight = flights.First();

	//	var dto = new ReservationDto
	//	{
	//		FlightId = validFlight.Id,
	//		PassengerName = "", // niepoprawny
	//	};

	//	var response = await _client.PostAsJsonAsync("/api/reservations", dto);
	//	response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	//}
}
