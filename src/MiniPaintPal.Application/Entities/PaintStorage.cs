namespace MiniPaintPal.Application.Entities;

public record PaintStorage
{
    public IEnumerable<Paint> MyPaints { get; set; }
}
