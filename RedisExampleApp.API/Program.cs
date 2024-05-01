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

//Bu kod, veritabaný iþlemlerini gerçekleþtiren bir repository sýnýfýný, önbellek iþlevselliði ekleyen bir dekoratör sýnýfý ile birleþtirerek, uygulamanýn performansýný artýrmayý amaçlar.
builder.Services.AddScoped<IProductRepository>(sp =>
{
    // AppDbContext hizmetini alýr
    var appDbContext = sp.GetRequiredService<AppDbContext>();

    // ProductRepository hizmetini oluþturur ve AppDbContext'i kullanarak baðýmlýlýklarýný enjekte eder
    var productRepository = new ProductRepository(appDbContext);

    // RedisService hizmetini alýr
    var redisService = sp.GetRequiredService<RedisService>();

    // ProductRepositoryWithCacheDecorator hizmetini oluþturur ve önceki ProductRepository ve RedisService'leri kullanarak baðýmlýlýklarýný enjekte eder
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
