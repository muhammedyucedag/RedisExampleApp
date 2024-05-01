using Microsoft.EntityFrameworkCore;
using RedisExampleApp.API.Models;
using RedisExampleApp.API.Repositories.Interfaces;
using RedisExampleApp.API.Services;
using RedisExampleApp.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductService, ProductService>();

//Bu kod, veritaban� i�lemlerini ger�ekle�tiren bir repository s�n�f�n�, �nbellek i�levselli�i ekleyen bir dekorat�r s�n�f� ile birle�tirerek, uygulaman�n performans�n� art�rmay� ama�lar.
builder.Services.AddScoped<IProductRepository>(sp =>
{
    // AppDbContext hizmetini al�r
    var appDbContext = sp.GetRequiredService<AppDbContext>();

    // ProductRepository hizmetini olu�turur ve AppDbContext'i kullanarak ba��ml�l�klar�n� enjekte eder
    var productRepository = new ProductRepository(appDbContext);

    // RedisService hizmetini al�r
    var redisService = sp.GetRequiredService<RedisService>();

    // ProductRepositoryWithCacheDecorator hizmetini olu�turur ve �nceki ProductRepository ve RedisService'leri kullanarak ba��ml�l�klar�n� enjekte eder
    return new ProductRepositoryWithCacheDecorator(productRepository, redisService);
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("RedisDatabase");
});

builder.Services.AddSingleton<RedisService>(sp =>
{
    return new RedisService(builder.Configuration["CacheOptions:Url"]);
});

builder.Services.AddSingleton<IDatabase>(sp =>
{
    var redisService = sp.GetRequiredService<RedisService>();
    return redisService.GetDb(0);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
