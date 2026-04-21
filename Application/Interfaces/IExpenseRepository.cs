using Domain.Entities;

namespace Application.Interfaces;

public interface IExpenseRepository : IGenericRepository<Expense>
{
    Task<Expense?> GetByIdWithRelationsAsync(int id);
    Task<IEnumerable<Expense>> GetFilteredAsync(int? categoryId, int? userId, bool isAdmin);
}