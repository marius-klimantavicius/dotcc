internal sealed class Pcap : IDisposable
{
    private readonly BinaryWriter output;
    private uint sequence;
    internal Pcap(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        output=new BinaryWriter(File.Create(path));output.Write(0xa1b2c3d4u);output.Write((ushort)2);output.Write((ushort)4);output.Write(0);output.Write(0u);output.Write(65535u);output.Write(1u);
    }
    internal void Write(byte[] frame)
    {lock(output){output.Write(0u);output.Write(sequence++);output.Write((uint)frame.Length);output.Write((uint)frame.Length);output.Write(frame);}}
    public void Dispose(){lock(output)output.Dispose();}
}
