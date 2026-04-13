namespace ServiceStackApp;

public class MiddlewareInstanceTracker
{
    private readonly Dictionary<Type, string> _instances = new();
    private readonly Lock _gate = new();

    public string GetOrCreateId<TMiddleware>()
    {
        lock (_gate)
        {
            if (_instances.TryGetValue(typeof(TMiddleware), out var existing))
            {
                return existing;
            }

            var created = Guid.NewGuid().ToString("N")[..8];
            _instances[typeof(TMiddleware)] = created;
            return created;
        }
    }
}
