using MiniPaintPal.Core.Entities;
using MiniPaintPal.Core.Extensions;

namespace MiniPaintPal.Core.Services;
public interface IGWPaintRetrievalService
{
    Task<IEnumerable<Paint>> RetrievePaintsFromPage(string url);
}

public class GWPaintRetrievalService : IGWPaintRetrievalService
{
    private const string GW_RESOURCE_URL = "https://www.games-workshop.com/resources/";
    private const string COLOUR_LIST_CONTAINER_CLASSNAME = "simplebar-content";

    private readonly IWebScraper _webScraper;
    public GWPaintRetrievalService(IWebScraper webScraper) 
        => (_webScraper) = (webScraper);

    public async Task<IEnumerable<Paint>> RetrievePaintsFromPage(string url)
    {
        var pageDocument = await _webScraper.GetPageDocument(url);

        var colourListContainer = pageDocument
            .QuerySelectorAll("*")
            .Where(domObj => domObj.LocalName == "div" && domObj.ClassName == COLOUR_LIST_CONTAINER_CLASSNAME)
            .FirstOrDefault();

        if(colourListContainer == null)
            return Enumerable.Empty<Paint>();

        var paintsResult = new List<Paint>();

        foreach(var colourList in colourListContainer.Children)
        {
            var titleDiv = colourList.GetElementsByClassName("effect-name");

            // TODO - Get the title out so we can group paints into their resulting colours (as per the website)
            var paintList = colourList.GetElementsByClassName("effect-elements").FirstOrDefault();

            if(paintList == null) continue;

            var extractedPaints = paintList.Children.ToList()
                .Select(paintElement => ExtractPaintDetails(paintElement.InnerHtml));

            paintsResult.AddRange(extractedPaints);
        }

        return paintsResult;
            
    }

    private Paint ExtractPaintDetails(string paintHTMLImageString)
    {
        var rawPaintString = RetrievePaintName(paintHTMLImageString);

        var paintComponents = rawPaintString.SplitCamelCase(' ');

        return new Paint
        {
            Type = paintComponents[0],
            Name = string.Join(' ', paintComponents[1..]),
            Brand = "Citadel" // TODO - Enum/Const
        };
    }

    private string RetrievePaintName(string paintHTMLImageString)
    {
        // Sample of the input string post-trim
        // https://www.games-workshop.com/resources/catalog/product/600x620/99189950025_baseAbaddonBlack.svg

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
}
