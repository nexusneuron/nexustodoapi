using TodoAPI.Data;
using TodoAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoAPI.MessageBroker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<TodoAPI.Interfaces.ITodoRepository, TodoAPI.Services.TodoRepository>();
builder.Services.AddScoped<TodoAPI.Interfaces.IMpesaServices, TodoAPI.Services.mpesaservices>();
builder.Services.AddScoped<IRabbitMQPublisher, RabbitMQPublisher>();
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    //,
                //options => options.EnableRetryOnFailure(
                //    maxRetryCount: 5,
                //    maxRetryDelay: System.TimeSpan.FromSeconds(30),
                //    errorNumbersToAdd: null));
});

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
