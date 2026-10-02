using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Edg.Runtime;

unsafe class Probe
{
    const int Workers = 12, Rounds = 128;
    static int Bucket(nuint p) => (int)(((p >> 3) ^ (p >> 11)) & 255);
    static void Check(bool condition) { if (!condition) throw new Exception("atomic wait check failed"); }
    static void Contended(bool collide)
    {
        long* storage = (long*)NativeMemory.AllocZeroed(65536);
        try
        {
            var locations = new List<nint>();
            int selected = Bucket((nuint)storage);
            for (int i = 0; i < 8192 && locations.Count < Workers; ++i)
                if (!collide || Bucket((nuint)(storage + i)) == selected)
                    locations.Add((nint)(storage + i));
            Check(locations.Count == Workers);
            int acknowledged = 0;
            var threads = new Thread[Workers];
            using var ready = new CountdownEvent(Workers);
            for (int worker = 0; worker < Workers; ++worker)
            {
                nint location = locations[worker];
                threads[worker] = new Thread(() => {
                    ready.Signal();
                    for (long round = 1; round <= Rounds; ++round)
                    {
                        LibcxxAtomicWait.Wait64((void*)location, round - 1);
                        Check(Interlocked.Read(ref *(long*)location) == round);
                        Interlocked.Increment(ref acknowledged);
                    }
                });
                threads[worker].IsBackground = true;
                threads[worker].Start();
            }
            ready.Wait();
            for (long round = 1; round <= Rounds; ++round)
            {
                foreach (nint location in locations)
                {
                    Interlocked.Exchange(ref *(long*)location, round);
                    LibcxxAtomicWait.Wake64((void*)location);
                }
                Check(SpinWait.SpinUntil(() => Volatile.Read(ref acknowledged) == round * Workers, 10000));
            }
            foreach (Thread thread in threads) Check(thread.Join(10000));
        }
        finally { NativeMemory.Free(storage); }
    }
    static int Main()
    {
        void* table = LibcxxAtomicWait.Table();
        Check(((nuint)table & 63) == 0);
        for (int i = 0; i < 256 * 64; ++i) Check(((byte*)table)[i] == 0);
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        Check(table == LibcxxAtomicWait.Table());
        long* storage = (long*)NativeMemory.AllocZeroed(8);
        try
        {
            // Notification before parking: observing a changed word must return.
            Interlocked.Exchange(ref *storage, 1);
            LibcxxAtomicWait.Wake64(storage);
            LibcxxAtomicWait.Wait64(storage, 0);
        }
        finally { NativeMemory.Free(storage); }
        Contended(false);
        Contended(true);
        Console.WriteLine("3072 per-address wait handshakes, colliding buckets and notify-before-wait passed");
        return 0;
    }
}
