using EBook.Application.Abstractions.Persistence;
using EBook.Application.Abstractions.Documents;
using EBook.Application.Abstractions.Storage;
using EBook.Application.Books.Upload;
using EBook.Application.Books.Status;
using EBook.Application.Books.Generation;
using EBook.Infrastructure.Persistence;
using EBook.Infrastructure.Storage;
using EBook.Infrastructure.Documents;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var envFilePath = FindEnvironmentFile(Directory.GetCurrentDirectory());
if (envFilePath is not null)
{
    Env.Load(envFilePath);
}

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<EBookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBookRepository, EfBookRepository>();
builder.Services.AddScoped<IBookUploadService, BookUploadService>();
builder.Services.AddScoped<IBookStatusService, BookStatusService>();
builder.Services.AddScoped<IBookGenerationService, BookGenerationService>();
builder.Services.AddSingleton<IDocumentReader, OpenXmlDocumentReader>();
builder.Services.AddSingleton<IContactSanitizer, ContactSanitizer>();
builder.Services.AddSingleton<IBookPdfGenerator, QuestPdfBookGenerator>();
builder.Services.AddSingleton<IFileStorage>(serviceProvider =>
{
    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
    var webRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
    return new LocalFileStorage(Path.Combine(webRootPath, "uploads"));
});

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
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

app.UseCors();

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
