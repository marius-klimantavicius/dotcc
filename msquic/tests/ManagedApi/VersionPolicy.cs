using Managed.Transport.Api;
using Managed.Transport;

/// <summary>Consumes the authored facade and actual translated APIs before any
/// ConnectionStart/ListenerStart call. No network or substituted API callbacks.</summary>
internal static class VersionPolicy
{
    internal static unsafe void CheckGlobal(QuicRuntime runtime)
        => Check(runtime.Api, null, MsQuic.QUIC_PARAM_GLOBAL_VERSION_SETTINGS, "global");

    internal static unsafe void CheckConfiguration(QuicRuntime runtime, QuicConfiguration configuration)
    {
        using var lease = configuration.AcquireLease();
        Check(runtime.Api, configuration.Handle, MsQuic.QUIC_PARAM_CONFIGURATION_VERSION_SETTINGS, "configuration");
    }

    internal static async Task CheckConnectionAsync(QuicRuntime runtime, QuicRegistration registration)
    {
        nint connection = OpenConnection(runtime, registration);
        try { CheckConnection(runtime, connection); }
        finally { await runtime.RunCleanup(() => CloseConnection(runtime, connection)).ConfigureAwait(false); }
    }

    private static unsafe nint OpenConnection(QuicRuntime runtime, QuicRegistration registration)
    {
        QUIC_HANDLE* connection = null;
        QuicError.ThrowIfFailed(runtime.Api->ConnectionOpen(registration.ConnectionParentHandle,
            &ConnectionCallback, null, &connection), "version control ConnectionOpen");
        if (connection == null) throw new InvalidOperationException("ConnectionOpen returned no handle.");
        return (nint)connection;
    }

    private static unsafe uint ConnectionCallback(QUIC_HANDLE* connection, void* context, QUIC_CONNECTION_EVENT* evt)
    {
        // Unstarted connection closure may deliver shutdown bookkeeping. An
        // actual protocol/data event here indicates unintended transport work.
        return evt->Type is QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE or
            QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT ? 0u : 95u;
    }

    private static unsafe void CheckConnection(QuicRuntime runtime, nint connection)
        => Check(runtime.Api, (QUIC_HANDLE*)connection, MsQuic.QUIC_PARAM_CONN_VERSION_SETTINGS, "unstarted connection");
    private static unsafe void CloseConnection(QuicRuntime runtime, nint connection)
        => runtime.Api->ConnectionClose((QUIC_HANDLE*)connection);

    private static unsafe void Check(QUIC_API_TABLE* api, QUIC_HANDLE* handle, uint parameter, string scope)
    {
        uint length = 0;
        uint status = api->GetParam(handle, parameter, &length, null);
        if (status != 75 || length != sizeof(QUIC_VERSION_SETTINGS) + 3 * sizeof(uint))
            throw new InvalidOperationException($"{scope} version size: status={status}, length={length}");
        byte* storage = stackalloc byte[checked((int)length)];
        QuicError.ThrowIfFailed(api->GetParam(handle, parameter, &length, storage), scope + " version settings");
        var versions = (QUIC_VERSION_SETTINGS*)storage;
        // Pinned upstream setters take host-order values, while this getter
        // copies the internal network-order arrays (settings.c:2191).
        if (versions->AcceptableVersionsLength != 1 || versions->OfferedVersionsLength != 1 ||
            versions->FullyDeployedVersionsLength != 1 ||
            versions->AcceptableVersions != (uint*)(versions + 1) ||
            versions->OfferedVersions != versions->AcceptableVersions + 1 ||
            versions->FullyDeployedVersions != versions->OfferedVersions + 1 ||
            versions->AcceptableVersions[0] != MsQuic.QUIC_VERSION_1 ||
            versions->OfferedVersions[0] != MsQuic.QUIC_VERSION_1 ||
            versions->FullyDeployedVersions[0] != MsQuic.QUIC_VERSION_1)
            throw new InvalidOperationException(scope + " did not retain exactly QUIC v1 in every effective list.");
    }
}
