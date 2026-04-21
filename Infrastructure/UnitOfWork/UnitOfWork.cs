using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;

namespace Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private ICategoryRepository? _categories;
    private IExpenseRepository? _expenses;
    private IUserRoleRepository? _userRoles;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IExpenseRepository Expenses => _expenses ??= new ExpenseRepository(_context);
    public IUserRoleRepository UserRoles => _userRoles ??= new UserRoleRepository(_context);

    public Task<int> SaveAsync() => _context.SaveChangesAsync();
}