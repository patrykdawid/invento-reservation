using AutoMapper;
using backend.Database;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
	private readonly IFlightRepository _flights;
	private readonly IMapper _mapper;

	public FlightsController(IFlightRepository flights, IMapper mapper)
	{
		_flights = flights;
		_mapper = mapper;
	}

	[HttpGet]
	[ProducesResponseType(typeof(IEnumerable<FlightDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<FlightDto>>> GetAll()
	{
		var flights = _flights.GetAll()
			.Select(_mapper.Map<FlightDto>)
			.ToList();

		await Task.CompletedTask;

		return Ok(flights);
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
