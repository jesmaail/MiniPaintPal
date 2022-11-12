using MiniPaintPal.Application;
using MiniPaintPal.Application.Entities;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.Configure<PaintStorage>(configuration.GetSection("PaintStorage"));

services.AddScoped<IPaintRepository, MyPaintRepository>();
services.AddScoped<IGamesWorkshopScraper, GamesWorkshopScraper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/scrape/paints", async (string url, IGamesWorkshopScraper scraper) => await scraper.ScrapePageForPaints(url));

app.MapGet("/list/paints", async (IPaintRepository repository) => await repository.GetPaintsList());

app.Run();
