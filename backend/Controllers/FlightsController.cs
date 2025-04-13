using AutoMapper;
using backend.Database;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
	private readonly IReservationRepository _reservations;
	private readonly IFlightRepository _flights;
	private readonly IMapper _mapper;

	public FlightsController(IReservationRepository reservations, IFlightRepository flights, IMapper mapper)
	{
		_reservations = reservations;
		_flights = flights;
		_mapper = mapper;
	}

	[HttpGet]
	[ProducesResponseType(typeof(PagedResult<FlightDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<FlightDto>>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] bool withReservations = false)
	{
		var all = _flights.GetAll().ToList();
		var totalCount = all.Count;

		var items = all
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.Select(_mapper.Map<FlightDto>)
			.ToList();

		if (withReservations) {
			var allReservations = _reservations.GetAll().ToList();
			foreach (var flight in items)
			{
				var reservations = allReservations.Where(r => r.FlightId == flight.Id).ToList();
				flight.Reservations = reservations.Select(_mapper.Map<ReservationDto>).ToList();
			}
		}

		await Task.CompletedTask;

		var result = new PagedResult<FlightDto>
		{
			Items = items,
			TotalCount = totalCount,
		};

		return Ok(result);
	}

	[HttpGet("{id}")]
	[ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id)
	{
		var flight = _flights.Find(id);
		if (flight == null)
			return NotFound();

		var dto = _mapper.Map<FlightDto>(flight);

		await Task.CompletedTask;

		return Ok(dto);
	}
}
