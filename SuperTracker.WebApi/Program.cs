using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SuperTracker.Application.Dtos;
using SuperTracker.Core.Configurations;
using SuperTracker.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapGet("/", () =>
{
  return Results.Ok("Hello World!");
});

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
