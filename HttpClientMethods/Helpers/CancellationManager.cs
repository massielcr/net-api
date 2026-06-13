using System.Collections.Concurrent;

namespace HttpClientMethods.Helpers
{
    public class CancellationManager
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _sources = new();

        public CancellationToken GetToken(string key, int? seconds = null)
        {
            while (true)
            {
                if (_sources.TryGetValue(key, out var existingCts))
                {
                    if (existingCts.IsCancellationRequested)
                    {
                        if (_sources.TryRemove(key, out var oldCts))
                        {
                            oldCts.Dispose();
                        }
                        continue; // Loop back and create a fresh one
                    }

                    if (seconds.HasValue)
                    {
                        existingCts.CancelAfter(TimeSpan.FromSeconds(seconds.Value));
                    }
                    return existingCts.Token;
                }

                var newCts = new CancellationTokenSource();
                if (_sources.TryAdd(key, newCts))
                {
                    if (seconds.HasValue)
                    {
                        newCts.CancelAfter(TimeSpan.FromSeconds(seconds.Value));
                    }
                    return newCts.Token;
                }

                newCts.Dispose();
            }
        }

        public void Cancel(string key)
        {
            if (_sources.TryRemove(key, out var cts))
            {
                try
                {
                    if (!cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }
    }
}
