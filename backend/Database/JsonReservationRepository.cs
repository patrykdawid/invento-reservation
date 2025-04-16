using AutoMapper;
using backend.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.Database;

public class JsonReservationRepository : IReservationRepository
{
	private static readonly string ReservationsPath = Path.Combine(AppContext.BaseDirectory, "!reservations.json");
	private static readonly object FileLock = new();

	private readonly IMapper _mapper;
	private readonly IFlightRepository _flightRepo;
	private readonly ILogger<JsonReservationRepository> _logger;

	private static readonly JsonSerializerOptions Options = new()
	{
		WriteIndented = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Converters = { new JsonStringEnumConverter() }
	};

	private List<Reservation> _reservations = [];

	public JsonReservationRepository(IMapper mapper, IFlightRepository flightRepo, ILogger<JsonReservationRepository> logger)
	{
		_mapper = mapper;
		_flightRepo = flightRepo;
		_logger = logger;

		Load();
		AttachFlights();
	}

	private void Load()
	{
		lock (FileLock)
		{
			try
			{
				if (File.Exists(ReservationsPath))
				{
					var json = File.ReadAllText(ReservationsPath);
					_reservations = JsonSerializer.Deserialize<List<Reservation>>(json, Options) ?? [];
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while loading the reservations file: {Path}", ReservationsPath);
				_reservations = [];
			}
		}
	}

	public void Save()
	{
		try
		{
			var toSerialize = _mapper.Map<List<Reservation>>(_reservations);

			var json = JsonSerializer.Serialize(toSerialize, Options);

			lock (FileLock)
			{
				using var stream = new FileStream(ReservationsPath, FileMode.Create, FileAccess.Write, FileShare.Read);
				using var writer = new StreamWriter(stream);
				writer.Write(json);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while saving the reservations file: {Path}", ReservationsPath);
			throw new IOException("Failed to save reservation data.", ex);
		}
	}

	[ExcludeFromCodeCoverage]
#if DEBUG
	public void Delete()
	{
		lock (FileLock)
		{
			if (File.Exists(ReservationsPath))
				File.Delete(ReservationsPath);

			_reservations = [];
		}
	}
#endif

	public IEnumerable<Reservation> GetAll() => _reservations;

	public Reservation? Find(Guid id) => _reservations.SingleOrDefault(r => r.Id == id);

	public void Add(Reservation reservation)
	{
		if (reservation == null)
			throw new ArgumentException("Reservation is required");

		if (reservation.Id == Guid.Empty)
			reservation.Id = Guid.NewGuid();

		_reservations.Add(reservation);
	}

	public void Remove(Reservation reservation)
	{
		if (reservation == null)
			throw new ArgumentException("Reservation is required");

		_reservations.Remove(reservation);
	}

	public void RemoveById(Guid id)
	{
		var reservation = Find(id) ?? throw new InvalidOperationException("Reservation not found");
		Remove(reservation);
	}

	public void Update(Reservation reservation)
	{
		if (reservation == null || reservation.Id == Guid.Empty)
			throw new ArgumentException("Reservation ID is required");

		var existing = Find(reservation.Id)
			?? throw new InvalidOperationException("Reservation not found");

		_mapper.Map(reservation, existing);
		existing.Flight = _flightRepo.Find(reservation.FlightId)
			?? throw new InvalidOperationException("Flight not found");
	}

	public bool ExistsByPassengerAndFlightExcept(string passengerName, Guid flightId, Guid exceptId) =>
		_reservations.Any(r =>
			r.Id != exceptId &&
			r.FlightId == flightId &&
			r.PassengerName.Equals(passengerName, StringComparison.OrdinalIgnoreCase));

	private void AttachFlights()
	{
		foreach (var reservation in _reservations)
		{
			reservation.Flight = _flightRepo.Find(reservation.FlightId)
				?? throw new InvalidOperationException($"Missing flight {reservation.FlightId}");
		}
	}
}
