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
        public Task<User?> GetById(int id);
        public Task<User?> GetByEmail(string email);
        public Task<User?> Update(User user);
        public Task<User?> Activate(int id);
        public Task<User?> Deactivate(int id);
        public Task<bool> ChangePassword(int userId, string currentPassword, string newPassword);
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
        public async Task<User?> GetById(int id)
            => await _repository.GetById(id);
        public async Task<User?> GetByEmail(string email)
            => await _repository.GetByEmail(email);
        public async Task<User?> Update(User user)
            => await _repository.Update(user);
        public async Task<User?> Activate(int id)
            => await _repository.Activate(id);
        public async Task<User?> Deactivate(int id)
            => await _repository.Deactivate(id);
        public async Task<bool> ChangePassword(int userId, string currentPassword, string newPassword)
            => await _repository.ChangePassword(userId, currentPassword, newPassword);
    }
}