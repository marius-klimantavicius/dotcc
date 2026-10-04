#nullable enable
using System.IO;

namespace Edg.Runtime;

/// <summary>Optional shared descriptor namespace for the ISO C stream provider.
/// A stream owns its descriptor; closing it must release that same descriptor.</summary>
public interface ILibcFileDescriptors
{
    Stream Open(string path, string mode);
    Stream Fdopen(int descriptor, string mode);
    int Fileno(Stream stream);
    Stream Standard(Stream stream, int descriptor, bool terminal, bool native);
    void SetAppend(Stream stream, bool append);
    string ResolvePath(string path);
}

public sealed class LibcDescriptorException : IOException
{
    public int ErrorNumber { get; }
    public LibcDescriptorException(int error) : base("POSIX descriptor operation failed: " + error) => ErrorNumber = error;
}
