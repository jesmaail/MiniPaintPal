using MiniPaintPal.Application;

var builder = WebApplication.CreateBuilder(args);

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

var gamesWorkshopScraper = new GamesWorkshopScraper();

app.MapGet("/paints", async (string url) => await gamesWorkshopScraper.ScrapePageForPaints(url));

app.Run();
