using Microsoft.EntityFrameworkCore;

namespace Repositories.ModelExtensions
{
    public class PaginationExtension
    {
        public PaginationExtension() { }

        public async Task<PaginationResult<T>> PaginateAsync<T>(IQueryable<T> query, int currentPage, int pageSize) where T : class
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (currentPage <= 0) currentPage = 1;

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = await query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginationResult<T>
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = items
            };
        }

        public PaginationResult<T> Paginate<T>(IQueryable<T> query, int currentPage, int pageSize) where T : class
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (currentPage <= 0) currentPage = 1;

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            return new PaginationResult<T>
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = items
            };
        }
    }
}
