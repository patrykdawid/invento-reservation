using AutoMapper;
using backend.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.Database;

public class JsonFlightRepository : IFlightRepository
{
	private static readonly string FlightsPath = Path.Combine(AppContext.BaseDirectory, "!flights.json");
	private static readonly object FileLock = new();

	private readonly IMapper _mapper;
	private readonly ILogger<JsonFlightRepository> _logger;

	private static readonly JsonSerializerOptions Options = new()
	{
		WriteIndented = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Converters = { new JsonStringEnumConverter() }
	};

	private List<Flight> _flights = [];

	public JsonFlightRepository(IMapper mapper, ILogger<JsonFlightRepository> logger)
	{
		_mapper = mapper;
		_logger = logger;

		Load();
	}

	private void Load()
	{
		lock (FileLock)
		{
			try
			{
				if (File.Exists(FlightsPath))
				{
					var json = File.ReadAllText(FlightsPath);
					_flights = JsonSerializer.Deserialize<List<Flight>>(json, Options) ?? [];
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while loading the flights file: {Path}", FlightsPath);
				_flights = [];
			}
		}
	}

	public void Save()
	{
		try
		{
			var json = JsonSerializer.Serialize(_flights, Options);

			lock (FileLock)
			{
				using var stream = new FileStream(FlightsPath, FileMode.Create, FileAccess.Write, FileShare.Read);
				using var writer = new StreamWriter(stream);
				writer.Write(json);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while saving the flights file: {Path}", FlightsPath);
			throw new IOException("Failed to save flight data.", ex);
		}
	}

	[ExcludeFromCodeCoverage]
#if DEBUG
	public void Delete()
	{
		lock (FileLock)
		{
			if (File.Exists(FlightsPath))
				File.Delete(FlightsPath);

			_flights = [];
		}
	}
#endif

	public IEnumerable<Flight> GetAll() => _flights;

	public Flight? Find(Guid id) => _flights.SingleOrDefault(f => f.Id == id);

	public void Add(Flight flight)
	{
		if (flight.Id == Guid.Empty)
			flight.Id = Guid.NewGuid();

		_flights.Add(flight);
	}

	public void Remove(Flight flight)
	{
		if (flight == null)
			throw new ArgumentException("Flight is required");

		_flights.Remove(flight);
	}

	public void RemoveById(Guid id)
	{
		var flight = Find(id) ?? throw new InvalidOperationException("Flight not found");
		Remove(flight);
	}

	public void Update(Flight flight)
	{
		if (flight == null || flight.Id == Guid.Empty)
			throw new ArgumentException("Flight ID is required");

		var existing = Find(flight.Id)
			?? throw new InvalidOperationException("Flight not found");

		_mapper.Map(flight, existing);
	}

	public bool ExistsByNumberExcept(string number, Guid exceptId) =>
		_flights.Any(f =>
			f.Id != exceptId &&
			f.Number.Equals(number, StringComparison.OrdinalIgnoreCase));
}
