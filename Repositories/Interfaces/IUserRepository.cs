using Repositories.ModelExtensions;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IUserRepository
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
}