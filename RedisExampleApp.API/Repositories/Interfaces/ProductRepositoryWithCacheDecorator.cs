using RedisExampleApp.API.Models;
using RedisExampleApp.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisExampleApp.API.Repositories.Interfaces;

public class ProductRepositoryWithCacheDecorator : IProductRepository
{
    private const string productKey = "productCaches";
    private readonly IProductRepository _repository;
    private readonly RedisService _redisService;
    private readonly IDatabase _cacheRepository;

    public ProductRepositoryWithCacheDecorator(IProductRepository repository, RedisService redisService)
    {
        _repository = repository;
        _redisService = redisService;
        _cacheRepository = _redisService.GetDb(1);

    }

    public async Task<Product> CreateAsync(Product product)
    {
        var newProduct = await _repository.CreateAsync(product);

        if (await _cacheRepository.KeyExistsAsync(productKey))  
            await _cacheRepository.HashSetAsync(productKey, product.Id.ToString(), JsonSerializer.Serialize(newProduct));

        return newProduct;
        



    }

    public async Task<List<Product>> GetAsync()
    {
        // Önbellekte ürünlerin bulunduğu anahtarın varlığını kontrol eder
        if (!await _cacheRepository.KeyExistsAsync(productKey))

            // Eğer önbellekte veri yoksa, veritabanından veri yükleyip önbelleğe koymak için loadToCacheFromDbAsync metodunu çağırır
            return await loadToCacheFromDbAsync();

        var products = new List<Product>();

        // Önbellekten tüm ürünleri alır
        var cacheProducts = await _cacheRepository.HashGetAllAsync(productKey);
        foreach (var item in cacheProducts.ToList())
        {
            // Her öğeyi JSON formatından ürün nesnesine dönüştürür ve ürün listesine ekler
            var product = JsonSerializer.Deserialize<Product>(item.Value);
            products.Add(product);
        }

        // Önbellekten alınan ürün listesini döndürür
        return products;
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        // Önbellekte ürünlerin bulunduğu anahtarın varlığını kontrol eder
        if (await _cacheRepository.KeyExistsAsync(productKey))
        {
            // Eğer önbellekte veri varsa, belirtilen 'id' değerine sahip ürünü önbellekten alır
            var product = await _cacheRepository.HashGetAsync(productKey, id.ToString());

            // Önbellekte ürün bulunuyorsa, JSON formatından ürün nesnesine dönüştürüp döndürür; aksi halde null döndürür
            return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
        }

        // Önbellekte veri yoksa, veritabanından tüm ürünleri yüklemek için loadToCacheFromDbAsync metodunu çağırır
        var products = await loadToCacheFromDbAsync();


        // Veritabanından alınan ürünler arasından belirtilen 'id' değerine sahip olanı bulur ve döndürür; bulunamazsa null döndürür
        return products.FirstOrDefault(x => x.Id == id);
    }

    private async Task<List<Product>> loadToCacheFromDbAsync()
    {
        var products = await _repository.GetAsync();

        products.ForEach(p =>
        {
            _cacheRepository.HashSetAsync(productKey, p.Id.ToString(), JsonSerializer.Serialize(p));
        });

        return products;
    }
}
