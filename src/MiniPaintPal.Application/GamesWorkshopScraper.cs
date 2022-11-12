using AngleSharp;
using MiniPaintPal.Application.Entities;
using MiniPaintPal.Application.Helpers;
using PuppeteerSharp;

namespace MiniPaintPal.Application;

public interface IGamesWorkshopScraper
{
    Task<IEnumerable<Paint>> ScrapePageForPaints(string pageUrl);
}

public class GamesWorkshopScraper : IGamesWorkshopScraper
{
    private const string GW_RESOURCE_URL = "https://www.games-workshop.com/resources/";
    private const string COLOUR_LIST_CONTAINER_CLASSNAME = "simplebar-content";

    public async Task<IEnumerable<Paint>> ScrapePageForPaints(string pageUrl)
    {
        var result = new List<Paint>();

        var pageContent = await GetPageAsString(pageUrl);

        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(pageContent));

        var colourListContainer = document
            .QuerySelectorAll("*")
            .Where(domObject => domObject.LocalName == "div" && domObject.ClassName == COLOUR_LIST_CONTAINER_CLASSNAME)
            .FirstOrDefault();

        if (colourListContainer == null)
            return Enumerable.Empty<Paint>();

        // TODO - Move some of this logic out
        foreach (var colourList in colourListContainer.Children)
        {
            var titleDiv = colourList.GetElementsByClassName("effect-name");
            // TODO - Get the title out so we can group by the resulting paint
            var paintList = colourList.GetElementsByClassName("effect-elements").FirstOrDefault();

            if (paintList == null) continue;

            foreach (var paintElement in paintList.Children)
            {
                var paintNameRaw = RetrievePaintName(paintElement.InnerHtml);
                var paintNameComponents = paintNameRaw.SplitCamelCase(' ');
                var paint = new Paint
                {
                    Type = paintNameComponents[0],
                    Name = string.Join(' ', paintNameComponents[1..]),
                };
                result.Add(paint);
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

        var paintNameLHS = resourceMatch.IndexOf('_') + 1;
        var paintNameRHS = resourceMatch.LastIndexOf('.');
        var paintNameLength = paintNameRHS - paintNameLHS;

        var trimmedResourceMatch = resourceMatch.Substring(paintNameLHS, paintNameLength);

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
