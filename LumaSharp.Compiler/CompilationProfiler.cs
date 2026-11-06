using System.Diagnostics;

namespace LumaSharp.Compiler;

internal sealed record CompilationPhaseSample(
    string Name,
    TimeSpan Elapsed,
    long AllocatedBytes,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections);

/// <summary>Low-overhead opt-in phase measurements used by compiler benchmarks.</summary>
internal sealed class CompilationProfiler
{
    private readonly List<CompilationPhaseSample> _samples = [];

    public IDisposable Measure(string name) => new Scope(this, name);

    public T Measure<T>(string name, Func<T> action)
    {
        using var scope = Measure(name);
        return action();
    }

    public void Measure(string name, Action action)
    {
        using var scope = Measure(name);
        action();
    }

    public IReadOnlyList<CompilationPhaseSample> Snapshot() => _samples.ToArray();

    private void Record(string name, long startedAt, long allocatedBytes, int threadId, int gen0, int gen1, int gen2)
    {
        _samples.Add(new(name, Stopwatch.GetElapsedTime(startedAt),
            Environment.CurrentManagedThreadId == threadId ? GC.GetAllocatedBytesForCurrentThread() - allocatedBytes : -1,
            GC.CollectionCount(0) - gen0, GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2));
    }

    private sealed class Scope(CompilationProfiler owner, string name) : IDisposable
    {
        private readonly long _startedAt = Stopwatch.GetTimestamp();
        private readonly long _allocatedBytes = GC.GetAllocatedBytesForCurrentThread();
        private readonly int _threadId = Environment.CurrentManagedThreadId;
        private readonly int _gen0 = GC.CollectionCount(0);
        private readonly int _gen1 = GC.CollectionCount(1);
        private readonly int _gen2 = GC.CollectionCount(2);
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            owner.Record(name, _startedAt, _allocatedBytes, _threadId, _gen0, _gen1, _gen2);
        }
    }
}
