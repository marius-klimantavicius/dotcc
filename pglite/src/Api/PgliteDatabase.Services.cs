// Manual .NET adaptation of @electric-sql/pglite src/base.ts and src/pglite.ts (Apache-2.0).
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using Backend = Managed.PGlite.Translated.PGlite;

namespace Managed.PGlite;

public sealed unsafe partial class PgliteDatabase : IAsyncDisposable
{
    private PgliteTransaction? serviceTransaction;
    private bool serviceTransactionEntry, serviceDeliveringCallbacks, serviceRefreshing, serviceInitializing, serviceClosing;
    private bool serviceNeedsRefresh;
    private int serviceBatchDepth, serviceExclusiveDepth;
    private PgliteTransactionStatus serviceTransactionStatus = PgliteTransactionStatus.Idle;
    private readonly Queue<(IReadOnlyList<PgliteError> Notices, IReadOnlyList<PgliteNotification> Notifications)> serviceResponses = new();
    private readonly Dictionary<string, HashSet<Action<string>>> serviceListeners = new(StringComparer.Ordinal);
    private readonly HashSet<Action<string, string>> serviceGlobalListeners = [];
    private readonly Dictionary<string, object?> serviceNamespaces = new(StringComparer.Ordinal);
    private readonly List<Action> serviceExtensionClosers = [];
    private readonly List<Exception> serviceCloseErrors = [];
    private PgliteLive? serviceLive;

    public bool Ready => IsReady && !serviceClosing;
    public bool Closed => disposed;
    public bool IsInTransaction { get { RequireOwner(); return ReadServiceTransactionStatus() != PgliteTransactionStatus.Idle; } }
    private PgliteTransactionStatus ReadServiceTransactionStatus()
    {
        byte state = 0;
        if (Dispatch(() => state = Backend.TransactionBlockStatusCode()) != 0)
            throw new IOException("Cannot inspect the translated engine transaction state.");
        return serviceTransactionStatus = (PgliteTransactionStatus)state;
    }
    public event Action<PgliteError>? OnNotice;
    public IReadOnlyDictionary<string, object?> Extensions
    { get { RequireOwner(); return new ReadOnlyDictionary<string, object?>(serviceNamespaces); } }
    public T GetExtension<T>(string name) where T : class
    {
        RequireOwner();
        if (!serviceNamespaces.TryGetValue(name, out var value) || value is not T typed)
            throw new KeyNotFoundException($"Extension namespace '{name}' is not registered as {typeof(T).Name}.");
        return typed;
    }

    /// <summary>Construction is synchronous and returns only after the readiness
    /// round trip and extension initialization. This never switches owner threads.</summary>
    public ValueTask WaitReady()
    {
        RequireOwner();
        if (!Ready) throw new InvalidOperationException("PGlite is not ready.");
        return ValueTask.CompletedTask;
    }
    public void Close() => Dispose();
    /// <summary>Completes synchronously on the owning thread. Use PgliteWorker for
    /// an asynchronous queue that preserves the engine's thread affinity.</summary>
    public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }

    partial void BeforeQuery()
    {
        if (serviceSnapshotting) throw new InvalidOperationException("Engine entry during a cluster snapshot is forbidden.");
        if (serviceDeliveringCallbacks) throw new InvalidOperationException("Query callbacks cannot synchronously reenter PGlite; queue the next operation after the callback.");
        if (serviceTransaction != null && !serviceTransactionEntry)
            throw new InvalidOperationException("Use the scoped transaction token while its callback is active.");
    }

    partial void OnStarted()
    {
        serviceInitializing = true;
        try
        {
            var initializers = new List<Action>();
            foreach (var extension in options.Extensions)
            {
                var setup = extension.Setup?.Invoke(this) ?? new PgliteExtensionSetup();
                serviceNamespaces.Add(extension.Name, setup.Namespace);
                if (setup.Close != null) serviceExtensionClosers.Add(setup.Close);
                if (setup.Initialize != null) initializers.Add(setup.Initialize);
            }
            foreach (var initialize in initializers) initialize();
        }
        finally { serviceInitializing = false; }
        DrainServiceResponses();
    }

    partial void OnClosing()
    {
        if (serviceInitializing || serviceClosing || serviceTransaction != null || serviceExclusiveDepth != 0 || serviceDeliveringCallbacks || serviceSnapshotting)
            throw new InvalidOperationException("Cannot close during a transaction, exclusive callback, service callback or snapshot.");
        serviceClosing = true;
        foreach (var close in serviceExtensionClosers)
        {
            try { close(); }
            catch (Exception error) { serviceCloseErrors.Add(error); }
        }
        serviceExtensionClosers.Clear();
        serviceLive?.CloseAll();
    }

    partial void OnClosed()
    {
        serviceResponses.Clear(); serviceListeners.Clear(); serviceGlobalListeners.Clear(); serviceNamespaces.Clear();
        try { options.Storage?.Detach(this); }
        catch (Exception error) { serviceCloseErrors.Add(error); }
        if (serviceCloseErrors.Count != 0)
            throw new AggregateException("The engine closed, but managed extension/storage cleanup failed.", serviceCloseErrors);
    }

    partial void OnResponse(PgliteQueryResult result)
    {
        if (result.HasReadyForQuery) serviceTransactionStatus = result.TransactionStatus;
        if (result.Notices.Count != 0 || result.Notifications.Count != 0)
            serviceResponses.Enqueue((result.Notices, result.Notifications));
        if (!serviceRefreshing && result.HasReadyForQuery && result.IsSuccess && result.TransactionStatus == PgliteTransactionStatus.Idle)
            serviceNeedsRefresh = true;
        DrainServiceResponses();
    }

    private void DrainServiceResponses()
    {
        if (serviceBatchDepth != 0 || serviceTransaction != null || serviceDeliveringCallbacks ||
            serviceRefreshing || serviceInitializing || serviceClosing) return;
        var errors = new List<Exception>();
        if (serviceNeedsRefresh)
        {
            serviceNeedsRefresh = false;
            serviceRefreshing = true;
            try { serviceLive?.RefreshAll(errors); }
            finally { serviceRefreshing = false; }
        }
        serviceDeliveringCallbacks = true;
        try
        {
            while (serviceResponses.TryDequeue(out var response))
            {
                foreach (var notice in response.Notices)
                {
                    if (options.OnNotice != null) TryServiceCallback(() => options.OnNotice(notice), errors);
                    if (OnNotice != null)
                        foreach (Action<PgliteError> callback in OnNotice.GetInvocationList())
                            TryServiceCallback(() => callback(notice), errors);
                }
                foreach (var notification in response.Notifications)
                {
                    if (serviceListeners.TryGetValue(notification.Channel, out var callbacks))
                        foreach (var callback in callbacks.ToArray())
                            TryServiceCallback(() => callback(notification.Payload), errors);
                    foreach (var callback in serviceGlobalListeners.ToArray())
                        TryServiceCallback(() => callback(notification.Channel, notification.Payload), errors);
                }
            }
        }
        finally { serviceDeliveringCallbacks = false; }
        if (errors.Count != 0) throw new AggregateException("A managed PGlite callback failed after engine execution completed.", errors);
    }

    internal void DeliverLiveCallback(Action callback, List<Exception> errors)
    {
        bool previous = serviceDeliveringCallbacks;
        serviceDeliveringCallbacks = true;
        try { TryServiceCallback(callback, errors); }
        finally { serviceDeliveringCallbacks = previous; }
    }
    private static void TryServiceCallback(Action callback, List<Exception> errors)
    { try { callback(); } catch (Exception error) { errors.Add(error); } }

    public T RunExclusive<T>(Func<T> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        RejectAsyncServiceCallback<T>(callback);
        RequireOwner(); BeforeQuery();
        if (serviceExclusiveDepth != 0) throw new InvalidOperationException("An exclusive callback is already active.");
        serviceExclusiveDepth++;
        serviceBatchDepth++;
        T result;
        try { result = callback(); }
        finally { serviceExclusiveDepth--; serviceBatchDepth--; }
        DrainServiceResponses();
        return result;
    }
    public void RunExclusive(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        RejectAsyncVoidServiceCallback(callback);
        RunExclusive(() => { callback(); return 0; });
    }
    private static void RejectAsyncVoidServiceCallback(Delegate callback)
    {
        if (callback.Method.IsDefined(typeof(AsyncStateMachineAttribute), inherit: false))
            throw new ArgumentException("The owner-thread callback must be synchronous. Use PgliteWorker to queue asynchronous callers.");
    }
    private static void RejectAsyncServiceCallback<T>(Delegate callback)
    {
        RejectAsyncVoidServiceCallback(callback);
        Type type = typeof(T);
        if (typeof(Task).IsAssignableFrom(type) || type == typeof(ValueTask) ||
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            throw new ArgumentException("The owner-thread callback cannot return a Task or ValueTask.");
    }

    public PgliteSubscription Listen(string channel, Action<string> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        RequireOwner(); BeforeQuery();
        string normalized = NormalizeChannel(channel);
        if (!serviceListeners.TryGetValue(normalized, out var callbacks)) serviceListeners.Add(normalized, callbacks = []);
        bool added = callbacks.Add(callback);
        try { Query("LISTEN " + channel).EnsureSuccess(); }
        catch
        {
            if (added) callbacks.Remove(callback);
            if (callbacks.Count == 0) serviceListeners.Remove(normalized);
            throw;
        }
        return new(() => Unlisten(QuoteServiceIdentifier(normalized), callback));
    }

    public void Unlisten(string channel, Action<string>? callback = null)
    {
        RequireOwner(); BeforeQuery();
        string normalized = NormalizeChannel(channel);
        if (!serviceListeners.TryGetValue(normalized, out var callbacks))
        {
            if (callback == null) Query("UNLISTEN " + channel).EnsureSuccess();
            return;
        }
        if (callback == null || callbacks.Count == 1 && callbacks.Contains(callback))
        {
            Query("UNLISTEN " + channel).EnsureSuccess();
            serviceListeners.Remove(normalized);
        }
        else callbacks.Remove(callback);
    }
    public PgliteSubscription OnNotification(Action<string, string> callback)
    {
        ArgumentNullException.ThrowIfNull(callback); RequireOwner(); BeforeQuery();
        serviceGlobalListeners.Add(callback);
        return new(() => OffNotification(callback));
    }
    public void OffNotification(Action<string, string> callback)
    { ArgumentNullException.ThrowIfNull(callback); RequireOwner(); BeforeQuery(); serviceGlobalListeners.Remove(callback); }

    private static string QuoteServiceIdentifier(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";
    private static string NormalizeChannel(string channel)
    {
        ArgumentException.ThrowIfNullOrEmpty(channel);
        string name;
        if (channel[0] == '"')
        {
            var decoded = new StringBuilder();
            bool closed = false;
            for (int i = 1; i < channel.Length; i++)
            {
                char c = channel[i];
                if (c != '"') { decoded.Append(c); continue; }
                if (i + 1 < channel.Length && channel[i + 1] == '"') { decoded.Append('"'); i++; continue; }
                if (i != channel.Length - 1) throw new ArgumentException("Expected one PostgreSQL channel identifier.", nameof(channel));
                closed = true;
            }
            if (!closed) throw new ArgumentException("Unterminated channel identifier.", nameof(channel));
            name = decoded.ToString();
        }
        else
        {
            var folded = new StringBuilder();
            for (int i = 0; i < channel.Length; i++)
            {
                char c = channel[i];
                if (!(c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_' || c >= 128 || i > 0 && (c is >= '0' and <= '9' or '$')))
                    throw new ArgumentException("Expected one PostgreSQL channel identifier; quote special characters.", nameof(channel));
                folded.Append(c is >= 'A' and <= 'Z' ? (char)(c + 32) : c);
            }
            name = folded.ToString();
        }
        if (name.Length == 0 || name.Contains('\0')) throw new ArgumentException("Invalid channel identifier.", nameof(channel));
        // PostgreSQL's NAMEDATALEN is 64 in the selected engine. Truncate only at UTF-8 boundaries.
        var truncated = new StringBuilder(); int bytes = 0;
        foreach (var rune in name.EnumerateRunes())
        { if (bytes + rune.Utf8SequenceLength > 63) break; truncated.Append(rune); bytes += rune.Utf8SequenceLength; }
        return truncated.ToString();
    }
}

/// <summary>Dispose on the database owner thread, or through its worker queue.</summary>
public sealed class PgliteSubscription : IDisposable
{
    private Action? unsubscribe;
    internal PgliteSubscription(Action unsubscribe) { this.unsubscribe = unsubscribe; }
    public void Dispose()
    {
        var action = unsubscribe;
        if (action == null) return;
        action();
        unsubscribe = null;
    }
}
