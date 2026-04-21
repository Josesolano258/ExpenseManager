using Domain.Entities;

namespace Application.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<bool> ExistsByNameAsync(string name);
}