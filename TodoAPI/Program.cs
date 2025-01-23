using TodoAPI.Data;
using TodoAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoAPI.MessageBroker.Services;
using MassTransit;
using TodoAPI.MessageBroker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<TodoAPI.Interfaces.ITodoRepository, TodoAPI.Services.TodoRepository>();
builder.Services.AddScoped<TodoAPI.Interfaces.IMpesaServices, TodoAPI.Services.mpesaservices>();
builder.Services.AddScoped<IRabbitMQPublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IRabbitMQConsumer, RabbitMQConsumer>();
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

builder.Services.Configure<RabbitMQSetting>(builder.Configuration.GetSection("RabbitMQ"));

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
