using AngleSharp;
using PuppeteerSharp;

var pageToScrape = "https://www.games-workshop.com/en-US/legiones-astartes-mk6-tactical-squad-2022";

var scraper = new GamesWorkshopScraper();

var paintsList = await scraper.ScrapePageForPaints(pageToScrape);

var x = "Breakpoint line";

public class GamesWorkshopScraper
{
    private const string GW_RESOURCE_URL = "https://www.games-workshop.com/resources/";
    private const string COLOUR_LIST_CONTAINER_CLASSNAME = "simplebar-content";

    public async Task<IEnumerable<string>> ScrapePageForPaints(string pageUrl)
    {
        var result = new List<string>();

        var pageContent = await GetPageAsString(pageUrl);

        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(pageContent));

        var colourListContainer = document
            .QuerySelectorAll("*")
            .Where(domObject => domObject.LocalName == "div" && domObject.ClassName == COLOUR_LIST_CONTAINER_CLASSNAME)
            .FirstOrDefault();

        if(colourListContainer == null)
            return Enumerable.Empty<string>();

        // TODO - Move some of this logic out
        foreach(var colourList in colourListContainer.Children)
        {
            var titleDiv = colourList.GetElementsByClassName("effect-name"); 
            // TODO - Get the title out so we can group by the resulting paint
            var paintList = colourList.GetElementsByClassName("effect-elements").FirstOrDefault();

            if(paintList == null) continue;

            foreach (var paint in paintList.Children)
            {
                var paintName = RetrievePaintName(paint.InnerHtml);
                result.Add(paintName);
            }
        }
        return result;

    }

    private string RetrievePaintName(string paintHTMLImageString)
    {
        // Sample of the input string post-trim
        // https://www.games-workshop.com/resources/catalog/product/600x620/99189950025_baseAbaddonBlack.svg
        // Could just use some Regex here but not sure how much this string is likely to change

        var trimmed = paintHTMLImageString.Trim();

        var stringComponents = trimmed.Split('"');

        var resourceMatch = stringComponents.Where(component => component.Contains(GW_RESOURCE_URL)).FirstOrDefault();

        if (resourceMatch == null) return "";

        var paintNameLHS = resourceMatch.IndexOf('_')+1;
        var paintNameRHS = resourceMatch.LastIndexOf('.');
        var paintNameLength = paintNameRHS - paintNameLHS;

        var trimmedResourceMatch = resourceMatch.Substring(paintNameLHS, paintNameLength);

        // TODO - Some pretty printing with the name via a model
        // baseAbaddonBlack should come out as
        // Type = BASE
        // Name = AbaddonBlack

        return trimmedResourceMatch;
    }

    private async Task<string?> GetPageAsString(string pageUrl)
    {
        // TODO - Caching?
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

        var pageContent = string.Empty;

        var taskList = Task.Run(async () =>
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false, // Headless seems to not load the page correctly, get captcha'd
            });
            var page = await browser.NewPageAsync();

            await page.GoToAsync(pageUrl);

            pageContent = await page.GetContentAsync();

            await browser.CloseAsync();
        });
            
        // Workaround for Chromium launching and pulling thread back to main
        // Investigate if static will help
        Task.WaitAll(taskList);

        return pageContent;
    }
}