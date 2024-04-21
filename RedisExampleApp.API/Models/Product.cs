namespace RedisExampleApp.API.Models;

public sealed class Product : BaseEntity
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
}
