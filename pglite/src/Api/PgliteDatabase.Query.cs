// Manual API port of @electric-sql/pglite base.ts/pglite.ts (Apache-2.0).
using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.ExceptionServices;
using Backend = Managed.PGlite.Translated.PGlite;

namespace Managed.PGlite;

public sealed unsafe partial class PgliteDatabase
{
    private byte[] pendingFrontend = [];
    // These describe the wire exchange, independently of the actual engine's
    // transaction state. They never replace TransactionBlockStatusCode().
    private bool protocolExchangePending, protocolDiscardUntilSync;
    private readonly PgliteProtocolDecoder protocolDecoder = new();
    private readonly PgliteCodecs sessionCodecs;

    /// <summary>The lifetime's independent, reusable value codec registry.</summary>
    public PgliteCodecs Codecs { get { RequireOwner(); return sessionCodecs; } }

    partial void BeforeQuery();
    partial void OnResponse(PgliteQueryResult result);
    partial void OnStarted();
    partial void OnClosing();
    partial void OnClosed();

    private void RequireQueryEntry(bool allowFragment = false)
    {
        RequireOwner(allowProtocolExchange: allowFragment);
        if (!ready) throw new InvalidOperationException("PGlite is not ready.");
        BeforeQuery();
    }

    private PgliteCodecs QueryCodecs(PgliteQueryOptions? queryOptions)
        => queryOptions?.Codecs ?? sessionCodecs;

    /// <summary>Discover arrays created since startup, including arrays of
    /// custom enum types. Element codec registrations remain effective.</summary>
    public void RefreshArrayTypes()
    {
        RequireQueryEntry();
        var result = RefreshArrayTypesCore();
        OnResponse(result);
    }

    private PgliteQueryResult RefreshArrayTypesCore()
    {
        var result = QueryCore("SELECT oid, typarray, typdelim FROM pg_catalog.pg_type WHERE typarray <> 0").EnsureSuccess();
        foreach (var row in result.ResultSets.Single().Rows)
        {
            uint element = uint.Parse(row[0]!, CultureInfo.InvariantCulture);
            uint array = uint.Parse(row[1]!, CultureInfo.InvariantCulture);
            sessionCodecs.RegisterArray(array, element, row[2]![0]);
        }
        return result;
    }

    /// <summary>Execute one statement using Parse, Describe, Bind, Execute and
    /// Sync. Server-described parameter OIDs select encoders. SQL errors are
    /// returned, and a real Sync completes error recovery before returning.</summary>
    public PgliteQueryResult Query(string sql, IReadOnlyList<object?> parameters, PgliteQueryOptions? options = null)
    {
        RequireQueryEntry();
        ArgumentNullException.ThrowIfNull(parameters);
        object?[] values = parameters.ToArray();
        IReadOnlyList<uint>? types = ParameterTypes(values, options?.ParameterTypes);
        byte[] parse = PgliteProtocol.Concat(PgliteProtocol.EncodeParse(sql, types), PgliteProtocol.EncodeDescribe());
        PgliteCodecs codecs = QueryCodecs(options);
        var responses = new List<byte[]>();
        Exception? failure = null;
        try
        {
            byte[] descriptionData = ExecuteProtocolCore(parse).Data;
            responses.Add(descriptionData);
            var description = new PgliteProtocolDecoder().Decode(descriptionData);
            if (description.IsSuccess)
            {
                var encoded = new string?[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    object? value = values[i] is PgliteParameter parameter ? parameter.Value : values[i];
                    uint oid = i < description.ParameterTypes.Count ? description.ParameterTypes[i] : 0;
                    encoded[i] = codecs.Encode(value, oid);
                }
                responses.Add(ExecuteProtocolCore(PgliteProtocol.Concat(PgliteProtocol.EncodeBind(encoded),
                    PgliteProtocol.EncodeDescribe('P'), PgliteProtocol.EncodeExecute())).Data);
            }
        }
        catch (Exception error) when (!faulted) { failure = error; }
        finally
        {
            // The engine, not the managed wrapper, controls ignore_till_sync
            // and transaction readiness, including after parameter conversion.
            if (!faulted) responses.Add(ExecuteProtocolCore(PgliteProtocol.EncodeSync()).Data);
        }
        var result = PgliteProtocol.DecodeResponse(Join(responses), codecs.Decode);
        OnQueryResponse(result, options);
        if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
        return result;
    }

    public PgliteQueryResult Sql(PgliteSql sql, PgliteQueryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(sql);
        return Query(sql.Text, sql.Parameters, options);
    }

    /// <summary>Execute one or more statements using Simple Query. Unlike the
    /// backward-compatible Query(string), SQL errors throw PgliteSqlException.</summary>
    public PgliteQueryResult Exec(string sql, PgliteQueryOptions? options = null)
    {
        RequireQueryEntry();
        byte[] data = ExecuteProtocolCore(PgliteProtocol.EncodeQuery(sql)).Data;
        var result = PgliteProtocol.DecodeResponse(data, QueryCodecs(options).Decode);
        OnQueryResponse(result, options);
        return result.EnsureSuccess();
    }

    public PgliteQueryDescription DescribeQuery(string sql, IReadOnlyList<uint>? parameterTypes = null)
    {
        RequireQueryEntry();
        byte[] request = PgliteProtocol.Concat(PgliteProtocol.EncodeParse(sql, parameterTypes),
            PgliteProtocol.EncodeDescribe(), PgliteProtocol.EncodeSync());
        var result = PgliteProtocol.DecodeResponse(ExecuteProtocolCore(request).Data);
        OnResponse(result);
        result.EnsureSuccess();
        return new(result.ParameterTypes, result.DescribedColumns);
    }

    /// <summary>Execute PostgreSQL frontend frames, bypassing decoded callbacks.
    /// Complete frames are processed immediately; an incomplete final frame is
    /// retained until a later call. Extended exchanges stay open until Sync.
    /// Terminate belongs to Dispose. COPY FROM must bundle its query/Execute,
    /// CopyData and CopyDone/CopyFail frames in one call, because core entry is
    /// synchronous and cannot suspend inside a COPY command.</summary>
    public byte[] ExecProtocolRaw(byte[] message)
    {
        RequireQueryEntry(allowFragment: true);
        return ExecuteProtocolCore(StageFrontend(message)).Data;
    }

    /// <summary>Decode an exchange, including partial extended responses. Row
    /// descriptions persist across calls. SQL errors throw by default; a raw
    /// extended error still requires an explicit Sync from the caller.</summary>
    public PgliteQueryResult ExecProtocol(byte[] message, PgliteQueryOptions? options = null, bool throwOnError = true)
    {
        RequireQueryEntry(allowFragment: true);
        var result = DecodeExchange(message, options);
        OnQueryResponse(result, options);
        return throwOnError ? result.EnsureSuccess() : result;
    }

    /// <summary>Deliver owned copies of backend write chunks, in order, after
    /// translated entry has finished. Exceptions from callbacks leave the core
    /// lifetime intact. This method bypasses decoded response events.</summary>
    public void ExecProtocolRawStream(byte[] message, Action<ReadOnlyMemory<byte>> onRawData)
    {
        RequireQueryEntry(allowFragment: true);
        ArgumentNullException.ThrowIfNull(onRawData);
        var exchange = ExecuteProtocolCore(StageFrontend(message));
        foreach (byte[] chunk in exchange.Chunks) onRawData(chunk);
    }

    /// <summary>Deliver decoded messages in wire order, after translated entry
    /// has finished. Messages and their buffers remain valid after later calls.</summary>
    public PgliteQueryResult ExecProtocolStream(byte[] message, Action<PgliteBackendMessage> onMessage,
        PgliteQueryOptions? options = null, bool throwOnError = true)
    {
        RequireQueryEntry(allowFragment: true);
        ArgumentNullException.ThrowIfNull(onMessage);
        var result = DecodeExchange(message, options);
        OnQueryResponse(result, options);
        foreach (var response in result.Messages) onMessage(response);
        return throwOnError ? result.EnsureSuccess() : result;
    }

    private PgliteQueryResult DecodeExchange(byte[] message, PgliteQueryOptions? queryOptions)
    {
        byte[] data = ExecuteProtocolCore(StageFrontend(message)).Data;
        return protocolDecoder.Decode(data, QueryCodecs(queryOptions).Decode);
    }

    private static IReadOnlyList<uint>? ParameterTypes(object?[] values, IReadOnlyList<uint>? supplied)
    {
        if (!values.Any(value => value is PgliteParameter)) return supplied;
        var types = new uint[Math.Max(values.Length, supplied?.Count ?? 0)];
        if (supplied != null) for (int i = 0; i < supplied.Count; i++) types[i] = supplied[i];
        for (int i = 0; i < values.Length; i++)
            if (values[i] is PgliteParameter parameter)
            {
                if (types[i] != 0 && parameter.TypeOid != 0 && types[i] != parameter.TypeOid)
                    throw new ArgumentException("A parameter's type OID conflicts with ParameterTypes.", nameof(values));
                if (parameter.TypeOid != 0) types[i] = parameter.TypeOid;
            }
        return types;
    }

    private byte[] StageFrontend(byte[] message)
    {
        ArgumentNullException.ThrowIfNull(message);
        byte[] input = new byte[checked(pendingFrontend.Length + message.Length)];
        pendingFrontend.CopyTo(input, 0); message.CopyTo(input, pendingFrontend.Length);
        int offset = 0;
        while (input.Length - offset >= 5)
        {
            char type = (char)input[offset];
            if (type == 'X') throw new ArgumentException("Use Dispose to terminate the embedded backend.", nameof(message));
            if (type is not ('Q' or 'P' or 'B' or 'D' or 'E' or 'C' or 'H' or 'S' or 'd' or 'c' or 'f' or 'F'))
                throw new ArgumentException($"Unsupported frontend message {type}.", nameof(message));
            int length = BinaryPrimitives.ReadInt32BigEndian(input.AsSpan(offset + 1));
            if (length < 4 || length == int.MaxValue) throw new ArgumentException("Invalid frontend message length.", nameof(message));
            if (length > input.Length - offset - 1) break;
            offset += length + 1;
        }
        byte[] complete = input.AsSpan(0, offset).ToArray();
        pendingFrontend = input.AsSpan(offset).ToArray();
        return complete;
    }

    private sealed record ProtocolExchange(byte[] Data, IReadOnlyList<byte[]> Chunks);

    private ProtocolExchange ExecuteProtocolCore(byte[] message)
    {
        if (message.Length == 0) return new([], Array.Empty<byte[]>());
        transport.Begin(message);
        try
        {
            long buffered = BufferedInput();
            int consumed = 0;
            // A read can prefetch several frontend messages into PostgreSQL's
            // own buffer. Both byte sources must be drained, including after
            // SQL errors, which are recovered by PgliteOwner.Dispatch.
            while (!transport.InputConsumed || buffered > 0)
            {
                int previousOffset = transport.InputOffset;
                long previousBuffered = buffered;
                int status = Dispatch(Backend.dpg_step);
                if (status is not (0 or 100)) throw new IOException($"Unexpected protocol yield {status}.");
                buffered = BufferedInput();
                int nextConsumed = checked(transport.InputOffset - (int)buffered);
                ObserveProtocolInput(message.AsSpan(consumed, nextConsumed - consumed), status);
                consumed = nextConsumed;
                if (transport.InputOffset == previousOffset && buffered >= previousBuffered)
                    throw new IOException("Original backend made no progress consuming frontend frames.");
            }
            // This upstream helper checks send_ready_for_query. It does not
            // force readiness for Parse/Bind/Execute; only a real Sync does so.
            int flush = Dispatch(() => { Backend.dpg_ready(); Backend.dpg_flush(); });
            if (flush != 0) throw new IOException($"Unexpected ready/flush yield {flush}.");
            if (!transport.InputConsumed || BufferedInput() != 0)
                throw new IOException("Original backend left unconsumed frontend bytes.");
            return new(transport.Response.ToArray(), transport.Chunks.ToArray());
        }
        catch (Exception error) { lifetimeError ??= error; faulted = true; ready = false; throw; }
        finally { transport.ReleaseRequest(); }
    }

    private void ObserveProtocolInput(ReadOnlySpan<byte> input, int status)
    {
        char command = input.IsEmpty ? '\0' : (char)input[0];
        while (!input.IsEmpty)
        {
            char type = (char)input[0];
            int length = checked(BinaryPrimitives.ReadInt32BigEndian(input[1..]) + 1);
            input = input[length..];
            if (type == 'S')
            {
                protocolExchangePending = false;
                protocolDiscardUntilSync = false;
            }
            else if (!protocolDiscardUntilSync)
            {
                if (type is 'P' or 'B' or 'D' or 'E' or 'C') protocolExchangePending = true;
                else if (type is 'Q' or 'F') protocolExchangePending = false;
            }
        }
        // PGliteOwner has already performed the original error recovery. Track
        // its skip state only to reject high-level calls; all raw frames still
        // reach PostgreSQL, which implements discard-until-Sync itself.
        if (status == 100 && command is 'P' or 'B' or 'D' or 'E' or 'C' or 'H')
            protocolExchangePending = protocolDiscardUntilSync = true;
    }

    private long BufferedInput()
    {
        long buffered = -1;
        if (Dispatch(() => buffered = Backend.dpg_buffered_input()) != 0 || buffered < 0)
            throw new IOException("Cannot inspect original backend input buffer.");
        return buffered;
    }

    private static byte[] Join(List<byte[]> responses) => PgliteProtocol.Concat(responses.ToArray());
}
