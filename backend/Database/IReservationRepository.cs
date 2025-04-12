using backend.Models;

namespace backend.Database;

public interface IReservationRepository
{
	IEnumerable<Reservation> GetAll();
	Reservation? Find(Guid id);
	void Add(Reservation reservation);
	void Remove(Reservation reservation);
	void RemoveById(Guid id);
	void Update(Reservation reservation);
	void Save();

#if DEBUG
	void Delete();
#endif
}
