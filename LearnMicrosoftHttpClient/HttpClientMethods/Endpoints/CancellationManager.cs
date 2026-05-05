using System.Collections.Concurrent;

namespace HttpClientMethods.Methods
{
    public class CancellationManager
    {
        // Stores CTS by a unique key (e.g., a "JobId" or User ID)
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _sources = new();

        public CancellationToken GetToken(string key)
        {
            var cts = _sources.GetOrAdd(key, _ => new CancellationTokenSource());
            return cts.Token;
        }

        public void Cancel(string key)
        {
            if (_sources.TryRemove(key, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}
