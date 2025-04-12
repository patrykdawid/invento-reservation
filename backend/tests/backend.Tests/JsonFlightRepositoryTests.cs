using backend.Database;
using backend.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using Xunit.Abstractions;

namespace backend.Tests;

public class JsonFlightRepositoryTests :
	IClassFixture<WebApplicationFactory<Program>>,
	IAsyncLifetime
{
	private readonly ITestOutputHelper _output;

	private readonly Stopwatch _stopwatch = new();

	private readonly IFlightRepository _repo;

	public JsonFlightRepositoryTests(ITestOutputHelper output)
	{
		_output = output;

		var mapper = TestHelper.CreateMapper();
		_repo = new JsonFlightRepository(mapper);

		_repo.GetAll().ToList().ForEach(_repo.Remove);
		_repo.Save();
	}

	public async Task InitializeAsync()
	{
		_stopwatch.Restart();

		_repo.Delete();

		await Task.CompletedTask;
	}

	public Task DisposeAsync()
	{
		_stopwatch.Stop();
		_output.WriteLine($"[TEST DURATION] {GetType().Name} - {DateTime.Now:HH:mm:ss.fff} - {_stopwatch.ElapsedMilliseconds} ms");

		_repo.Delete();

		return Task.CompletedTask;
	}

	[Fact]
	public void Add_And_Find_Flight()
	{
		var flight = new Flight { Number = "LO100", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(2) };
		_repo.Add(flight);
		_repo.Save();

		var found = _repo.Find(flight.Id);
		Assert.NotNull(found);
		Assert.Equal(flight.Number, found!.Number);
	}

	[Fact]
	public void RemoveById_Should_Remove()
	{
		var flight = new Flight { Number = "LO200", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(1) };
		_repo.Add(flight);
		_repo.Save();

		_repo.RemoveById(flight.Id);
		_repo.Save();

		Assert.Null(_repo.Find(flight.Id));
	}

	[Fact]
	public void GetAll_ShouldReturnAllFlights()
	{
		var flight1 = new Flight { Number = "LO300", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(1) };
		var flight2 = new Flight { Number = "LO301", DepartureTime = DateTimeOffset.UtcNow.AddHours(1), ArrivalTime = DateTimeOffset.UtcNow.AddHours(3) };

		_repo.Add(flight1);
		_repo.Add(flight2);
		_repo.Save();

		var all = _repo.GetAll().ToList();

		Assert.Contains(all, f => f.Id == flight1.Id);
		Assert.Contains(all, f => f.Id == flight2.Id);
	}

	[Fact]
	public void Update_ShouldUpdateFlight()
	{
		var flight = new Flight { Number = "LO400", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(2) };
		_repo.Add(flight);
		_repo.Save();

		flight.Number = "LO401";
		_repo.Update(flight);
		_repo.Save();

		var updated = _repo.Find(flight.Id);
		Assert.NotNull(updated);
		Assert.Equal("LO401", updated!.Number);
	}

	[Fact]
	public void Remove_ShouldRemoveFlight()
	{
		var flight = new Flight { Number = "LO500", DepartureTime = DateTimeOffset.UtcNow, ArrivalTime = DateTimeOffset.UtcNow.AddHours(2) };
		_repo.Add(flight);
		_repo.Save();

		_repo.Remove(flight);
		_repo.Save();

		Assert.Null(_repo.Find(flight.Id));
	}

	[Fact]
	public void Find_ShouldReturnNull_WhenNotExists()
	{
		var result = _repo.Find(Guid.NewGuid());

		Assert.Null(result);
	}
}
