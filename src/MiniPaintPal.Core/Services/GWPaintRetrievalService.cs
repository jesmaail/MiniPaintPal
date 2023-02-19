using AngleSharp.Dom;
using MiniPaintPal.Core.Entities;
using MiniPaintPal.Core.Extensions;

namespace MiniPaintPal.Core.Services;
public interface IGWPaintRetrievalService
{
    Task<IEnumerable<ColourScheme>> RetrieveColoursFromPage(string url);

    Task<IEnumerable<Paint>> RetrievePaintsFromPage(string url);
}

public class GWPaintRetrievalService : IGWPaintRetrievalService
{
    private const string GW_RESOURCE_URL = "https://www.games-workshop.com/resources/";
    private const string COLOUR_LIST_CONTAINER_CLASSNAME = "simplebar-content";

    private readonly IWebScraper _webScraper;
    public GWPaintRetrievalService(IWebScraper webScraper) 
        => (_webScraper) = (webScraper);

    public async Task<IEnumerable<ColourScheme?>> RetrieveColoursFromPage(string url)
    {
        var colourListContainer = await GetColourListContainer(url);

        if (colourListContainer == null)
            return Enumerable.Empty<ColourScheme>();

        return colourListContainer.Children.ToList()
            .Select(colourScheme => GetColourSchemeDetails(colourScheme));
    }

    public async Task<IEnumerable<Paint>> RetrievePaintsFromPage(string url)
    {
        var colourListContainer = await GetColourListContainer(url);

        if (colourListContainer == null)
            return Enumerable.Empty<Paint>();

        return colourListContainer.Children.ToList()
            .SelectMany(colourScheme => GetPaintsForColour(colourScheme));
    }

    private async Task<IElement?> GetColourListContainer(string url)
    {
        var pageDocument = await _webScraper.GetPageDocument(url);

        return pageDocument
            .QuerySelectorAll("*")
            .Where(domObj => domObj.LocalName == "div" && domObj.ClassName == COLOUR_LIST_CONTAINER_CLASSNAME)
            .FirstOrDefault();
    }

    private ColourScheme GetColourSchemeDetails(IElement colourListElement)
    {
        var colourNameContainer = colourListElement.GetElementsByClassName("effect-name");
        var colourName = colourNameContainer[0].TextContent.Trim();

        return new ColourScheme
        {
            Name = colourName,
            Paints = GetPaintsForColour(colourListElement)
        };
    }

    private IEnumerable<Paint> GetPaintsForColour(IElement colourListElement)
    {
        var paintsResult = new List<Paint>();

        var paintList = colourListElement.GetElementsByClassName("effect-elements").FirstOrDefault();

        if (paintList == null) 
            return Enumerable.Empty<Paint>();

        return paintList.Children.ToList()
            .Select(paintElement => ExtractPaintDetails(paintElement.InnerHtml));
    }

    private Paint ExtractPaintDetails(string paintHTMLImageString)
    {
        var rawPaintString = RetrievePaintName(paintHTMLImageString);

        var paintComponents = rawPaintString.SplitCamelCase(' ');

        return new Paint
        {
            Type = paintComponents[0].ConvertToCapitalStartChar(),
            Name = string.Join(' ', paintComponents[1..]),
            Brand = Constants.GWPaintName
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
