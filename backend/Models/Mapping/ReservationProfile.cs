using AutoMapper;

namespace backend.Models.Mapping
{
	public class ReservationProfile : Profile
	{
		public ReservationProfile()
		{
			CreateMap<Reservation, Reservation>()
				.ForMember(dest => dest.Flight, opt => opt.Ignore());

			CreateMap<ReservationDto, Reservation>()
				.ForMember(dest => dest.Flight, opt => opt.Ignore());
			CreateMap<Reservation, ReservationDto>();
		}
	}
}
