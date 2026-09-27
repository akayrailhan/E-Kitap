using EBook.Application.Abstractions.Persistence;
using EBook.Application.Abstractions.Storage;
using EBook.Application.Books.Upload;
using EBook.Infrastructure.Persistence;
using EBook.Infrastructure.Storage;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

var envFilePath = FindEnvironmentFile(Directory.GetCurrentDirectory());
if (envFilePath is not null)
{
    Env.Load(envFilePath);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EBookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBookRepository, EfBookRepository>();
builder.Services.AddScoped<IBookUploadService, BookUploadService>();
builder.Services.AddSingleton<IFileStorage>(serviceProvider =>
{
    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
    var webRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
    return new LocalFileStorage(Path.Combine(webRootPath, "uploads"));
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

static string? FindEnvironmentFile(string startDirectory)
{
    var directory = new DirectoryInfo(startDirectory);

    while (directory is not null)
    {
        var environmentFilePath = Path.Combine(directory.FullName, ".env");
        if (File.Exists(environmentFilePath))
        {
            return environmentFilePath;
        }

        directory = directory.Parent;
    }

    return null;
}
