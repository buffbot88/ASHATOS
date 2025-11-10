using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ASHATGoddessClient.Profiling;

/// <summary>
/// Performance profiler for real-time analytics
/// </summary>
public class PerformanceProfiler
{
    private readonly Dictionary<string, ProfilerSample> _samples = new();
    private readonly Dictionary<string, List<float>> _history = new();
    private readonly int _historySize = 100;
    private readonly Stopwatch _frameTimer = new();
    private float _lastFrameTime;

    public float CurrentFPS { get; private set; }
    public float AverageFrameTime { get; private set; }
    public IReadOnlyDictionary<string, ProfilerSample> Samples => _samples;

    public PerformanceProfiler()
    {
        _frameTimer.Start();
    }

    /// <summary>
    /// Begin a profiling sample
    /// </summary>
    public void BeginSample(string name)
    {
        if (!_samples.ContainsKey(name))
        {
            _samples[name] = new ProfilerSample(name);
            _history[name] = new List<float>();
        }

        _samples[name].Begin();
    }

    /// <summary>
    /// End a profiling sample
    /// </summary>
    public void EndSample(string name)
    {
        if (_samples.TryGetValue(name, out var sample))
        {
            sample.End();
            
            // Update history
            if (_history.TryGetValue(name, out var history))
            {
                history.Add(sample.LastTime);
                
                if (history.Count > _historySize)
                {
                    history.RemoveAt(0);
                }
            }
        }
    }

    /// <summary>
    /// Update frame timing
    /// </summary>
    public void EndFrame()
    {
        _lastFrameTime = (float)_frameTimer.Elapsed.TotalMilliseconds;
        _frameTimer.Restart();

        CurrentFPS = 1000.0f / _lastFrameTime;
        
        // Calculate average frame time from history
        if (!_history.ContainsKey("Frame"))
        {
            _history["Frame"] = new List<float>();
        }

        var frameHistory = _history["Frame"];
        frameHistory.Add(_lastFrameTime);
        
        if (frameHistory.Count > _historySize)
        {
            frameHistory.RemoveAt(0);
        }

        AverageFrameTime = frameHistory.Average();
    }

    /// <summary>
    /// Get statistics for a sample
    /// </summary>
    public ProfilerStats GetStats(string sampleName)
    {
        if (_history.TryGetValue(sampleName, out var history) && history.Count > 0)
        {
            return new ProfilerStats
            {
                Name = sampleName,
                Average = history.Average(),
                Min = history.Min(),
                Max = history.Max(),
                Current = history.Last(),
                SampleCount = history.Count
            };
        }

        return new ProfilerStats { Name = sampleName };
    }

    /// <summary>
    /// Get all statistics
    /// </summary>
    public Dictionary<string, ProfilerStats> GetAllStats()
    {
        var stats = new Dictionary<string, ProfilerStats>();

        foreach (var name in _history.Keys)
        {
            stats[name] = GetStats(name);
        }

        return stats;
    }

    /// <summary>
    /// Clear all samples and history
    /// </summary>
    public void Clear()
    {
        _samples.Clear();
        _history.Clear();
    }

    /// <summary>
    /// Print profiling report to console
    /// </summary>
    public void PrintReport()
    {
        Console.WriteLine("\n=== Performance Profile ===");
        Console.WriteLine($"FPS: {CurrentFPS:F1} | Frame Time: {_lastFrameTime:F2}ms (avg: {AverageFrameTime:F2}ms)");
        Console.WriteLine("\nSample Statistics:");
        Console.WriteLine($"{"Name",-30} {"Current",-10} {"Avg",-10} {"Min",-10} {"Max",-10}");
        Console.WriteLine(new string('-', 70));

        foreach (var stats in GetAllStats().Values.OrderByDescending(s => s.Average))
        {
            Console.WriteLine($"{stats.Name,-30} {stats.Current,-10:F2}ms {stats.Average,-10:F2}ms {stats.Min,-10:F2}ms {stats.Max,-10:F2}ms");
        }

        Console.WriteLine("===========================\n");
    }
}

/// <summary>
/// Individual profiler sample
/// </summary>
public class ProfilerSample
{
    private readonly Stopwatch _stopwatch = new();
    
    public string Name { get; }
    public float LastTime { get; private set; }
    public int CallCount { get; private set; }

    public ProfilerSample(string name)
    {
        Name = name;
    }

    public void Begin()
    {
        _stopwatch.Restart();
    }

    public void End()
    {
        _stopwatch.Stop();
        LastTime = (float)_stopwatch.Elapsed.TotalMilliseconds;
        CallCount++;
    }
}

/// <summary>
/// Profiler statistics
/// </summary>
public class ProfilerStats
{
    public string Name { get; set; } = string.Empty;
    public float Current { get; set; }
    public float Average { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
    public int SampleCount { get; set; }
}

/// <summary>
/// Disposable profiler scope for convenient profiling
/// </summary>
public class ProfilerScope : IDisposable
{
    private readonly PerformanceProfiler _profiler;
    private readonly string _sampleName;

    public ProfilerScope(PerformanceProfiler profiler, string sampleName)
    {
        _profiler = profiler;
        _sampleName = sampleName;
        _profiler.BeginSample(_sampleName);
    }

    public void Dispose()
    {
        _profiler.EndSample(_sampleName);
    }
}

/// <summary>
/// Memory profiler for tracking memory usage
/// </summary>
public class MemoryProfiler
{
    public long GetTotalMemory()
    {
        return GC.GetTotalMemory(false);
    }

    public long GetTotalMemoryForced()
    {
        return GC.GetTotalMemory(true);
    }

    public string GetMemoryInfo()
    {
        var totalMemory = GetTotalMemory();
        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);

        return $"Memory: {totalMemory / 1024 / 1024}MB | GC: Gen0={gen0} Gen1={gen1} Gen2={gen2}";
    }

    public void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
