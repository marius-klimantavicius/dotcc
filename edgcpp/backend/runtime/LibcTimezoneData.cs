#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Edg.Runtime;

/// <summary>Immutable pinned IANA TZif metadata, shared by generated owners.
/// The source manifest and embedded bundle are checked by the driver.</summary>
public static unsafe class LibcTimezoneData
{
    public const string Version="2026e";
    private readonly record struct Entry(int Offset,int Length);
    private static readonly byte[] data=Load();
    private static readonly Dictionary<string,Entry> index=MakeIndex();
    private static byte[] Load()
    {
        using Stream stream=typeof(LibcTimezoneData).Assembly.GetManifestResourceStream("Edg.Runtime.Tzdb.bin")
            ?? throw new InvalidDataException("Pinned translated timezone resource is missing");
        if(stream.Length<12 || stream.Length>16*1024*1024)throw new InvalidDataException("Invalid translated timezone resource size");
        byte[] result=GC.AllocateArray<byte>(checked((int)stream.Length),pinned:true);
        stream.ReadExactly(result);
        return result;
    }
    private static Dictionary<string,Entry> MakeIndex()
    {
        ReadOnlySpan<byte> bytes=data;
        if(!bytes[..8].SequenceEqual("EDGTZ01\0"u8))throw new InvalidDataException("Invalid translated timezone resource header");
        uint count=BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(8,4));
        if(count>10000)throw new InvalidDataException("Invalid translated timezone resource index");
        var result=new Dictionary<string,Entry>((int)count,StringComparer.Ordinal);
        int cursor=12,firstPayload=data.Length;
        for(uint i=0;i<count;++i) {
            if(cursor>data.Length-2)throw new InvalidDataException("Truncated translated timezone index");
            int length=BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(cursor,2));cursor+=2;
            if(length==0 || cursor>data.Length-length-8)throw new InvalidDataException("Truncated translated timezone name");
            ReadOnlySpan<byte> name=bytes.Slice(cursor,length);cursor+=length;
            foreach(byte c in name)if(c==0 || c>=128)throw new InvalidDataException("Invalid translated timezone name");
            uint offset=BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(cursor,4));
            uint size=BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(cursor+4,4));cursor+=8;
            if(size<44 || offset>data.Length || size>(uint)data.Length-offset)throw new InvalidDataException("Invalid translated timezone payload bounds");
            firstPayload=Math.Min(firstPayload,(int)offset);
            if(!result.TryAdd(Encoding.ASCII.GetString(name),new Entry((int)offset,(int)size)))throw new InvalidDataException("Duplicate translated timezone name");
        }
        if(cursor!=firstPayload)throw new InvalidDataException("Translated timezone index overlaps payload");
        return result;
    }
    public static bool TryGet(string name,out byte* address,out nuint length)
    {
        if(index.TryGetValue(name,out Entry entry)) {
            address=(byte*)Unsafe.AsPointer(ref data[entry.Offset]);length=(nuint)entry.Length;return true;
        }
        address=null;length=0;return false;
    }
}
