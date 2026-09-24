#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ManagedGlobResult
    {
        public nuint Count;
        public byte** Paths;
        public nuint Offset;
        public int Flags;
        public nint CloseDirectory, ReadDirectory, OpenDirectory, Lstat, Stat;
    }
    private delegate int GlobError(string path, int error);
    private const int GlobSupportedFlags = 1 | 2 | 4 | 16 | 64 | 128 | 8192;

    /// <summary>Linux glob with C-locale byte matching, using the bound runtime's filesystem and heap.</summary>
    public static int glob(byte* pattern, int flags, delegate*<byte*, int, int> error, void* result)
    {
        nint address = (nint)error;
        return GlobCore(pattern, flags, address == 0 ? null : (path, code) =>
        {
            fixed (byte* bytes = Encoding.UTF8.GetBytes(path + "\0"))
                return ((delegate*<byte*, int, int>)address)(bytes, code);
        }, result);
    }

    /// <summary>Preserves the explicit program identity through filesystem access, allocations and callbacks.</summary>
    public static int GlobManaged<T>(T instance, byte* pattern, int flags,
        delegate*<T, byte*, int, int> error, void* result) where T : class, IProgramInstance
    {
        ArgumentNullException.ThrowIfNull(instance);
        using var binding = instance.__DotCcRuntime.Enter();
        nint address = (nint)error;
        return GlobCore(pattern, flags, address == 0 ? null : (path, code) =>
        {
            fixed (byte* bytes = Encoding.UTF8.GetBytes(path + "\0"))
                return ((delegate*<T, byte*, int, int>)address)(instance, bytes, code);
        }, result);
    }

    public static void globfree(void* result)
    {
        if (result == null) return;
        var output = (ManagedGlobResult*)result;
        if (output->Paths != null)
        {
            // Validate ownership before touching an allocation from another runtime.
            lock (ownedHeapLock)
            {
                FindOwnedHeapBlock(output->Paths, "globfree", out _);
                for (nuint i = 0; i < output->Count; i++) FreeHeap(output->Paths[i]);
                FreeHeap(output->Paths);
            }
        }
        output->Count = 0;
        output->Paths = null;
        output->Offset = 0;
    }

    private static int GlobCore(byte* pattern, int flags, GlobError? error, void* result)
    {
        if (result == null || pattern == null) { errno = EFAULT; return 2; }
        var output = (ManagedGlobResult*)result;
        output->Count = 0; output->Paths = null; output->Offset = 0; output->Flags = 0;
        if ((flags & ~GlobSupportedFlags) != 0) { errno = ENOTSUP; return 2; }
        try
        {
            string text = Str(pattern);
            bool noEscape = (flags & 64) != 0;
            var prefixes = new List<string> { "" };
            bool magic = false;
            int position = 0;
            while (position < text.Length)
            {
                int separatorStart = position;
                while (position < text.Length && text[position] == '/') position++;
                string separator = text.Substring(separatorStart, position - separatorStart);
                int start = position;
                while (position < text.Length && text[position] != '/') position++;
                string component = text.Substring(start, position - start);
                bool final = position == text.Length;
                byte[] match = Encoding.UTF8.GetBytes(component);
                bool wildcard = GlobHasMagic(match, noEscape);
                magic |= wildcard;
                var next = new List<string>();
                foreach (string prefix in prefixes)
                {
                    string logicalBase = prefix + separator;
                    if (!wildcard)
                    {
                        string candidate = logicalBase + GlobUnescape(component, noEscape);
                        if (component.Length == 0 ? Directory.Exists(ResolvePath(candidate)) :
                            !final || GlobPathExists(candidate)) next.Add(candidate);
                        continue;
                    }
                    string directory = logicalBase.Length == 0 ? "." : logicalBase.TrimEnd('/');
                    if (directory.Length == 0) directory = "/";
                    string physical = ResolvePath(directory);
                    // glibc treats a non-directory prefix as no matches, without errfunc.
                    if (File.Exists(physical) && !Directory.Exists(physical)) continue;
                    string[] entries;
                    try { entries = Directory.GetFileSystemEntries(physical); }
                    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                    {
                        int code = exception is UnauthorizedAccessException ? EACCES :
                            exception is DirectoryNotFoundException ? ENOENT : EIO;
                        errno = code;
                        if ((error?.Invoke(directory, code) ?? 0) != 0 || (flags & 1) != 0) return 2;
                        continue;
                    }
                    bool dots = (flags & 128) != 0 || (match.Length > 0 && match[0] == (byte)'.') ||
                        (!noEscape && match.Length > 1 && match[0] == (byte)'\\' && match[1] == (byte)'.');
                    var names = new List<string>();
                    if (dots) { names.Add("."); names.Add(".."); }
                    foreach (string entry in entries) names.Add(global::System.IO.Path.GetFileName(entry));
                    foreach (string name in names)
                    {
                        if (name.StartsWith('.') && !dots) continue;
                        if (!GlobMatch(match, Encoding.UTF8.GetBytes(name), noEscape)) continue;
                        string candidate = logicalBase + name;
                        if ((!final || (flags & 8192) != 0) && !Directory.Exists(ResolvePath(candidate))) continue;
                        next.Add(candidate);
                    }
                }
                prefixes = next;
            }
            if (text.Length == 0) prefixes.Clear();
            if ((flags & 8192) != 0) prefixes.RemoveAll(path => !Directory.Exists(ResolvePath(path)));
            if (prefixes.Count == 0)
            {
                if ((flags & 16) == 0) return 3;
                prefixes.Add(text);
            }
            if ((flags & 2) != 0)
                for (int i = 0; i < prefixes.Count; i++)
                    if (!prefixes[i].EndsWith('/') && Directory.Exists(ResolvePath(prefixes[i]))) prefixes[i] += "/";
            if ((flags & 4) == 0) prefixes.Sort(static (left, right) =>
                Encoding.UTF8.GetBytes(left).AsSpan().SequenceCompareTo(Encoding.UTF8.GetBytes(right)));
            output->Paths = (byte**)AllocateHeap(checked((nuint)(prefixes.Count + 1) * (nuint)sizeof(byte*)), true);
            if (output->Paths == null) { errno = ENOMEM; return 1; }
            foreach (string path in prefixes)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(path);
                byte* value = (byte*)AllocateHeap((nuint)bytes.Length + 1);
                if (value == null) { globfree(result); errno = ENOMEM; return 1; }
                bytes.CopyTo(new Span<byte>(value, bytes.Length)); value[bytes.Length] = 0;
                output->Paths[output->Count++] = value;
            }
            output->Flags = flags | (magic ? 256 : 16);
            return 0;
        }
        catch (OutOfMemoryException) { globfree(result); errno = ENOMEM; return 1; }
    }

    private static bool GlobPathExists(string path)
    {
        try { _ = File.GetAttributes(ResolvePath(path)); return true; }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }
    private static string GlobUnescape(string pattern, bool noEscape)
    {
        if (noEscape) return pattern;
        var result = new StringBuilder();
        for (int i = 0; i < pattern.Length; i++)
        {
            if (pattern[i] == '\\' && i + 1 < pattern.Length) i++;
            result.Append(pattern[i]);
        }
        return result.ToString();
    }
    private static bool GlobHasMagic(byte[] pattern, bool noEscape)
    {
        for (int i = 0; i < pattern.Length; i++)
        {
            if (!noEscape && pattern[i] == '\\') { i++; continue; }
            if (pattern[i] == '*' || pattern[i] == '?' || pattern[i] == '[') return true;
        }
        return false;
    }
    private static bool GlobMatch(byte[] pattern, byte[] name, bool noEscape)
    {
        int p = 0, n = 0, star = -1, retry = 0;
        while (n < name.Length)
        {
            if (p < pattern.Length && pattern[p] == '*') { star = ++p; retry = n; continue; }
            int end = p;
            if (GlobAtom(pattern, ref end, name[n], noEscape)) { p = end; n++; continue; }
            if (star < 0) return false;
            p = star; n = ++retry;
        }
        while (p < pattern.Length && pattern[p] == '*') p++;
        return p == pattern.Length;
    }
    private static bool GlobAtom(byte[] pattern, ref int p, byte value, bool noEscape)
    {
        if (p >= pattern.Length) return false;
        byte atom = pattern[p++];
        if (atom == '?') return true;
        if (atom == '\\' && !noEscape)
            return p < pattern.Length && pattern[p++] == value;
        if (atom != '[') return atom == value;
        int begin = p;
        bool negate = p < pattern.Length && (pattern[p] == '!' || pattern[p] == '^');
        if (negate) p++;
        bool matched = false, first = true;
        while (p < pattern.Length && (pattern[p] != ']' || first))
        {
            first = false;
            if (pattern[p] == '[' && p + 1 < pattern.Length && pattern[p + 1] == ':')
            {
                int classStart = p + 2, classEnd = classStart;
                while (classEnd + 1 < pattern.Length && !(pattern[classEnd] == ':' && pattern[classEnd + 1] == ']')) classEnd++;
                if (classEnd + 1 < pattern.Length)
                {
                    matched |= GlobCharacterClass(Encoding.ASCII.GetString(pattern, classStart, classEnd - classStart), value);
                    p = classEnd + 2;
                    continue;
                }
            }
            byte lo = pattern[p++];
            if (lo == '\\' && !noEscape && p < pattern.Length) lo = pattern[p++];
            byte hi = lo;
            if (p + 1 < pattern.Length && pattern[p] == '-' && pattern[p + 1] != ']')
            {
                p++; hi = pattern[p++];
                if (hi == '\\' && !noEscape && p < pattern.Length) hi = pattern[p++];
            }
            matched |= value >= lo && value <= hi;
        }
        if (p == pattern.Length) { p = begin; return value == '['; }
        p++;
        return negate ? !matched : matched;
    }
    private static bool GlobCharacterClass(string name, byte value)
    {
        bool upper = value >= 'A' && value <= 'Z', lower = value >= 'a' && value <= 'z';
        bool digit = value >= '0' && value <= '9';
        return name switch
        {
            "alnum" => upper || lower || digit,
            "alpha" => upper || lower,
            "blank" => value == ' ' || value == '\t',
            "cntrl" => value < 32 || value == 127,
            "digit" => digit,
            "graph" => value >= 33 && value <= 126,
            "lower" => lower,
            "print" => value >= 32 && value <= 126,
            "punct" => value >= 33 && value <= 126 && !(upper || lower || digit),
            "space" => value == ' ' || (value >= 9 && value <= 13),
            "upper" => upper,
            "xdigit" => digit || (value >= 'a' && value <= 'f') || (value >= 'A' && value <= 'F'),
            _ => false
        };
    }

}
