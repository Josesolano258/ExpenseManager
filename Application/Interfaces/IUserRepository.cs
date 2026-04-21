using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailWithRolesAsync(string email);
    Task<User?> GetByIdWithRolesAsync(int id);
}