using Repositories.Interfaces;
using Repositories.ModelExtensions;
using Repositories.Models;

namespace Services
{
    public interface IUserService
    {
        public Task<User?> LoginUser(string email, string password);
        public Task<User?> SignUpUser(User newUser);
        public Task<PaginationResult<User>> GetUsers(int currentPage, int pageSize);
        Task<User?> GetById(int id);
        Task<User?> Update(User user);
        Task<User?> Activate(int id);
        Task<User?> Deactivate(int id);
        Task<bool> ChangePassword(int userId, string currentPassword, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<User?> LoginUser(string email, string password)
            => await _repository.LoginUser(email, password);
        public async Task<User?> SignUpUser(User newUser)
            => await _repository.SignUpUser(newUser);
        public async Task<PaginationResult<User>> GetUsers(int currentPage, int pageSize)
            => await _repository.GetUsers(currentPage, pageSize);

        public Task<User?> GetById(int id)
            => _repository.GetById(id);

        public Task<User?> Update(User user)
            => _repository.Update(user);

        public Task<User?> Activate(int id)
            => _repository.Activate(id);

        public Task<User?> Deactivate(int id)
            => _repository.Deactivate(id);

        public Task<bool> ChangePassword(int userId, string currentPassword, string newPassword)
            => _repository.ChangePassword(userId, currentPassword, newPassword);
    }
}