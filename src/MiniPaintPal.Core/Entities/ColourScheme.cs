namespace MiniPaintPal.Core.Entities;

public record ColourScheme
{
    public string Name { get; set; }
    public IEnumerable<Paint> Paints { get; set; }
}
