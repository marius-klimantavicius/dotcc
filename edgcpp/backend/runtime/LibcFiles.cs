#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Edg.Runtime;

/// <summary>Owner-specific byte-oriented FILE services. Handles are opaque;
/// no native libc FILE object or process-wide mutable provider is used.</summary>
public sealed unsafe partial class LibcFiles : IDisposable
{
    private const int Eof = -1, Ebadf = 9, Enomem = 12, Einval = 22, Espipe = 29, Eoverflow = 75;
    private readonly Action<int> setErrno;
    private readonly object gate = new();
    private readonly Dictionary<nuint, Entry> files = new();
    // Addressable standard-stream globals are owner globals, so pinning is
    // appropriate here. FILE tokens and short-lived I/O buffers use NativeMemory.
    private readonly nint[] standard = GC.AllocateArray<nint>(3, pinned: true);
    private readonly string workingDirectory;
    private readonly Func<ulong> getEncoding;
    private bool disposed;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    private sealed class Entry
    {
        internal readonly object Gate = new();
        internal readonly Stream Stream;
        internal readonly bool Readable, Writable, Append, OwnStream;
        internal bool Closed, End, Error, Started;
        internal int Direction, Pushback = -1, BufferMode;
        internal int Orientation, WidePushback = -1, WidePushbackBytes;
        internal bool Utf8Encoding;
        internal uint DecodeState, DecodeValue;
        internal int ReadOffset, ReadCount, FailureEpoch;
        internal byte* Buffer;
        internal nuint Capacity = 8192, Used;
        internal bool OwnBuffer;
        internal Entry(Stream stream, bool readable, bool writable, bool append, bool ownStream, int mode = 0)
        { Stream = stream; Readable = readable; Writable = writable; Append = append; OwnStream = ownStream; BufferMode = mode; }
    }

    public LibcFiles(Action<int> setErrno, Stream? input = null, Stream? output = null, Stream? error = null,
        string? workingDirectory = null, Func<ulong>? getEncoding = null)
    {
        this.setErrno = setErrno;
        this.getEncoding = getEncoding ?? (() => 1UL);
        this.workingDirectory = Path.GetFullPath(workingDirectory ?? Directory.GetCurrentDirectory());
        try
        {
            standard[0] = (nint)Add(new Entry(input ?? Console.OpenStandardInput(), true, false, false, false));
            standard[1] = (nint)Add(new Entry(output ?? Console.OpenStandardOutput(), false, true, false, false,
                output is null && !Console.IsOutputRedirected ? 1 : 0));
            standard[2] = (nint)Add(new Entry(error ?? Console.OpenStandardError(), false, true, false, false, 2));
        }
        catch { Dispose(); throw; }
    }

    public void** StdinLocation { get { fixed (nint* p = standard) return (void**)p; } }
    public void** StdoutLocation { get { fixed (nint* p = standard) return (void**)(p + 1); } }
    public void** StderrLocation { get { fixed (nint* p = standard) return (void**)(p + 2); } }
    public void* Stdin => *StdinLocation;
    public void* Stdout => *StdoutLocation;
    public void* Stderr => *StderrLocation;

    private void* Add(Entry entry)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            // A tombstone remains until owner disposal, so stale FILE addresses
            // cannot silently identify later fopen results within this owner.
            files.EnsureCapacity(checked(files.Count + 1));
            void* token = NativeMemory.Alloc(1);
            if (token == null) throw new OutOfMemoryException();
            try { files.Add((nuint)token, entry); return token; }
            catch { NativeMemory.Free(token); throw; }
        }
    }

    private Entry? Find(void* handle)
    {
        lock (gate)
        {
            if (disposed || !files.TryGetValue((nuint)handle, out var entry)) { setErrno(Ebadf); return null; }
            return entry;
        }
    }
    private int Fail(Entry? entry, int error)
    {
        if (entry is not null) { entry.Error = true; entry.FailureEpoch = unchecked(entry.FailureEpoch + 1); }
        setErrno(error); return Eof;
    }
    private static int ErrorNumber(Exception error) => error switch
    {
        FileNotFoundException or DirectoryNotFoundException => 2,
        UnauthorizedAccessException => 13,
        PathTooLongException => 36,
        DecoderFallbackException or EncoderFallbackException => 84,
        OutOfMemoryException => Enomem,
        PlatformNotSupportedException => 95,
        NotSupportedException => Espipe,
        ArgumentException => Einval,
        IOException io => IoErrorNumber(io.HResult & 0xffff),
        _ => 5
    };
    private static int IoErrorNumber(int code)
    {
        // BCL exception categories cover most portable cases. Remaining native
        // error values have different meanings on Windows, Linux and macOS.
        if (OperatingSystem.IsWindows()) return code switch
        {
            2 or 3 => 2, 5 or 32 or 33 => 13, 17 => 18, 80 or 183 => 17,
            39 or 112 => 28, 145 => 39, 206 => 36, _ => 5
        };
        if (OperatingSystem.IsMacOS()) code = code switch { 35 => 11, 63 => 36, 66 => 39, _ => code };
        return code switch
        {
            1 or 2 or 5 or 9 or 11 or 12 or 13 or 17 or 18 or 20 or 21 or 22 or 24
                or 27 or 28 or 29 or 30 or 32 or 36 or 39 or 40 => code,
            _ => 5
        };
    }
    private static bool PlatformFailure(Exception error) => error is IOException or UnauthorizedAccessException
        or ArgumentException or NotSupportedException or OutOfMemoryException;
    private static string Decode(sbyte* text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        int length = 0; while (text[length] != 0) length = checked(length + 1);
        return Utf8.GetString(new ReadOnlySpan<byte>(text, length));
    }

    public void* Fopen(sbyte* path, sbyte* mode)
    {
        FileStream? stream = null;
        try
        {
            string spelling = Decode(mode);
            if (spelling.Length == 0 || "rwa".IndexOf(spelling[0]) < 0) { setErrno(Einval); return null; }
            bool update = false, binary = false, exclusive = false;
            for (int i = 1; i < spelling.Length; ++i)
            {
                if (spelling[i] == '+' && !update) update = true;
                else if (spelling[i] == 'b' && !binary) binary = true;
                else if (spelling[i] == 'x' && !exclusive && spelling[0] == 'w') exclusive = true;
                else { setErrno(Einval); return null; }
            }
            bool read = spelling[0] == 'r' || update, write = spelling[0] != 'r' || update, append = spelling[0] == 'a';
            string decoded = Decode(path);
            if (decoded.Length == 0) { setErrno(2); return null; }
            string filename = Path.GetFullPath(decoded, workingDirectory);
            stream = append ? LibcAppend.Open(filename, read) : new FileStream(filename, new FileStreamOptions
            {
                Mode = spelling[0] == 'r' ? FileMode.Open : spelling[0] == 'w' ?
                    (exclusive ? FileMode.CreateNew : FileMode.Create) : FileMode.OpenOrCreate,
                Access = read ? (write ? FileAccess.ReadWrite : FileAccess.Read) : FileAccess.Write,
                Share = FileShare.ReadWrite | FileShare.Delete,
                BufferSize = 1
            });
            // ISO C leaves the initial append read position implementation
            // defined. Select beginning for a+ (musl/glibc), end for a.
            if (append && !read) stream.Seek(0, SeekOrigin.End);
            var result = Add(new Entry(stream, read, write, append, true));
            stream = null; return result;
        }
        catch (Exception error) when (PlatformFailure(error)) { setErrno(ErrorNumber(error)); return null; }
        finally { stream?.Dispose(); }
    }

    private bool FlushOutput(Entry entry)
    {
        try
        {
            nuint written = 0;
            while (written < entry.Used)
            {
                int count = (int)Math.Min((ulong)(entry.Used - written), int.MaxValue);
                var bytes = new ReadOnlySpan<byte>(entry.Buffer + written, count);
                if (entry.Append) LibcAppend.Write((FileStream)entry.Stream, bytes);
                else entry.Stream.Write(bytes);
                written += (uint)count;
            }
            entry.Used = 0; entry.Stream.Flush(); return true;
        }
        catch (Exception error) when (PlatformFailure(error))
        {
            // After an I/O failure the partial external byte count is unknown;
            // retaining the whole buffer would duplicate already-written bytes.
            entry.Used = 0; Fail(entry, ErrorNumber(error)); return false;
        }
    }

    public int Fclose(void* handle)
    {
        var entry = Find(handle); if (entry is null) return Eof;
        lock (entry.Gate)
        {
            if (entry.Closed) return Fail(null, Ebadf);
            bool ok = !entry.Writable || FlushOutput(entry);
            try { if (entry.OwnStream) entry.Stream.Dispose(); }
            catch (Exception error) when (PlatformFailure(error)) { Fail(entry, ErrorNumber(error)); ok = false; }
            finally
            {
                entry.Closed = true;
                if (entry.OwnBuffer) NativeMemory.Free(entry.Buffer);
                entry.Buffer = null; entry.Capacity = entry.Used = 0;
            }
            return ok ? 0 : Eof;
        }
    }

    private string ResolvePath(sbyte* path)
    {
        string decoded = Decode(path);
        if (decoded.Length == 0) throw new FileNotFoundException("Empty filename.");
        return Path.GetFullPath(decoded, workingDirectory);
    }
    public int Remove(sbyte* path)
    {
        try { LibcPaths.Remove(ResolvePath(path)); return 0; }
        catch (Exception error) when (PlatformFailure(error)) { setErrno(ErrorNumber(error)); return -1; }
    }
    public int Rename(sbyte* source, sbyte* destination)
    {
        try { LibcPaths.Rename(ResolvePath(source), ResolvePath(destination)); return 0; }
        catch (Exception error) when (PlatformFailure(error)) { setErrno(ErrorNumber(error)); return -1; }
    }

    private bool PrepareRead(Entry entry)
    {
        if (entry.Closed || !entry.Readable) { Fail(entry, Ebadf); return false; }
        // The C update-stream sequencing rule requires fflush/positioning after
        // output. Report EINVAL for that undefined call sequence rather than
        // accidentally returning stale bytes from an output buffer.
        if (entry.Direction == 2) { Fail(entry, Einval); return false; }
        entry.Started = true; entry.Direction = 1; return true;
    }
    private bool PrepareWrite(Entry entry)
    {
        if (entry.Closed || !entry.Writable) { Fail(entry, Ebadf); return false; }
        if (entry.Direction == 1 && !entry.End) { Fail(entry, Einval); return false; }
        entry.Started = true; entry.Direction = 2; return true;
    }

    public ulong Fread(void* destination, ulong size, ulong count, void* handle)
    {
        if(size==0 || count==0)return 0;
        var entry=Find(handle);if(entry is null)return 0;
        lock(entry.Gate) { if(!Orient(entry,-1))return 0;return ReadBytes(destination,size,count,entry); }
    }
    private ulong ReadBytes(void* destination,ulong size,ulong count,Entry entry)
    {
        if (count > (ulong)nint.MaxValue / size) { Fail(entry, Eoverflow); return 0; }
        if (!PrepareRead(entry) || entry.End) return 0;
        ulong total = size * count, read = 0;
        byte* output = (byte*)destination;
        if (entry.Pushback >= 0) { output[read++] = (byte)entry.Pushback; entry.Pushback = -1; }
        try
        {
            while (read < total)
            {
                if (entry.ReadOffset < entry.ReadCount)
                {
                    int copied = (int)Math.Min(total - read, (uint)(entry.ReadCount - entry.ReadOffset));
                    Buffer.MemoryCopy(entry.Buffer + entry.ReadOffset, output + read, total - read, (ulong)copied);
                    entry.ReadOffset += copied; read += (uint)copied; continue;
                }
                if (entry.BufferMode != 2 && total - read < entry.Capacity)
                {
                    if (!AllocateBuffer(entry)) break;
                    entry.ReadOffset = 0;
                    entry.ReadCount = entry.Stream.Read(new Span<byte>(entry.Buffer, (int)Math.Min((ulong)entry.Capacity, int.MaxValue)));
                    if (entry.ReadCount == 0) { entry.End = true; break; }
                }
                else
                {
                    int length = (int)Math.Min(total - read, int.MaxValue);
                    int n = entry.Stream.Read(new Span<byte>(output + read, length));
                    if (n == 0) { entry.End = true; break; }
                    read += (uint)n;
                }
            }
        }
        catch (Exception error) when (PlatformFailure(error)) { Fail(entry, ErrorNumber(error)); }
        return read / size;
    }

    private bool AllocateBuffer(Entry entry)
    {
        if (entry.Buffer != null) return true;
        try
        {
            entry.Buffer = (byte*)NativeMemory.Alloc(entry.Capacity);
            if (entry.Buffer == null) { Fail(entry, Enomem); return false; }
            entry.OwnBuffer = true; return true;
        }
        catch (OutOfMemoryException) { Fail(entry, Enomem); return false; }
    }

    public ulong Fwrite(void* source, ulong size, ulong count, void* handle)
    {
        if(size==0 || count==0)return 0;
        var entry=Find(handle);if(entry is null)return 0;
        lock(entry.Gate) { if(!Orient(entry,-1))return 0;return WriteBytes(source,size,count,entry); }
    }
    private ulong WriteBytes(void* source,ulong size,ulong count,Entry entry)
    {
        if (count > (ulong)nint.MaxValue / size) { Fail(entry, Eoverflow); return 0; }
        if (!PrepareWrite(entry)) return 0;
        ulong total = size * count, accepted = 0;
        byte* input = (byte*)source;
        try
        {
            if (entry.BufferMode == 2)
            {
                while (accepted < total)
                {
                    int length = (int)Math.Min(total - accepted, int.MaxValue);
                    var bytes = new ReadOnlySpan<byte>(input + accepted, length);
                    if (entry.Append) LibcAppend.Write((FileStream)entry.Stream, bytes);
                    else entry.Stream.Write(bytes);
                    accepted += (uint)length;
                }
            }
            else
            {
                if (!AllocateBuffer(entry)) return 0;
                ulong bufferedFromCall = 0;
                while (accepted < total)
                {
                    int length = (int)Math.Min(Math.Min(total - accepted, (ulong)(entry.Capacity - entry.Used)), int.MaxValue);
                    bool newline = false;
                    if (entry.BufferMode == 1)
                    {
                        int index = new ReadOnlySpan<byte>(input + accepted, length).IndexOf((byte)'\n');
                        if (index >= 0) { length = index + 1; newline = true; }
                    }
                    Buffer.MemoryCopy(input + accepted, entry.Buffer + entry.Used, (ulong)(entry.Capacity - entry.Used), (ulong)length);
                    accepted += (uint)length; bufferedFromCall += (uint)length; entry.Used += (uint)length;
                    if (newline || entry.Used == entry.Capacity)
                    {
                        if (!FlushOutput(entry)) { accepted -= bufferedFromCall; break; }
                        bufferedFromCall = 0;
                    }
                }
            }
        }
        catch (Exception error) when (PlatformFailure(error)) { Fail(entry, ErrorNumber(error)); }
        return accepted / size;
    }

    public int Fflush(void* handle)
    {
        if (handle == null)
        {
            List<Entry> snapshot;
            lock (gate) snapshot = new(files.Values);
            int result = 0;
            foreach (var candidate in snapshot) lock (candidate.Gate)
                if (!candidate.Closed && candidate.Writable)
                {
                    if (!FlushOutput(candidate)) result = Eof;
                    else if (candidate.Direction == 2) candidate.Direction = 0;
                }
            return result;
        }
        var entry = Find(handle); if (entry is null) return Eof;
        lock (entry.Gate)
        {
            if (entry.Closed) return Fail(null, Ebadf);
            int unread = entry.ReadCount - entry.ReadOffset + (entry.Pushback >= 0 ? 1 : 0) + entry.WidePushbackBytes;
            if (entry.Direction == 1 && unread != 0)
            {
                try { entry.Stream.Seek(-unread, SeekOrigin.Current); entry.Pushback = -1; entry.WidePushback = -1; entry.WidePushbackBytes = 0; entry.DecodeState = entry.DecodeValue = 0; entry.ReadCount = entry.ReadOffset = 0; }
                catch (Exception error) when (PlatformFailure(error)) { return Fail(entry, ErrorNumber(error)); }
            }
            if (entry.Writable && !FlushOutput(entry)) return Eof;
            entry.Direction = 0; return 0;
        }
    }

    public int Fseek(void* handle, long offset, int origin)
    {
        var entry = Find(handle); if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Closed) return Fail(null, Ebadf);
            if (origin < 0 || origin > 2) return Fail(entry, Einval);
            if (!entry.Stream.CanSeek) return Fail(entry, Espipe);
            if (entry.Writable && !FlushOutput(entry)) return -1;
            try
            {
                if (origin == 1) offset = checked(offset - (entry.ReadCount - entry.ReadOffset) - (entry.Pushback >= 0 ? 1 : 0) - entry.WidePushbackBytes);
                entry.Stream.Seek(offset, (SeekOrigin)origin);
                entry.Pushback = -1; entry.WidePushback = -1; entry.WidePushbackBytes = 0; entry.DecodeState = entry.DecodeValue = 0; entry.ReadCount = entry.ReadOffset = 0;
                entry.End = false; entry.Direction = 0; entry.Started = true; return 0;
            }
            catch (OverflowException) { return Fail(entry, Eoverflow); }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(entry, ErrorNumber(error)); }
        }
    }

    public long Ftell(void* handle)
    {
        var entry = Find(handle); if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Closed) return Fail(null, Ebadf);
            if (!entry.Stream.CanSeek) return Fail(entry, Espipe);
            try
            {
                // An append stream's pending bytes will be written at current
                // end, irrespective of its last explicit seek position.
                long position = entry.Append && entry.Used != 0 ? entry.Stream.Length : entry.Stream.Position;
                return checked(position + (long)entry.Used - (entry.ReadCount - entry.ReadOffset) - (entry.Pushback >= 0 ? 1 : 0) - entry.WidePushbackBytes);
            }
            catch (OverflowException) { return Fail(entry, Eoverflow); }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(entry, ErrorNumber(error)); }
        }
    }
    public int Fgetpos(void* handle, long* position)
    { long result = Ftell(handle); if (result < 0) return -1; *position = result; return 0; }
    public int Fsetpos(void* handle, long* position) => Fseek(handle, *position, 0);
    public int Feof(void* handle)
    { var entry = Find(handle); if (entry is null) return 0; lock (entry.Gate) return entry.End ? 1 : 0; }
    public int Ferror(void* handle)
    { var entry = Find(handle); if (entry is null) return 0; lock (entry.Gate) return entry.Error ? 1 : 0; }
    public void Clearerr(void* handle)
    { var entry = Find(handle); if (entry is null) return; lock (entry.Gate) { entry.End = false; entry.Error = false; } }
    public int Setvbuf(void* handle, sbyte* buffer, int mode, ulong size)
    {
        var entry = Find(handle); if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Closed) return Fail(null, Ebadf);
            if (entry.Started || mode < 0 || mode > 2 || (mode != 2 && (size == 0 || size > (ulong)nint.MaxValue)))
                { setErrno(Einval); return -1; }
            if (entry.OwnBuffer) NativeMemory.Free(entry.Buffer);
            entry.Buffer = mode == 2 ? null : (byte*)buffer; entry.OwnBuffer = false;
            entry.Capacity = mode == 2 ? 0 : (nuint)size; entry.Used = 0; entry.BufferMode = mode; return 0;
        }
    }
    public int Fgetc(void* handle) { byte value; return Fread(&value, 1, 1, handle) == 1 ? value : Eof; }
    public int Getchar() => Fgetc(Stdin);
    public int Putchar(int value) => Fputc(value, Stdout);
    public sbyte* Fgets(sbyte* destination, int count, void* handle)
    {
        if (count <= 0) return null;
        var entry = Find(handle); if (entry is null) return null;
        lock (entry.Gate)
        {
            if (!Orient(entry,-1) || !PrepareRead(entry)) return null;
            if (count == 1) { *destination = 0; return destination; }
            int length = 0, epoch = entry.FailureEpoch;
            while (length < count - 1)
            {
                int value = Fgetc(handle);
                if (value == Eof) break;
                destination[length++] = unchecked((sbyte)value);
                if (value == '\n') break;
            }
            if (length == 0 || epoch != entry.FailureEpoch) return null;
            destination[length] = 0; return destination;
        }
    }
    public int Ungetc(int value, void* handle)
    {
        if (value == Eof) return Eof;
        var entry = Find(handle); if (entry is null) return Eof;
        lock (entry.Gate)
        {
            if (!Orient(entry,-1) || !PrepareRead(entry) || entry.Pushback >= 0) return Eof;
            entry.Pushback = unchecked((byte)value); entry.End = false; return entry.Pushback;
        }
    }
    public int Fputc(int value, void* handle)
    { byte character = unchecked((byte)value); return Fwrite(&character, 1, 1, handle) == 1 ? character : Eof; }
    public int Fputs(sbyte* text, void* handle)
    {
        ulong length = 0; while (text[length] != 0) ++length;
        return Fwrite(text, 1, length, handle) == length ? 0 : Eof;
    }
    public int Puts(sbyte* text)
    {
        void* handle = Stdout; var entry = Find(handle); if (entry is null) return Eof;
        lock (entry.Gate) return Fputs(text, handle) < 0 ? Eof : Fputc('\n', handle) < 0 ? Eof : 0;
    }

    public void Dispose()
    {
        List<KeyValuePair<nuint, Entry>> snapshot;
        lock (gate) { if (disposed) return; disposed = true; snapshot = new(files); }
        foreach (var item in snapshot)
        {
            var entry = item.Value;
            lock (entry.Gate)
            {
                if (!entry.Closed)
                {
                    if (entry.Writable) FlushOutput(entry);
                    try { if (entry.OwnStream) entry.Stream.Dispose(); }
                    catch (Exception error) when (PlatformFailure(error)) { setErrno(ErrorNumber(error)); }
                    if (entry.OwnBuffer) NativeMemory.Free(entry.Buffer);
                    entry.Buffer = null; entry.Used = 0; entry.Closed = true;
                }
                NativeMemory.Free((void*)item.Key);
            }
        }
        lock (gate) { files.Clear(); Array.Clear(standard); }
    }
}
