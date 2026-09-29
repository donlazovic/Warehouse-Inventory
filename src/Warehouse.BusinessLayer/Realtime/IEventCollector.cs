namespace Warehouse.BusinessLayer.Realtime;

public interface IEventCollector
{
    void Add(RealtimeEvent realtimeEvent);
    IReadOnlyList<RealtimeEvent> Drain();
}

public class EventCollector : IEventCollector
{
    private readonly List<RealtimeEvent> _events = new();

    public void Add(RealtimeEvent realtimeEvent) => _events.Add(realtimeEvent);

    public IReadOnlyList<RealtimeEvent> Drain()
    {
        var snapshot = _events.ToList();
        _events.Clear();
        return snapshot;
    }
}
