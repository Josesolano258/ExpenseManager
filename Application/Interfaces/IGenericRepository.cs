namespace Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    IEnumerable<T> Find(Func<T, bool> predicate);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}