using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRoleRepository
{
    Task AddAsync(UserRole userRole);
}