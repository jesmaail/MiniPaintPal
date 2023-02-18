using MiniPaintPal.Core;
using MiniPaintPal.Core.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddScoped<IWebScraper, WebScraper>();
services.AddScoped<IGWPaintRetrievalService, GWPaintRetrievalService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/ping", () => "Machine that goes Ping!")
    .WithName("Ping")
    .WithOpenApi();

app.MapGet("/retrieveGWPaints", async (IGWPaintRetrievalService service, string url) => await service.RetrievePaintsFromPage(url))
    .WithName("Retrieve GW Paints")
    .WithOpenApi();

app.Run();
