using Repositories.Models;
namespace Repositories.Interfaces;
public interface ICampusRepository
{
    public Task<List<Campus>> GetAll();
    public Task<Campus?> GetCampusById(int campusId);
    public Task<Campus> CreateCampus(Campus campus);
    public Task<Campus> UpdateCampus(Campus campus);
}
