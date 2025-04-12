using AutoMapper;

namespace backend.Models.Mapping
{
	public class FlightProfile : Profile
	{
		public FlightProfile()
		{
			CreateMap<Flight, Flight>();

			CreateMap<Flight, FlightDto>();
		}
	}
}
