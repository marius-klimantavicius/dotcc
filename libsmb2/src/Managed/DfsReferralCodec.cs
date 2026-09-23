using System.Buffers.Binary;
using System.Text;

namespace Managed.Smb;

internal static class DfsReferralCodec
{
    private const ushort NameListReferral = 0x0002;
    private static readonly Encoding Utf16 = new UnicodeEncoding(false, false, true);

    internal static byte[] EncodeRequest(string path)
    {
        string protocolPath = NormalizeProtocolPath(path);
        byte[] encoded = Utf16.GetBytes(protocolPath + '\0');
        byte[] request = new byte[2 + encoded.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(request, 4);
        encoded.CopyTo(request, 2);
        return request;
    }

    internal static DfsReferralResponse Parse(string requestedPath, ReadOnlySpan<byte> response)
    {
        string protocolPath = NormalizeProtocolPath(requestedPath);
        if (response.Length < 8) throw InvalidResponse("Referral header is truncated");
        ushort pathConsumedBytes = ReadUInt16(response, 0);
        ushort count = ReadUInt16(response, 2);
        uint flags = ReadUInt32(response, 4);
        byte[] requestedBytes = Utf16.GetBytes(protocolPath);
        if ((pathConsumedBytes & 1) != 0 || pathConsumedBytes > requestedBytes.Length)
            throw InvalidResponse("PathConsumed is outside the requested path");
        int consumed = pathConsumedBytes / 2;
        if (consumed > 0 && consumed < protocolPath.Length &&
            char.IsHighSurrogate(protocolPath[consumed - 1]) && char.IsLowSurrogate(protocolPath[consumed]))
            throw InvalidResponse("PathConsumed splits a Unicode surrogate pair");
        if (consumed < protocolPath.Length && protocolPath[consumed] != '\\')
            throw InvalidResponse("PathConsumed splits a path component");
        if (count == 0 || count > 1024) throw InvalidResponse("Referral count is invalid");

        // String offsets are relative to entries but must refer past the entire
        // entry table, not to another entry's metadata (MS-DFSC 2.2.5).
        int stringStart = 8;
        ushort responseVersion = ReadUInt16(response, stringStart);
        for (int i = 0; i < count; i++)
        {
            RequireRange(response, stringStart, 8);
            if (ReadUInt16(response, stringStart) != responseVersion)
                throw InvalidResponse("Referral entries disagree on version");
            ushort entrySize = ReadUInt16(response, stringStart + 2);
            if (entrySize < 8 || (entrySize & 1) != 0) throw InvalidResponse("Referral entry size is invalid");
            RequireRange(response, stringStart, entrySize);
            stringStart = checked(stringStart + entrySize);
        }
        var entries = new List<DfsReferralEntry>(count);
        int offset = 8;
        for (int i = 0; i < count; i++)
        {
            RequireRange(response, offset, 8);
            ushort version = ReadUInt16(response, offset);
            ushort size = ReadUInt16(response, offset + 2);
            ushort serverType = ReadUInt16(response, offset + 4);
            ushort entryFlags = ReadUInt16(response, offset + 6);
            if (size < 8 || (size & 1) != 0) throw InvalidResponse("Referral entry size is invalid");
            RequireRange(response, offset, size);
            if (version == 1)
            {
                string target = NormalizeProtocolPath(ReadString(response, offset + 8, offset + size));
                entries.Add(new DfsReferralEntry(version, serverType != 0, 300, null, target, false, false, []));
            }
            else if (version == 2)
            {
                if (size < 22) throw InvalidResponse("Version 2 referral is truncated");
                uint ttl = ReadUInt32(response, offset + 12);
                string dfsPath = ReadOffsetString(response, offset, ReadUInt16(response, offset + 16), Math.Max(22, stringStart - offset));
                string target = NormalizeProtocolPath(ReadOffsetString(response, offset, ReadUInt16(response, offset + 20), Math.Max(22, stringStart - offset)));
                entries.Add(new DfsReferralEntry(version, serverType != 0, ttl, dfsPath, target, false, false, []));
            }
            else if (version is 3 or 4)
            {
                if (size < 18) throw InvalidResponse("Version 3/4 referral is truncated");
                uint ttl = ReadUInt32(response, offset + 8);
                bool nameList = (entryFlags & NameListReferral) != 0;
                if (nameList)
                {
                    ushort expandedCount = ReadUInt16(response, offset + 14);
                    ushort expandedOffset = ReadUInt16(response, offset + 16);
                    var expanded = ReadStrings(response, offset, expandedOffset, expandedCount, Math.Max(18, stringStart - offset));
                    entries.Add(new DfsReferralEntry(version, serverType != 0, ttl,
                        ReadOffsetString(response, offset, ReadUInt16(response, offset + 12), Math.Max(18, stringStart - offset)),
                        null, false, true, expanded));
                }
                else
                {
                    if (size < 34) throw InvalidResponse("Version 3/4 target referral is truncated");
                    string dfsPath = ReadOffsetString(response, offset, ReadUInt16(response, offset + 12), Math.Max(34, stringStart - offset));
                    string target = NormalizeProtocolPath(ReadOffsetString(response, offset, ReadUInt16(response, offset + 16), Math.Max(34, stringStart - offset)));
                    entries.Add(new DfsReferralEntry(version, serverType != 0, ttl, dfsPath, target,
                        version == 4 && (entryFlags & 0x0004) != 0, false, []));
                }
            }
            else
            {
                throw InvalidResponse("Unsupported referral version " + version);
            }
            offset = checked(offset + size);
        }
        if (entries.Count > 1 && entries.Select(entry => entry.TimeToLive).Distinct().Skip(1).Any())
            throw InvalidResponse("Referral entries disagree on TTL");
        return new DfsReferralResponse(protocolPath, consumed, flags, entries);
    }

    internal static string NormalizeProtocolPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (path.Contains('\0')) throw new ArgumentException("DFS path cannot contain NUL", nameof(path));
        string normalized = path.Replace('/', '\\');
        while (normalized.StartsWith("\\\\", StringComparison.Ordinal)) normalized = normalized[1..];
        if (!normalized.StartsWith('\\')) normalized = "\\" + normalized;
        if (normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            throw new ArgumentException("DFS path must contain a server and share", nameof(path));
        string[] parts = normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Any(part => part is "." or "..")) throw new ArgumentException("DFS paths cannot contain dot components", nameof(path));
        return "\\" + string.Join('\\', parts);
    }

    private static List<string> ReadStrings(ReadOnlySpan<byte> response, int entryOffset,
        ushort relativeOffset, ushort count, int minimumOffset)
    {
        var values = new List<string>(count);
        int offset = CheckedStringOffset(response, entryOffset, relativeOffset, minimumOffset);
        for (int i = 0; i < count; i++)
        {
            string value = ReadString(response, offset, response.Length, out int bytesRead);
            values.Add(value);
            offset = checked(offset + bytesRead);
        }
        return values;
    }

    private static string ReadOffsetString(ReadOnlySpan<byte> response, int entryOffset,
        ushort relativeOffset, int minimumOffset)
        => ReadString(response, CheckedStringOffset(response, entryOffset, relativeOffset, minimumOffset), response.Length);

    private static int CheckedStringOffset(ReadOnlySpan<byte> response, int entryOffset,
        ushort relativeOffset, int minimumOffset)
    {
        if (relativeOffset < minimumOffset || (relativeOffset & 1) != 0)
            throw InvalidResponse("Referral string offset is invalid");
        int offset = checked(entryOffset + relativeOffset);
        RequireRange(response, offset, 2);
        return offset;
    }

    private static string ReadString(ReadOnlySpan<byte> response, int offset, int end)
        => ReadString(response, offset, end, out _);

    private static string ReadString(ReadOnlySpan<byte> response, int offset, int end, out int bytesRead)
    {
        if ((offset & 1) != 0 || end > response.Length || offset > end)
            throw InvalidResponse("Referral string range is invalid");
        int cursor = offset;
        while (cursor + 1 < end && (response[cursor] != 0 || response[cursor + 1] != 0)) cursor += 2;
        if (cursor + 1 >= end) throw InvalidResponse("Referral string is not terminated");
        bytesRead = cursor - offset + 2;
        try { return Utf16.GetString(response[offset..cursor]); }
        catch (DecoderFallbackException error) { throw InvalidResponse("Referral string is not valid UTF-16", error); }
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> value, int offset)
    {
        RequireRange(value, offset, 2);
        return BinaryPrimitives.ReadUInt16LittleEndian(value[offset..]);
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> value, int offset)
    {
        RequireRange(value, offset, 4);
        return BinaryPrimitives.ReadUInt32LittleEndian(value[offset..]);
    }

    private static void RequireRange(ReadOnlySpan<byte> value, int offset, int length)
    {
        if (offset < 0 || length < 0 || offset > value.Length - length)
            throw InvalidResponse("Referral response is truncated");
    }

    private static InvalidDataException InvalidResponse(string message, Exception? inner = null)
        => new("Invalid DFS referral response: " + message, inner);
}

internal sealed class DfsReferralResponse
{
    public string RequestedPath { get; }
    public int PathConsumedCharacters { get; }
    public uint Flags { get; }
    public IReadOnlyList<DfsReferralEntry> Entries { get; }
    public bool ReferralServers => (Flags & 1) != 0;
    public bool StorageServers => (Flags & 2) != 0;
    public bool TargetFailback => (Flags & 4) != 0;

    public DfsReferralResponse(string requestedPath, int pathConsumedCharacters, uint flags,
        IReadOnlyList<DfsReferralEntry> entries)
    {
        RequestedPath = requestedPath;
        PathConsumedCharacters = pathConsumedCharacters;
        Flags = flags;
        Entries = entries;
    }
}

internal sealed class DfsReferralEntry
{
    public ushort Version { get; }
    public bool IsRoot { get; }
    public uint TimeToLive { get; }
    public string? DfsPath { get; }
    public string? TargetPath { get; }
    public bool TargetSetBoundary { get; }
    public IReadOnlyList<string> ExpandedNames { get; }
    public bool IsNameList { get; }

    public DfsReferralEntry(ushort version, bool isRoot, uint timeToLive, string? dfsPath,
        string? targetPath, bool targetSetBoundary, bool isNameList, IReadOnlyList<string> expandedNames)
    {
        Version = version;
        IsRoot = isRoot;
        TimeToLive = timeToLive;
        DfsPath = dfsPath;
        TargetPath = targetPath;
        TargetSetBoundary = targetSetBoundary;
        IsNameList = isNameList;
        ExpandedNames = expandedNames;
    }
}
