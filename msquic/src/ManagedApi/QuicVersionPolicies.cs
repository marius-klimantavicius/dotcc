using System.Buffers.Binary;
using Managed.Transport.Hosting;

namespace Managed.Transport.Api;

public sealed partial class QuicConfiguration
{
    /// <summary>Copies the actual effective configuration version lists into
    /// independent host-order storage. The selected profile contains only v1.</summary>
    public unsafe QuicVersionPolicy GetVersionPolicy(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        uint parameter = QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONFIGURATION_VERSION_SETTINGS, priority);
        using var operation = EnterOperation();
        lock (settingsGate) return ScopedQuicVersionPolicy.Read(Runtime.Api, Handle, parameter);
    }

    /// <summary>Replaces all effective version lists with exactly QUIC v1.
    /// Configuration parameters execute inline even when High is requested.</summary>
    public unsafe void SetVersionPolicy(QuicVersionPolicy policy, QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        ScopedQuicVersionPolicy.ValidateSnapshot(policy);
        uint parameter = QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONFIGURATION_VERSION_SETTINGS, priority);
        using var operation = EnterOperation();
        lock (settingsGate) QuicRuntime.SetVersionOne(Runtime.Api, Handle, parameter);
    }
}

public sealed partial class QuicConnection
{
    /// <summary>Copies the actual effective connection version lists into
    /// independent host-order storage. This is distinct from the negotiated version.</summary>
    public unsafe QuicVersionPolicy GetVersionPolicy(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        uint parameter = QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_VERSION_SETTINGS, priority);
        using var operation = EnterOperation();
        lock (parameterGate) return ScopedQuicVersionPolicy.Read(Runtime.Api, Handle, parameter);
    }

    /// <summary>Applies exactly-v1 lists through the actual connection parameter
    /// operation. Upstream determines the allowed connection state and status.</summary>
    public unsafe void SetVersionPolicy(QuicVersionPolicy policy, QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        ScopedQuicVersionPolicy.ValidateSnapshot(policy);
        uint parameter = QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_VERSION_SETTINGS, priority);
        using var operation = EnterOperation();
        lock (parameterGate) QuicRuntime.SetVersionOne(Runtime.Api, Handle, parameter);
    }
}

internal static unsafe class ScopedQuicVersionPolicy
{
    internal static void ValidateSnapshot(QuicVersionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        // Check lengths before allocating, then validate independent copies.
        // A caller mutation after this point cannot change the native request.
        if (policy.AcceptableVersions.Length != 1 || policy.OfferedVersions.Length != 1 || policy.FullyDeployedVersions.Length != 1)
            throw new NotSupportedException("Each effective version list must contain exactly QUIC v1.");
        uint[] acceptable = policy.AcceptableVersions.ToArray();
        uint[] offered = policy.OfferedVersions.ToArray();
        uint[] fully = policy.FullyDeployedVersions.ToArray();
        if (acceptable[0] != 1 || offered[0] != 1 || fully[0] != 1)
            throw new NotSupportedException("Each effective version list must contain exactly QUIC v1.");
    }

    internal static QuicVersionPolicy Read(QUIC_API_TABLE* api, QUIC_HANDLE* handle, uint parameter)
    {
        uint length = 0;
        uint status = api->GetParam(handle, parameter, &length, null);
        if (status != Status.BufferTooSmall)
        {
            QuicError.ThrowIfFailed(status, "version policy size query");
            throw new InvalidOperationException("Version policy size query did not report a buffer extent.");
        }
        const uint MaximumListCount = 64;
        uint minimum = (uint)sizeof(QUIC_VERSION_SETTINGS);
        uint maximum = minimum + 3 * MaximumListCount * sizeof(uint);
        if (length < minimum || length > maximum)
            throw new InvalidOperationException("Version policy returned an invalid or unbounded extent.");
        byte[] bytes = new byte[checked((int)length)];
        fixed (byte* buffer = bytes)
        {
            QuicError.ThrowIfFailed(api->GetParam(handle, parameter, &length, buffer), "version policy query");
            if (length < minimum || length > bytes.Length)
                throw new InvalidOperationException("Version policy returned an inconsistent extent.");
            var value = (QUIC_VERSION_SETTINGS*)buffer;
            uint[] acceptable = Copy(value->AcceptableVersions, value->AcceptableVersionsLength, buffer, length);
            uint[] offered = Copy(value->OfferedVersions, value->OfferedVersionsLength, buffer, length);
            uint[] fully = Copy(value->FullyDeployedVersions, value->FullyDeployedVersionsLength, buffer, length);
            if (acceptable.Length != 1 || offered.Length != 1 || fully.Length != 1 ||
                acceptable[0] != 1 || offered[0] != 1 || fully[0] != 1)
                throw new InvalidOperationException("Effective version policy escaped the selected QUIC v1 profile.");
            return new(acceptable, offered, fully);
        }
    }

    private static uint[] Copy(uint* values, uint count, byte* buffer, uint length)
    {
        nuint address = (nuint)values, start = (nuint)buffer;
        if (count > 64 || address < start)
            throw new InvalidOperationException("Version list points outside its result buffer.");
        nuint offset = address - start;
        if (offset < (nuint)sizeof(QUIC_VERSION_SETTINGS) || offset > length ||
            (address & 3) != 0 || (ulong)count * sizeof(uint) > length - offset)
            throw new InvalidOperationException("Version list points outside its result buffer.");
        uint[] copy = new uint[checked((int)count)];
        // Pinned settings.c's getter copies network-order internal lists;
        // setters take host-order values. Never apply the negotiated-version
        // getter's different already-host-order convention to these arrays.
        for (int i = 0; i < copy.Length; ++i)
            copy[i] = BinaryPrimitives.ReadUInt32BigEndian(new ReadOnlySpan<byte>(values + i, sizeof(uint)));
        return copy;
    }
}
