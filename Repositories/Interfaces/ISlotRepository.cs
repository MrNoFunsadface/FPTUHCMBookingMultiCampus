using Repositories.Models;
namespace Repositories.Interfaces;
public interface ISlotRepository
{
    public Task<List<Slot>> GetAll();
}