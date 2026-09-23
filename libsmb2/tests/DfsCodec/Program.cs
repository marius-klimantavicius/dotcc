using System.Buffers.Binary;
using System.Text;
using Managed.Smb;

internal static class Program
{
    private static int Main()
    {
        byte[] request = DfsReferralCodec.EncodeRequest(@"\\domain.example\namespace\link\file.txt");
        Require(BinaryPrimitives.ReadUInt16LittleEndian(request) == 4, "request level");
        Require(Encoding.Unicode.GetString(request.AsSpan(2)).Equals(
            "\\domain.example\\namespace\\link\\file.txt\0", StringComparison.Ordinal), "request path");

        const string dfsPath = @"\domain.example\namespace\link";
        const string target1 = @"\server-a\share";
        const string target2 = @"\server-b\share";
        byte[] response = BuildV4Response(dfsPath, target1, target2);
        DfsReferralResponse parsed = DfsReferralCodec.Parse(
            @"\domain.example\namespace\link\folder\file.txt", response);
        Require(parsed.PathConsumedCharacters == dfsPath.Length, "path consumed");
        Require(parsed.StorageServers && parsed.TargetFailback && !parsed.ReferralServers, "header flags");
        Require(parsed.Entries.Count == 2, "entry count");
        Require(parsed.Entries[0].Version == 4 && parsed.Entries[0].TargetSetBoundary, "v4 boundary");
        Require(parsed.Entries[0].TimeToLive == 600, "ttl");
        Require(parsed.Entries[0].DfsPath == dfsPath && parsed.Entries[0].TargetPath == target1, "first target");
        Require(parsed.Entries[1].TargetPath == target2, "second target");

        Expect<InvalidDataException>(() => DfsReferralCodec.Parse(dfsPath, response[..7]), "short header");
        byte[] oddConsumed = response.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(oddConsumed, 1);
        Expect<InvalidDataException>(() => DfsReferralCodec.Parse(dfsPath, oddConsumed), "odd PathConsumed");
        byte[] badOffset = response.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(badOffset.AsSpan(8 + 16), ushort.MaxValue);
        Expect<InvalidDataException>(() => DfsReferralCodec.Parse(dfsPath, badOffset), "bad target offset");

        string unicode = @"\domain.example\namespace\資料😀";
        var unicodeReply = BuildV4Response(unicode, target1);
        Require(DfsReferralCodec.Parse(unicode + @"\file.txt", unicodeReply).PathConsumedCharacters == unicode.Length,
            "UTF-16 path consumption");
        var metadataOffset = response.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(metadataOffset.AsSpan(8 + 16), 34);
        Expect<InvalidDataException>(() => DfsReferralCodec.Parse(dfsPath, metadataOffset), "string in entry metadata");
        var mixedVersion = response.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(mixedVersion.AsSpan(8 + 34), 3);
        Expect<InvalidDataException>(() => DfsReferralCodec.Parse(dfsPath, mixedVersion), "mixed referral versions");
        Expect<ArgumentException>(() => DfsReferralCodec.EncodeRequest(@"\\server\share\..\file"), "dot component");

        Console.WriteLine("DfsCodec: PASS");
        return 0;
    }

    private static byte[] BuildV4Response(string dfsPath, params string[] targets)
    {
        byte[][] paths = targets.Select(target => Encoding.Unicode.GetBytes(dfsPath + "\0")).ToArray();
        byte[][] addresses = targets.Select(target => Encoding.Unicode.GetBytes(target + "\0")).ToArray();
        int entryBytes = targets.Length * 34;
        int strings = paths.Sum(value => value.Length) * 2 + addresses.Sum(value => value.Length);
        byte[] response = new byte[8 + entryBytes + strings];
        BinaryPrimitives.WriteUInt16LittleEndian(response, checked((ushort)(dfsPath.Length * 2)));
        BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(2), checked((ushort)targets.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(4), 0x00000006);
        int stringOffset = 8 + entryBytes;
        for (int i = 0; i < targets.Length; i++)
        {
            int entry = 8 + i * 34;
            BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(entry), 4);
            BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(entry + 2), 34);
            BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(entry + 6), i == 0 ? (ushort)4 : (ushort)0);
            BinaryPrimitives.WriteUInt32LittleEndian(response.AsSpan(entry + 8), 600);
            BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(entry + 12), checked((ushort)(stringOffset - entry)));
            paths[i].CopyTo(response, stringOffset);
            stringOffset += paths[i].Length;
            BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(entry + 14), checked((ushort)(stringOffset - entry)));
            paths[i].CopyTo(response, stringOffset);
            stringOffset += paths[i].Length;
            BinaryPrimitives.WriteUInt16LittleEndian(response.AsSpan(entry + 16), checked((ushort)(stringOffset - entry)));
            addresses[i].CopyTo(response, stringOffset);
            stringOffset += addresses[i].Length;
        }
        return response;
    }

    private static void Require(bool value, string name)
    {
        if (!value) throw new InvalidOperationException("Failed: " + name);
    }

    private static void Expect<T>(Action action, string name) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ": " + name);
    }
}
