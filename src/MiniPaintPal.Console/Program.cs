using MiniPaintPal.Application;

var pageToScrape = "https://www.games-workshop.com/en-US/legiones-astartes-mk6-tactical-squad-2022";

var scraper = new GamesWorkshopScraper();

var paintsList = await scraper.ScrapePageForPaints(pageToScrape);

var x = "Breakpoint line";