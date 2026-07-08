using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == usernameOrEmail ||
                    u.Email == usernameOrEmail);
        }

        public async Task<User?> GetUserWithRolesAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task<bool> HasAnyAdminAsync()
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .AnyAsync(ur =>
                    ur.Role.RoleName == "Admin" &&
                    ur.User.IsActive);
        }

        public async Task<bool> IsLastAdminAsync(int userId)
        {
            var adminCount = await _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .CountAsync(ur =>
                    ur.Role.RoleName == "Admin" &&
                    ur.User.IsActive);

            var isCurrentUserAdmin = await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur =>
                    ur.UserId == userId &&
                    ur.Role.RoleName == "Admin");

            return isCurrentUserAdmin && adminCount <= 1;
        }
    }
}