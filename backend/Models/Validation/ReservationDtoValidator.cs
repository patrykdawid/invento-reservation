using FluentValidation;
using backend.Models;
using backend.Database;

public class ReservationDtoValidator : AbstractValidator<ReservationDto>
{
	public ReservationDtoValidator(IReservationRepository reservationRepo)
	{
		RuleFor(r => r.PassengerName)
			.NotEmpty()
			.WithMessage("Passenger name is required.");

		RuleFor(r => r.Class)
			.IsInEnum()
			.WithMessage("Invalid ticket class.");

		RuleFor(r => r.FlightId)
			.NotEmpty()
			.WithMessage("Flight ID is required.");

		RuleFor(r => r)
			.Must(r =>
			{
				return !reservationRepo.GetAll() //TODO: optimization
					.Any(x =>
						x.FlightId == r.FlightId &&
						x.PassengerName.ToLower() == r.PassengerName.ToLower());
			})
			.WithMessage("Passenger already exists on this flight.");
	}
}
