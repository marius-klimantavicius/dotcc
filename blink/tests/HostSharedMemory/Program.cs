using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;

byte[] seed = new byte[6000];
for (int i = 0; i < seed.Length; ++i) seed[i] = unchecked((byte)(i * 13 + 7));
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { ["/seed.bin"] = seed });
bool workersJoined = true;
try
{
    if (Blink.SharedMemoryLegacy() != 0) throw new InvalidOperationException("Legacy memory probe failed.");
    var opened = io.OpenFile("/seed.bin", FileAccessMode.Read);
    if (!opened.Succeeded) throw new InvalidOperationException("Open private seed failed.");
    int fd = opened.Value;
    var seek = io.Seek(fd, 17, SeekOrigin.Begin);
    if (!seek.Succeeded || seek.Value != 17) throw new InvalidOperationException("Set private cursor failed.");
    for (int cycle = 0; cycle < 2; ++cycle)
    {
        if (Blink.SharedMemoryStart(fd) != 0) throw new InvalidOperationException("Shared owner setup failed.");
        Exception?[] failures = new Exception?[2];
        Thread[] workers = new Thread[2];
        workersJoined = false;
        for (int id = 0; id < 2; ++id)
        {
            int workerId = id;
            workers[id] = new Thread(() =>
            {
                Blink.BindHostIo(io);
                try
                {
                    if (Blink.SharedMemoryWorker(workerId) != 0)
                        throw new InvalidOperationException("Shared worker failed.");
                }
                catch (Exception error) { failures[workerId] = error; }
                finally { Blink.UnbindHostIo(); }
            });
            workers[id].Start();
        }
        foreach (Thread worker in workers)
            if (!worker.Join(TimeSpan.FromSeconds(20)))
                throw new TimeoutException("Shared memory worker did not finish; no concurrent forced disposal.");
        workersJoined = true;
        foreach (Exception? failure in failures)
            if (failure != null) throw new InvalidOperationException("Shared worker exception.", failure);
        if (Blink.SharedMemoryFinish(cycle) != 0) throw new InvalidOperationException("Shared owner cleanup failed.");
        seek = io.Seek(fd, 0, SeekOrigin.Current);
        if (!seek.Succeeded || seek.Value != 17) throw new InvalidOperationException("File mapping moved cursor.");
        GC.Collect(2, GCCollectionMode.Forced, true, true);
    }
    if (!io.Close(fd).Succeeded || io.OpenDescriptors != 3)
        throw new InvalidOperationException("Private file cleanup failed.");
    if (Blink.SharedMemoryTestEnd() != 0) throw new InvalidOperationException("Test synchronization cleanup failed.");
    Console.WriteLine("file_cursor=17 cleanup=joined-detached-destroyed");
}
finally { if (workersJoined) io.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
