using VM = Managed.Zstd.Native.Zstd;

namespace Managed.Zstd;

/// <summary>Immutable copied dictionary bytes. Both trained and raw-content dictionaries are accepted.
/// Contexts copy these bytes when loading them, so no managed address is retained.</summary>
public sealed unsafe class ZstdDictionary
{
    internal byte[] Bytes { get; }
    public int Length => Bytes.Length;
    public uint Id { get; }
    public ZstdDictionary(ReadOnlySpan<byte> bytes)
    {
        Bytes = bytes.ToArray();
        using var binding = ZstdRuntime.Enter();
        fixed (byte* data = Bytes) Id = VM.dz_dictionary_id(data, (ulong)Bytes.Length);
    }
    public byte[] ToArray() => Bytes.ToArray();
    /// <summary>Trains an upstream dictionary from independent sample records.</summary>
    public static ZstdDictionary Train(IReadOnlyList<ReadOnlyMemory<byte>> samples, int capacity = 8192)
    {
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        if (samples.Count == 0) throw new ArgumentException("At least one sample is required.", nameof(samples));
        int total = 0;
        var sizes = new ulong[samples.Count];
        for (int i = 0; i < samples.Count; i++) { total = checked(total + samples[i].Length); sizes[i] = (ulong)samples[i].Length; }
        var corpus = new byte[total];
        int position = 0;
        foreach (var sample in samples) { sample.Span.CopyTo(corpus.AsSpan(position)); position += sample.Length; }
        var dictionary = new byte[capacity];
        using var binding = ZstdRuntime.Enter();
        fixed (byte* source = corpus) fixed (byte* output = dictionary) fixed (ulong* lengths = sizes)
        {
            int written = checked((int)NativeResult.Check(VM.dz_train(output, (ulong)capacity, source, lengths, (uint)samples.Count)));
            return new ZstdDictionary(dictionary.AsSpan(0, written));
        }
    }
}
