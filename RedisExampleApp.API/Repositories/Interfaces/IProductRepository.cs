using RedisExampleApp.API.Models;

namespace RedisExampleApp.API.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAsync();
        Task<Product> GetByIdAsync(Guid Id);
        Task<Product> CreateAsync(Product product);
    }
}
