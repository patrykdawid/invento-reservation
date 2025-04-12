using backend.Models;

namespace backend.Database;

public interface IFlightRepository
{
	IEnumerable<Flight> GetAll();
	Flight? Find(Guid id);
	void Add(Flight flight);
	void Remove(Flight flight);
	void RemoveById(Guid id);
	void Update(Flight flight);
	void Save();

#if DEBUG
	void Delete();
#endif
}
