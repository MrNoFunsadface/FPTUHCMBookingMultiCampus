using Repositories.ModelExtensions;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<User?> LoginUser(string email, string password);
        public Task<User?> SignUpUser(User newUser);
        public Task<PaginationResult<User>> GetUsers(int currentPage, int pageSize);
        Task<User?> GetById(int id);
        Task<User?> Update(User user);
        Task<bool> Delete(int id);
        Task<User?> Activate(int id);
        Task<User?> Deactivate(int id);
        Task<bool> ChangePassword(int userId, string currentPassword, string newPassword);
    }
}