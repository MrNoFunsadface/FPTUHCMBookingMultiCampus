using Repositories.ModelExtensions;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<User?> LoginUser(string email, string password);
        public Task<User?> SignUpUser(User newUser);
        public Task<PaginationResult<User>> GetUsers(int currentPage, int pageSize);
    }
}