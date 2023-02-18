using AngleSharp;
using AngleSharp.Dom;
using PuppeteerSharp;

namespace MiniPaintPal.Core;

public interface IWebScraper
{
    Task<IDocument> GetPageDocument(string pageUrl);
}

public class WebScraper : IWebScraper
{
    public async Task<IDocument> GetPageDocument(string pageUrl)
    {
        var pageContent = await GetPageAsString(pageUrl);

        var context = BrowsingContext.New(Configuration.Default);

        var document = await context.OpenAsync(req => req.Content(pageContent));

        return document;
    }

    private async Task<string?> GetPageAsString(string pageUrl)
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

        var pageContent = string.Empty;

        var taskList = Task.Run(async () =>
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false, // Headless seems to not load the page correctly, gets captcha'd
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
