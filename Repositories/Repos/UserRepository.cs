using Repositories.Models;
using Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repositories.ModelExtensions;

namespace Repositories.Repos
{
    public class UserRepository : IUserRepository
    {
        private readonly Context _context;
        private readonly PaginationExtension _paginationExtension;

        public UserRepository()
        {
            _context ??= new Context();
            _paginationExtension ??= new PaginationExtension();
        }

        public UserRepository(Context context, PaginationExtension paginationExtension)
        {
            _context = context;
            _paginationExtension = paginationExtension;
        }

        public async Task<User?> LoginUser (string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user =  await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            return user;
        }

        public async Task<User?> SignUpUser (User newUser)
        {
            if (newUser == null || string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.Password))
            {
                return null;
            }
            var existingUser = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == newUser.Email);
            if (existingUser != null)
            {
                return null;
            }
            _context.Set<User>().Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }

        // GetUsers: có pagination
        public async Task<PaginationResult<User>> GetUsers(int currentPage, int pageSize)
        {
            IQueryable<User> user = _context.Set<User>();
            return await _paginationExtension.PaginateAsync(user, currentPage, pageSize);
        }
    }
}
