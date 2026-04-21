using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<Expense?> GetByIdWithRelationsAsync(int id) =>
        await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<Expense>> GetFilteredAsync(int? categoryId, int? userId, bool isAdmin)
    {
        var query = _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .AsQueryable();

        if (!isAdmin && userId.HasValue)
            query = query.Where(e => e.UserId == userId.Value);

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        return await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
    }
}