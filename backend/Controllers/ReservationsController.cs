using AutoMapper;
using backend.Database;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
	private readonly IReservationRepository _reservations;
	private readonly IFlightRepository _flights;
	private readonly IMapper _mapper;

	public ReservationsController(IReservationRepository reservations, IFlightRepository flights, IMapper mapper)
	{
		_reservations = reservations;
		_flights = flights;
		_mapper = mapper;
	}

	[HttpGet]
	[ProducesResponseType(typeof(PagedResult<ReservationDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<ReservationDto>>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
	{
		var all = _reservations.GetAll().ToList();
		var totalCount = all.Count;

		var items = all
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.Select(_mapper.Map<ReservationDto>)
			.ToList();

		await Task.CompletedTask;

		var result = new PagedResult<ReservationDto>
		{
			Items = items,
			TotalCount = totalCount
		};

		return Ok(result);
	}

	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id)
	{
		var reservation = _reservations.Find(id);
		if (reservation == null)
			return NotFound();

		var dto = _mapper.Map<ReservationDto>(reservation);

		await Task.CompletedTask;

		return Ok(dto);
	}

	[HttpPost]
	[ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Create([FromBody] ReservationDto dto)
	{
		var (isValid, error, flight) = ValidateDto(dto);
		if (!isValid)
			return error!;

		var reservation = _mapper.Map<Reservation>(dto);
		reservation.Id = Guid.NewGuid();
		reservation.Flight = flight!;

		_reservations.Add(reservation);
		_reservations.Save();

		var resultDto = _mapper.Map<ReservationDto>(reservation);

		await Task.CompletedTask;

		return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, resultDto);
	}

	[HttpPut("{id}")]
	[ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Update(Guid id, [FromBody] ReservationDto dto)
	{
		var existing = _reservations.Find(id);
		if (existing == null)
			return NotFound();

		var (isValid, error, flight) = ValidateDto(dto);
		if (!isValid)
			return error!;

		_mapper.Map(dto, existing);
		existing.Flight = flight!;

		_reservations.Update(existing);
		_reservations.Save();

		var resultDto = _mapper.Map<ReservationDto>(existing);

		await Task.CompletedTask;

		return Ok(resultDto);
	}

	[HttpDelete("{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Delete(Guid id)
	{
		var reservation = _reservations.Find(id);
		if (reservation == null)
			return NotFound();

		_reservations.Remove(reservation);
		_reservations.Save();

		await Task.CompletedTask;

		return NoContent();
	}

	private (bool IsValid, IActionResult? ErrorResult, Flight? Flight) ValidateDto(ReservationDto dto)
	{
		if (!Enum.IsDefined(typeof(TicketClass), dto.Class))
			return (false, Problem("Invalid ticket class.", statusCode: StatusCodes.Status400BadRequest), null);

		var flight = _flights.Find(dto.FlightId);
		if (flight == null)
			return (false, Problem("Flight not found.", statusCode: StatusCodes.Status400BadRequest), null);

		return (true, null, flight);
	}
}
