using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repos;
public class CampusRepository : ICampusRepository
{
    private readonly Context _context;
    public CampusRepository() { _context = new Context(); }
    public CampusRepository(Context context) { _context = context; }

    public Task<List<Campus>> GetAll() => _context.Set<Campus>().ToListAsync();
    public Task<Campus?> GetCampusById(int campusId) => _context.Set<Campus>().FirstOrDefaultAsync(c => c.CampusId == campusId);
    public async Task<Campus> CreateCampus(Campus campus)
    {
        _context.Campuses.Add(campus);
        await _context.SaveChangesAsync();
        return campus;
    }
    public async Task<Campus> UpdateCampus(Campus campus)
    {
        _context.Campuses.Update(campus);
        await _context.SaveChangesAsync();
        return campus;
    }

    public async Task<bool> DeleteCampus(int campusId)
    {
        var campus = await _context.Campuses.FirstOrDefaultAsync(c => c.CampusId == campusId);
        if (campus == null) return false;

        _context.Campuses.Remove(campus);
        await _context.SaveChangesAsync();
        return true;
    }
}