using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);

        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);

        Task<User?> GetUserWithRolesAsync(int userId);

        Task<bool> IsUsernameExistsAsync(string username);

        Task<bool> IsEmailExistsAsync(string email);

        Task<bool> HasAnyAdminAsync();

        Task<bool> IsLastAdminAsync(int userId);
    }
}