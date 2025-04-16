using AutoMapper;

namespace backend.Models.Mapping
{
	public class ReservationProfile : Profile
	{
		public ReservationProfile()
		{
			CreateMap<Reservation, Reservation>()
				.ForMember(dest => dest.Flight, opt => opt.Ignore())
				.ForMember(dest => dest.FlightId, opt => opt.MapFrom(src => src.Flight.Id));

			CreateMap<ReservationDto, Reservation>()
				.ForMember(dest => dest.Flight, opt => opt.Ignore());
			CreateMap<Reservation, ReservationDto>();
		}
	}
}
