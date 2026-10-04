using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using Edg.Runtime;

internal static unsafe class AtomicReadOnlyTest
{
    private static void Check<T>(byte* address, int order) where T : unmanaged
    {
        T expected = Unsafe.ReadUnaligned<T>(address);
        T observed = AtomicOps.Load(ref Unsafe.AsRef<T>(address), order);
        if (!new ReadOnlySpan<byte>(&expected, sizeof(T)).SequenceEqual(new ReadOnlySpan<byte>(&observed, sizeof(T))))
            throw new InvalidOperationException("typed atomic load changed bits");
        T raw = default;
        AtomicOps.LoadBytes((ulong)sizeof(T), address, &raw, order);
        if (!new ReadOnlySpan<byte>(&expected, sizeof(T)).SequenceEqual(new ReadOnlySpan<byte>(&raw, sizeof(T))))
            throw new InvalidOperationException("raw atomic load changed bits");
    }
    private static void CheckAll(byte* address, int order)
    {
        Check<byte>(address, order);
        Check<ushort>(address, order);
        Check<uint>(address, order);
        Check<ulong>(address, order);
        Check<UInt128>(address, order);
        Check<float>(address, order);
        Check<double>(address, order);
        byte* bytes = stackalloc byte[24];
        foreach (int size in new[] { 3, 7, 24 })
        {
            AtomicOps.LoadBytes((ulong)size, address, bytes, order);
            if (!new ReadOnlySpan<byte>(address, size).SequenceEqual(new ReadOnlySpan<byte>(bytes, size)))
                throw new InvalidOperationException("arbitrary-size atomic load changed bits");
        }
    }
    public static void Main(string[] args)
    {
        string path = args[0];
        byte[] initial = new byte[4096];
        for (int i = 256; i < initial.Length; ++i) initial[i] = unchecked((byte)(i * 37));
        File.WriteAllBytes(path, initial);
        using (var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read))
        using (var view = file.CreateViewAccessor(0, 4096, MemoryMappedFileAccess.Read))
        {
            byte* address = null;
            view.SafeMemoryMappedViewHandle.AcquirePointer(ref address);
            try
            {
                address += view.PointerOffset;
                foreach (int order in new[] { 0, 1, 2, 5 })
                {
                    for (int offset = 0; offset < 8; ++offset)
                    {
                        CheckAll(address + offset, order);
                        CheckAll(address + 256 + offset, order);
                    }
                }
            }
            finally { view.SafeMemoryMappedViewHandle.ReleasePointer(); }
        }
        if (!File.ReadAllBytes(path).AsSpan().SequenceEqual(initial))
            throw new InvalidOperationException("atomic load modified mapped file");
        File.Delete(path);
        Console.WriteLine("PASS: read-only mapped atomic loads, all legal load orders, aligned/unaligned, zero/nonzero, typed/raw");
    }
}
