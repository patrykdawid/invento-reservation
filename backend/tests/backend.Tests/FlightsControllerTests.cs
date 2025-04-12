using System.Net;
using System.Net.Http.Json;
using backend.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using Xunit.Abstractions;

namespace backend.Tests;

public class FlightsControllerTests :
	IClassFixture<WebApplicationFactory<Program>>,
	IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly ITestOutputHelper _output;

	private readonly Stopwatch _stopwatch = new();

	public FlightsControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
	{
		_client = factory.CreateClient();

		_output = output;
	}

	public async Task InitializeAsync()
	{
		_stopwatch.Restart();

		await _client.PostAsync("/api/devdata/clear", null);
		await _client.PostAsync("/api/devdata/generate?flights=10&reservations=0", null);
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
		var response = await _client.GetAsync("/api/flights");
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetById_NotFound()
	{
		var response = await _client.GetAsync($"/api/flights/{Guid.NewGuid()}");
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetById_ShouldReturnFlight()
	{
		var flights = await _client.GetFromJsonAsync<FlightDto[]>("/api/flights");
		flights.Should().NotBeNull("Should return flight array");

		flights!.Length.Should().BeGreaterThan(0, "Should have generated at least 1 flight");

		var flight = flights.First();

		var response = await _client.GetAsync($"/api/flights/{flight.Id}");
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
