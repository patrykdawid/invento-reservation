﻿using AutoMapper;
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
	[ProducesResponseType(typeof(IEnumerable<FlightDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<FlightDto>>> Get([FromQuery] bool withReservations = false)
	{
		var items = _flights.GetAll()
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

		return Ok(items);
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

	[HttpPost]
	[ProducesResponseType(typeof(FlightDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Create([FromBody] FlightDto dto)
	{
		var flight = _mapper.Map<Flight>(dto);
		flight.Id = Guid.NewGuid();

		_flights.Add(flight);
		_flights.Save();

		var resultDto = _mapper.Map<FlightDto>(flight);

		await Task.CompletedTask;

		return CreatedAtAction(nameof(GetById), new { id = flight.Id }, resultDto);
	}

	[HttpPut("{id}")]
	[ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Update(Guid id, [FromBody] FlightDto dto)
	{
		var existing = _flights.Find(id);
		if (existing == null)
			return NotFound();

		if (dto.DepartureTime >= dto.ArrivalTime)
			return BadRequest("Odlot musi być wcześniejszy niż przylot.");

		_mapper.Map(dto, existing);
		_flights.Update(existing);
		_flights.Save();

		var resultDto = _mapper.Map<FlightDto>(existing);

		await Task.CompletedTask;

		return Ok(resultDto);
	}
}
