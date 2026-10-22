using System.Text.Json;
using Managed.PGlite.Initialization;

namespace Managed.PGlite;

/// <summary>Observations of actual initdb-to-backend invocations. Command counts
/// cannot establish that original bootstrap and post-bootstrap execution ran.</summary>
internal sealed class PgliteInitializationTrace(string diagnostics)
{
    private sealed record Observation(int Index, BootstrapOperation Operation,
        string[] Arguments, int? Status, string? Failure);
    private readonly List<Observation> observations = [];

    internal void Complete(int index, BootstrapInvocation request, int? status, Exception? failure)
    {
        try
        {
            Record(index, request, status, failure);
            // The process transport removes only its own private spools.
            // Preserve original BKI/SQL and backend output before that cleanup.
            foreach (var (path, suffix) in new[] { (request.InputPath, ".stdin"),
                (request.OutputPath, ".stdout"), (request.ErrorPath, ".stderr") })
                if (File.Exists(path)) File.Copy(path, Path.Combine(diagnostics, $"bootstrap-{index:000}{suffix}"));
        }
        catch (Exception diagnosticFailure) when (failure != null)
        {
            // Called from a finally while the original backend exception is
            // unwinding. Keep that exception first, and retain the diagnostic
            // failure too; neither may replace or silently hide the other.
            throw new AggregateException("The translated backend failed and preserving its initialization diagnostics also failed.",
                failure, diagnosticFailure);
        }
    }

    internal void Record(int index, BootstrapInvocation request, int? status, Exception? failure)
    {
        observations.Add(new(index, request.Operation, request.Arguments.ToArray(), status, failure?.ToString()));
        // Explicit JSON writes keep normally trimmed consumers independent of
        // reflection serialization and preserve failures before initdb returns.
        using var stream = File.Create(Path.Combine(diagnostics, "backend-invocations.json"));
        using var json = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        json.WriteStartArray();
        foreach (var observation in observations)
        {
            json.WriteStartObject();
            json.WriteNumber("index", observation.Index);
            json.WriteString("operation", observation.Operation.ToString());
            json.WriteStartArray("arguments");
            foreach (string argument in observation.Arguments) json.WriteStringValue(argument);
            json.WriteEndArray();
            if (observation.Status is int code) json.WriteNumber("status", code);
            else json.WriteNull("status");
            json.WriteString("failure", observation.Failure);
            json.WriteEndObject();
        }
        json.WriteEndArray();
    }

    internal void RequireCompletedOperations()
    {
        bool Completed(BootstrapOperation operation) => observations.Any(observation =>
            observation.Operation == operation && observation.Status == 0 && observation.Failure == null);
        if (!Completed(BootstrapOperation.Bootstrap) || !Completed(BootstrapOperation.SingleUser))
            throw new IOException($"Original initdb did not complete successful Bootstrap and SingleUser backend operations; diagnostics: {diagnostics}");
    }
}
