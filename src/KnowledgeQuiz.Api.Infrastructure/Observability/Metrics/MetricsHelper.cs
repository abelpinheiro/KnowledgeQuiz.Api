using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace KnowledgeQuiz.Api.Infrastructure.Observability.Telemetry;

public static class MetricsHelper
{
    public static async Task<T> TrackDurationAsync<T>(
        Histogram<double> histogram,
        Func<Task<T>> func,
        params KeyValuePair<string, object>[] tags)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await func();
        }
        finally
        {
            sw.Stop();
            histogram.Record(sw.Elapsed.TotalMilliseconds, tags);
        }
    }
    
    public static async Task TrackDurationAsync(
        Histogram<double> histogram,
        Func<Task> func,
        params KeyValuePair<string, object>[] tags)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await func();
        }
        finally
        {
            sw.Stop();
            histogram.Record(sw.Elapsed.TotalMilliseconds, tags);
        }
    }
}