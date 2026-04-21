namespace Application.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    ICategoryRepository Categories { get; }
    IExpenseRepository Expenses { get; }
    IUserRoleRepository UserRoles { get; }
    Task<int> SaveAsync();
}