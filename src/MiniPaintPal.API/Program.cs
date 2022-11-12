using MiniPaintPal.Application;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

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


app.Run();
