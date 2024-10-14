using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Web.WatermarkApp.BackgroundServices;
using RabbitMQ.Web.WatermarkApp.Models;
using RabbitMQ.Web.WatermarkApp.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext service BEFORE builder.Build()
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase(databaseName: "productDb"); //inMemory yazma sebebimiz datayý ram(memory) de tutarak hýzlý bir þekilde sunucuya göstermek 
});


//configuration metoduna direkt olarak eriþim yokmuþ builder üzerinden eriþtik
string? rabbitMqUri = builder.Configuration.GetConnectionString("RabbitMQ");

if (string.IsNullOrEmpty(rabbitMqUri))
{
    throw new InvalidOperationException("RabbitMQ baðlantý dizesi bulunamadý.");
}

builder.Services.AddSingleton(sp => new ConnectionFactory()
{
    Uri = new Uri(rabbitMqUri)
});


builder.Services.AddSingleton<RabbitMQClientService>();
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddHostedService<ImageWatermarkProcessBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
