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
    }
}