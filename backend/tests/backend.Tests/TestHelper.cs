using AutoMapper;
using backend.Models.Mapping;

namespace backend.Tests;

public static class TestHelper
{
	private static readonly string ReservationsPath = Path.Combine(AppContext.BaseDirectory, "!reservations.json");
	private static readonly string FlightsPath = Path.Combine(AppContext.BaseDirectory, "!flights.json");

	public static IMapper CreateMapper()
	{
		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<ReservationProfile>();
			cfg.AddProfile<FlightProfile>();
		});

		return config.CreateMapper();
	}
}
