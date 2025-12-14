using Repositories.Interfaces;
using Repositories.Models;

namespace Services;
public interface ISlotService
{
    public Task<List<Slot>> GetAll();
}

public class SlotService : ISlotService
{
    private readonly ISlotRepository _repository;
    public SlotService(ISlotRepository repository) 
    {
        _repository = repository;
    }

    public async Task<List<Slot>> GetAll() 
        => await _repository.GetAll();
}