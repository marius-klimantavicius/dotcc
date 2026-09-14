using System.Buffers.Binary;
using Managed.Interpreters;

internal static class ModuleProbes
{
    internal static bool Rejects(byte[] bytes)
    {
        using var engine = new PintaEngine(new Dictionary<string, byte[]> { ["invalid.pint"] = bytes },
            arenaBytes: 65536, heapBytes: 1024, stackBytes: 1024);
        try { engine.LoadModule("invalid.pint"); return false; }
        catch (InvalidOperationException) { return true; }
    }
    internal static bool TruncatedHeaders(byte[] receipt)
    {
        for (int length = 0; length < 84; length++)
            if (!Rejects(receipt.AsSpan(0, length).ToArray())) return false;
        return true;
    }
    internal static bool BadMagic(byte[] receipt)
    {
        var bytes = (byte[])receipt.Clone();
        Array.Clear(bytes, 0, 4);
        return Rejects(bytes);
    }
    internal static bool BadRecordBounds(byte[] receipt)
    {
        foreach (int index in new[] { 10, 11, 16, 17, 19, 20 })
        {
            var bytes = (byte[])receipt.Clone();
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4 * index, 4), uint.MaxValue);
            if (!Rejects(bytes)) return false;
        }
        return true;
    }
}
