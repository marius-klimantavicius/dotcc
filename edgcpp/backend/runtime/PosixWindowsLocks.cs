#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

// Windows has no atomic POSIX range-lock conversion. Translated POSIX processes
// therefore coordinate through an advisory registry, not through LockFileEx.
// Namespace: the current user's LocalApplicationData directory and a user-only,
// cross-session named mutex. Different users and native Windows SQLite do NOT
// participate in this protocol. Keep native-interoperability claims Unix-only.
internal static class PosixWindowsLocks
{
    private const uint Magic = 0x314c5045; // EPL1
    private const int MaximumRecords = 65536;
    private sealed class Owner { internal readonly Guid Id = Guid.NewGuid(); }
    private readonly record struct Record(int Pid, long Birth, Guid Owner, long Start, long End, short Type);
    private static readonly ConditionalWeakTable<object, Owner> Owners = new();
    private static readonly int ProcessId = Environment.ProcessId;
    private static readonly long ProcessBirth = CurrentBirth();
    private static long CurrentBirth() { using var process = Process.GetCurrentProcess(); return Birth(process); }
    private static long Birth(Process process)
    {
        // The provider is also exercised directly by cross-process Unix tests.
        // Linux Process.StartTime derives boot time with tiny rounding changes
        // between processes; the kernel's start-tick field is the stable token.
        if (OperatingSystem.IsLinux())
        {
            string stat = File.ReadAllText("/proc/" + process.Id + "/stat");
            int endName = stat.LastIndexOf(')');
            if (endName < 0) throw PosixPlatform.Failure(5);
            return long.Parse(stat.AsSpan(endName + 2).ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)[19], System.Globalization.CultureInfo.InvariantCulture);
        }
        return process.StartTime.ToUniversalTime().Ticks;
    }

    private sealed class Registry : IDisposable
    {
        internal readonly string Path;
        private readonly Mutex mutex;
        internal Registry(ulong device, ulong inode, ulong high)
        {
            string root = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Edg.Runtime", "posix-locks-v1");
            if (!System.IO.Path.IsPathFullyQualified(root)) throw PosixPlatform.Failure(95);
            Directory.CreateDirectory(root);
            Path = System.IO.Path.Combine(root, device.ToString("x16") + "-" + inode.ToString("x16") + "-" + high.ToString("x16") + ".locks");
            string mutexKey = OperatingSystem.IsWindows() ? Path.ToUpperInvariant() : Path;
            string name = "Edg.PosixLocks.v1." + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(mutexKey)));
            mutex = new Mutex(false, name, new NamedWaitHandleOptions { CurrentUserOnly = true, CurrentSessionOnly = false });
            try { mutex.WaitOne(); }
            catch (AbandonedMutexException) { /* The previous committed snapshot remains authoritative. */ }
            catch { mutex.Dispose(); throw; }
            try { File.Delete(Path + ".pending"); }
            catch { mutex.ReleaseMutex(); mutex.Dispose(); throw; }
        }
        public void Dispose() { mutex.ReleaseMutex(); mutex.Dispose(); }

        internal List<Record> Read()
        {
            var records = new List<Record>();
            FileStream stream;
            try { stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete); }
            catch (FileNotFoundException) { return records; }
            using (stream)
            using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
            {
                if (stream.Length < 8 || reader.ReadUInt32() != Magic) throw PosixPlatform.Failure(5);
                int count = reader.ReadInt32();
                if (count < 0 || count > MaximumRecords || stream.Length != 8L + count * 46L) throw PosixPlatform.Failure(5);
                records.EnsureCapacity(count);
                for (int i = 0; i < count; i++)
                {
                    int pid = reader.ReadInt32(); long birth = reader.ReadInt64();
                    byte[] id = reader.ReadBytes(16);
                    long start = reader.ReadInt64(), end = reader.ReadInt64(); short type = reader.ReadInt16();
                    if (pid <= 0 || birth <= 0 || id.Length != 16 || start < 0 || end <= start || type is not (0 or 1)) throw PosixPlatform.Failure(5);
                    records.Add(new(pid, birth, new Guid(id), start, end, type));
                }
            }
            return records;
        }
        internal void Write(List<Record> records)
        {
            if (records.Count > MaximumRecords) throw PosixPlatform.Failure(37);
            if (records.Count == 0) { File.Delete(Path); return; }
            // A killed writer can leave a temporary file, but cannot damage the
            // previous snapshot. The rename is the commit point under the mutex.
            // Locks do not need disk durability: a power failure kills all owners.
            string temporary = Path + ".pending";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    writer.Write(Magic); writer.Write(records.Count);
                    Span<byte> id = stackalloc byte[16];
                    foreach (var record in records)
                    {
                        writer.Write(record.Pid); writer.Write(record.Birth);
                        record.Owner.TryWriteBytes(id); writer.Write(id);
                        writer.Write(record.Start); writer.Write(record.End); writer.Write(record.Type);
                    }
                }
                File.Move(temporary, Path, true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }

    private static bool IsAlive(int pid, long birth)
    {
        if (pid == ProcessId) return birth == ProcessBirth;
        try { using var process = Process.GetProcessById(pid); return !process.HasExited && Birth(process) == birth; }
        catch (FileNotFoundException) { return false; }
        catch (DirectoryNotFoundException) { return false; }
        catch (ArgumentException) { return false; }
        catch (InvalidOperationException) { return false; }
        // A permission failure must never erase a potentially live owner's lock.
        catch (Win32Exception) { return true; }
    }
    private static bool Own(Record record, Guid owner) => record.Pid == ProcessId && record.Birth == ProcessBirth && record.Owner == owner;

    internal static void Control(object owner, SafeFileHandle handle, string path, int command, ref PosixFlock request, Func<bool>? closed)
    {
        Guid id = Owners.GetValue(owner, static _ => new Owner()).Id;
        var stat = PosixPlatform.LockIdentity(handle, path);
        long start = request.l_start, end = request.l_len == 0 ? long.MaxValue : checked(start + request.l_len);
        while (true)
        {
            using (var registry = new Registry(stat.Device, stat.Inode, stat.High))
            {
                if (handle.IsClosed || closed?.Invoke() == true) throw PosixPlatform.Failure(9);
                var records = registry.Read();
                Record? conflict = null;
                if (request.l_type != 2)
                    for (int i = 0; i < records.Count; i++)
                    {
                        var record = records[i];
                        if (Own(record, id) || record.Start >= end || record.End <= start || record.Type != 1 && request.l_type != 1) continue;
                        // Process liveness is expensive. Probe only a conflicting
                        // owner; unrelated snapshots require no process query.
                        if (!IsAlive(record.Pid, record.Birth)) { records.RemoveAt(i--); continue; }
                        conflict = record; break;
                    }
                if (records.Count >= MaximumRecords - 2) records.RemoveAll(static record => !IsAlive(record.Pid, record.Birth));
                if (command == 5)
                {
                    if (conflict is Record held) { request.l_type = held.Type; request.l_whence = 0; request.l_start = held.Start; request.l_len = held.End == long.MaxValue ? 0 : held.End - held.Start; request.l_pid = held.Pid; }
                    else request.l_type = 2;
                    return;
                }
                if (conflict is null)
                {
                    var next = new List<Record>(records.Count + 2);
                    foreach (var record in records)
                    {
                        if (!Own(record, id) || record.End <= start || record.Start >= end) { next.Add(record); continue; }
                        if (record.Start < start) next.Add(record with { End = start });
                        if (record.End > end) next.Add(record with { Start = end });
                    }
                    if (request.l_type != 2) next.Add(new(ProcessId, ProcessBirth, id, start, end, request.l_type));
                    next.Sort(static (a, b) => a.Start.CompareTo(b.Start));
                    for (int i = 0; i < next.Count; i++)
                        for (int j = i + 1; j < next.Count; j++)
                            if (next[i].Pid == next[j].Pid && next[i].Birth == next[j].Birth && next[i].Owner == next[j].Owner && next[i].Type == next[j].Type && next[i].End >= next[j].Start)
                            { next[i] = next[i] with { End = Math.Max(next[i].End, next[j].End) }; next.RemoveAt(j--); }
                    registry.Write(next);
                    return;
                }
                if (command == 6) throw PosixPlatform.Failure(11);
            }
            Thread.Sleep(10);
        }
    }

    internal static void Close(object owner, SafeFileHandle handle, string path)
    {
        try
        {
            if (!Owners.TryGetValue(owner, out var identity)) return;
            Guid id = identity.Id;
            var stat = PosixPlatform.LockIdentity(handle, path);
            using var registry = new Registry(stat.Device, stat.Inode, stat.High);
            var records = registry.Read();
            int removed = records.RemoveAll(record => Own(record, id));
            if (removed != 0) registry.Write(records);
        }
        finally { handle.Dispose(); }
    }
}
