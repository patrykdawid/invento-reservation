using FluentValidation;
using backend.Models;
using backend.Database;

public class FlightDtoValidator : AbstractValidator<FlightDto>
{
	public FlightDtoValidator(IFlightRepository flightRepo)
	{
		RuleFor(f => f.Number)
			.NotEmpty()
			.WithMessage("Flight number is required.")
			.Must(number => !flightRepo.GetAll().Any(f => f.Number == number)) //TODO: optimization
			.WithMessage("Flight number must be unique.");

		RuleFor(f => f.DepartureTime)
			.NotEmpty()
			.WithMessage("Departure time is required.");

		RuleFor(f => f.ArrivalTime)
			.NotEmpty()
			.WithMessage("Arrival time is required.");

		RuleFor(f => f)
			.Must(f => f.DepartureTime < f.ArrivalTime)
			.WithMessage("Departure time must be earlier than arrival time.");
	}
}
