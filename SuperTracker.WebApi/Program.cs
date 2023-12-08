using Microsoft.AspNetCore.Mvc;
using SuperTracker.Application.Dtos;
using SuperTracker.Core.Configurations;
using SuperTracker.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// Add services to the container.
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

app.UseHttpsRedirection();

app.MapGet("/track", (HttpRequest request, 
  [FromServices] IRabbitMQService rabbitMQService,
  [FromServices] RabbitMQConfiguration rabbitMQConfiguration) =>
{
  var userAgent = request.Headers.UserAgent;
  var referer = request.Headers.Referer;
  var ip = request.HttpContext.Connection.RemoteIpAddress?.ToString();

  rabbitMQService.Publish(new TrackingDetailsDto(userAgent!, referer!, ip!), rabbitMQConfiguration.QueueName);

   var imagePath = "Assets/asset.gif";

  var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);

  return Results.File(physicalPath, "image/gif");
})
.WithName("Track")
.WithOpenApi();

app.Run();