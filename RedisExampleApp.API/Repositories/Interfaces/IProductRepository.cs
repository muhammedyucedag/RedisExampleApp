using RedisExampleApp.API.Models;

namespace RedisExampleApp.API.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAsync();
        Task<Product> GetByIdAsync(Guid id);
        Task<Product> CreateAsync(Product product);
    }
}
