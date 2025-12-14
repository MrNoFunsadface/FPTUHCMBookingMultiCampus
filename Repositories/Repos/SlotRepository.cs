using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repos;
public class SlotRepository : ISlotRepository
{
    private readonly Context _context;
    public SlotRepository() 
    {
        _context = new Context();
    }
    public SlotRepository(Context context)
    {
        _context = context;
    }

    public async Task<List<Slot>> GetAll()
        => await _context.Slots.OrderBy(s => s.SlotNumber).ToListAsync();
}