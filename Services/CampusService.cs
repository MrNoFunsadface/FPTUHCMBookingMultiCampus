using Repositories.Interfaces;
using Repositories.Models;

namespace Services;
public interface ICampusService
{
    public Task<List<Campus>> GetAll();
    public Task<Campus?> GetCampusById(int campusId);
    public Task<Campus> CreateCampus(Campus campus);
    public Task<Campus> UpdateCampus(Campus campus);
    public Task<bool> DeleteCampus(int campusId);
}

public class CampusService : ICampusService
{
    private readonly ICampusRepository _repository;
    public CampusService(ICampusRepository repository) 
    {
        _repository = repository;
    }

    public async Task<List<Campus>> GetAll()
        => await _repository.GetAll();
    public async Task<Campus?> GetCampusById(int campusId)
        => await _repository.GetCampusById(campusId);
    public async Task<Campus> CreateCampus(Campus campus)
        => await _repository.CreateCampus(campus);
    public async Task<Campus> UpdateCampus(Campus campus)
        => await _repository.UpdateCampus(campus);
    public async Task<bool> DeleteCampus(int campusId)
        => await _repository.DeleteCampus(campusId);
}