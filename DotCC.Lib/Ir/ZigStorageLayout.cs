using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotCC.Ir;

/// <summary>Zig source semantics, separate from C packed records and CLR value tokens.</summary>
public enum ZigStorageKind { PackedInteger, Zero, Unsupported }

public sealed record ZigStorageField(string Name, CType SourceType, string TargetName,
    int BitOffset, int LogicalBits, bool Signed, bool HasDefault, bool HasSentinel);

public sealed record ZigStorageLayout(ZigStorageKind Kind, CType.Named SourceCType, string SourceIdentity,
    string ProofOrigin, string BackingKind, bool BackingSigned, int LogicalBits, int NativeSize,
    int NativeAlignment, string Profile, IReadOnlyList<ZigStorageField> SourceMembers, string? UnsupportedReason = null)
{
    public int Stride => NativeSize;
    // This is a measured finite target profile, not a general Zig ABI rule.
    public const string AcceptedProfile = "zig-0.17-linux-x86_64-le-dotnet10-integer1to64-r49-v1";
    public static bool CompilerModelHostMatches => OperatingSystem.IsLinux()
        && RuntimeInformation.ProcessArchitecture == Architecture.X64
        && BitConverter.IsLittleEndian && IntPtr.Size == 8 && Environment.Version.Major == 10;

    public void RequireSupported()
    {
        if (Profile != AcceptedProfile || !CompilerModelHostMatches)
            throw new IrUnsupportedException("zig storage: unsupported target profile; requires " + AcceptedProfile);
        if (Kind == ZigStorageKind.Unsupported)
            throw new IrUnsupportedException("zig storage '" + SourceIdentity + "': " + UnsupportedReason);
    }
}

/// <summary>Finite R47 native proof for { u64, ZigZero, u8 }; no claim about general auto-layout.</summary>
public sealed record ZigZeroOwnerLayout(CType.Named SourceCType, IReadOnlyList<StructField> SourceMembers,
    string Profile, int NativeSize = 16, int NativeAlignment = 8)
{
    public IReadOnlyList<int> NativeOffsets => new[] { 0, 8, 8 };
}
