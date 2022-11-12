using Microsoft.Extensions.Options;
using MiniPaintPal.Application.Entities;

namespace MiniPaintPal.Application;

public interface IPaintRepository
{
    Task<IEnumerable<Paint>> GetPaintsList();
}

// Lazy way of doing it via the Config, can be changed later for a better serialisation format
public class MyPaintRepository : IPaintRepository
{
    private readonly IEnumerable<Paint> _paintList;

    public MyPaintRepository(IOptionsMonitor<PaintStorage> options)
    {
        _paintList = options.CurrentValue.MyPaints;
    }

    public async Task<IEnumerable<Paint>> GetPaintsList() => _paintList;
}
