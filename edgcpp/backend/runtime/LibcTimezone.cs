#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Edg.Runtime;

/// <summary>Platform boundary for translated musl timezone algorithms. Calendar
/// arithmetic, POSIX rules and DST selection remain translated C. This service
/// owns locking and file views; shared named-zone bytes are immutable metadata.</summary>
public sealed unsafe class LibcTimezone : IDisposable
{
    private readonly record struct Mapping(byte[]? Owned,ulong Length,int References);
    private static readonly Encoding utf8=new UTF8Encoding(false,true);
    private readonly object gate=new();
    private readonly Action<int> setErrno;
    private readonly Dictionary<nuint,Mapping> maps=new();
    private bool disposed;
    public LibcTimezone(Action<int> setErrno)
    {
        ArgumentNullException.ThrowIfNull(setErrno);
        // Load/validate immutable resources before translated C can acquire its
        // explicit timezone lock. A missing deployment resource is a host error.
        if(!LibcTimezoneData.TryGet("UTC",out _,out _))throw new InvalidDataException("Pinned UTC timezone is missing");
        this.setErrno=setErrno;
    }
    private void Alive()=>ObjectDisposedException.ThrowIf(disposed,this);
    public void Enter()
    {
        Monitor.Enter(gate);
        if(disposed){Monitor.Exit(gate);throw new ObjectDisposedException(nameof(LibcTimezone));}
    }
    public void Exit()=>Monitor.Exit(gate);

    // Bound every slice consumed by musl's TZif parser. Leap records are skipped
    // by POSIX time conversion, but their sizes still participate in validation.
    private static bool Block(ReadOnlySpan<byte> bytes,int start,int width,out int end)
    {
        end=0;
        if(start<0 || start>bytes.Length-44 || !bytes.Slice(start,4).SequenceEqual("TZif"u8))return false;
        uint gmt=BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(start+20,4));
        uint standard=BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(start+24,4));
        uint leaps=BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(start+28,4));
        uint times=BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(start+32,4));
        uint types=BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(start+36,4));
        uint chars=BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(start+40,4));
        if(types==0 || types>256 || chars==0 || (gmt!=0 && gmt!=types) || (standard!=0 && standard!=types))return false;
        long size=(long)(width+1)*times+6L*types+chars+(width+4L)*leaps+standard+gmt;
        if(size>bytes.Length-start-44)return false;
        end=start+44+(int)size;
        int transition=start+44,indices=transition+(int)times*width,typeData=indices+(int)times,abbreviations=typeData+(int)types*6;
        long previous=long.MinValue;
        for(int i=0;i<(int)times;++i) {
            long time=width==8?BinaryPrimitives.ReadInt64BigEndian(bytes.Slice(transition+i*8,8)):BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(transition+i*4,4));
            if((i!=0 && time<=previous) || bytes[indices+i]>=types)return false;
            previous=time;
        }
        ReadOnlySpan<byte> names=bytes.Slice(abbreviations,(int)chars);
        for(int i=0;i<(int)types;++i) {
            int offset=typeData+i*6,index=bytes[offset+5];
            if(bytes[offset+4]>1 || index>=names.Length || names[index..].IndexOf((byte)0)<0)return false;
        }
        int flags=end-(int)standard-(int)gmt;
        for(int i=flags;i<end;++i)if(bytes[i]>1)return false;
        return true;
    }
    public static bool ValidateTzif(ReadOnlySpan<byte> bytes)
    {
        if(bytes.Length<44)return false;
        byte version=bytes[4];
        if(version!=0 && version!='1' && version!='2' && version!='3' && version!='4')return false;
        if(!Block(bytes,0,4,out int end))return false;
        if(version==0 || version=='1')return end==bytes.Length;
        if(end>bytes.Length-44 || bytes[end+4]!=version || !Block(bytes,end,8,out end))return false;
        if(end>bytes.Length-2 || bytes[end]!='\n' || bytes[^1]!='\n')return false;
        return ValidFooter(bytes.Slice(end+1,bytes.Length-end-2));
    }
    // Files are an external byte boundary. Reject malformed rules before musl
    // receives pointers; its internal parser assumes structurally valid TZif.
    private static bool Name(ReadOnlySpan<byte> text,ref int at)
    {
        int start=at;
        if(at<text.Length && text[at]=='<') {
            ++at;int first=at;
            while(at<text.Length && text[at]!='>') {
                byte c=text[at++];if(!((c>='A'&&c<='Z')||(c>='a'&&c<='z')||(c>='0'&&c<='9')||c=='+'||c=='-'))return false;
            }
            if(at==text.Length || at-first<3)return false;
            ++at;return true;
        }
        while(at<text.Length && ((text[at]>='A'&&text[at]<='Z')||(text[at]>='a'&&text[at]<='z')))++at;
        return at-start>=3;
    }
    private static bool Number(ReadOnlySpan<byte> text,ref int at,int low,int high)
    {
        int start=at,value=0;
        while(at<text.Length && text[at]>='0' && text[at]<='9') {
            value=value*10+text[at++]-'0';if(value>high)return false;
        }
        return at!=start && value>=low;
    }
    private static bool Offset(ReadOnlySpan<byte> text,ref int at,int maxHour)
    {
        if(at<text.Length && (text[at]=='+'||text[at]=='-'))++at;
        if(!Number(text,ref at,0,maxHour))return false;
        if(at<text.Length && text[at]==':') {
            ++at;if(!Number(text,ref at,0,59))return false;
            if(at<text.Length && text[at]==':'){++at;if(!Number(text,ref at,0,59))return false;}
        }
        return true;
    }
    private static bool Rule(ReadOnlySpan<byte> text,ref int at)
    {
        if(at==text.Length)return false;
        if(text[at]=='M') {
            ++at;if(!Number(text,ref at,1,12)||at==text.Length||text[at++]!='.')return false;
            if(!Number(text,ref at,1,5)||at==text.Length||text[at++]!='.')return false;
            if(!Number(text,ref at,0,6))return false;
        } else if(text[at]=='J') {
            ++at;if(!Number(text,ref at,1,365))return false;
        } else if(!Number(text,ref at,0,365))return false;
        if(at<text.Length&&text[at]=='/'){++at;if(!Offset(text,ref at,167))return false;}
        return true;
    }
    private static bool ValidFooter(ReadOnlySpan<byte> text)
    {
        if(text.Length==0)return true;
        int at=0;if(!Name(text,ref at)||!Offset(text,ref at,24))return false;
        if(at==text.Length)return true;
        if(!Name(text,ref at))return false;
        if(at<text.Length&&text[at]!=','&&!Offset(text,ref at,24))return false;
        if(at==text.Length||text[at++]!=','||!Rule(text,ref at))return false;
        if(at==text.Length||text[at++]!=','||!Rule(text,ref at))return false;
        return at==text.Length;
    }
    private static string Decode(sbyte* path)
    {
        if(path==null)throw new ArgumentNullException(nameof(path));
        int length=0;while(path[length]!=0)length=checked(length+1);
        return utf8.GetString(new ReadOnlySpan<byte>(path,length));
    }
    private byte* Register(byte* address,ulong length,byte[]? owner)
    {
        nuint key=(nuint)address;
        if(maps.TryGetValue(key,out Mapping prior))maps[key]=prior with { References=checked(prior.References+1) };
        else maps.Add(key,new Mapping(owner,length,1));
        return address;
    }
    private byte* Named(string name,ulong* length)
    {
        if(!LibcTimezoneData.TryGet(name,out byte* bytes,out nuint size))return null;
        if(!ValidateTzif(new ReadOnlySpan<byte>(bytes,checked((int)size))))throw new InvalidDataException("Invalid pinned TZif payload");
        *length=(ulong)size;return Register(bytes,*length,null);
    }
    private byte* DefaultName(ulong* length)
    {
        string name=TimeZoneInfo.Local.Id;
        byte* result=Named(name,length);
        if(result!=null)return result;
        if(TimeZoneInfo.TryConvertWindowsIdToIanaId(name,out string? iana))return Named(iana,length);
        return null;
    }
    private byte* ReadFile(string path,ulong* length)
    {
        using var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
        long size=file.Length;
        if(size>Array.MaxLength){setErrno(12);return null;}
        if(size<44)return null;
        byte[] data=GC.AllocateArray<byte>((int)size,pinned:true);
        file.ReadExactly(data);
        if(!ValidateTzif(data))return null;
        // The selected musl parser's private v1 marker is ASCII '1'. Standard
        // TZif v1 files carry NUL; normalize only this owned private file view.
        if(data[4]==0)data[4]=(byte)'1';
        *length=(ulong)size;
        return Register((byte*)Unsafe.AsPointer(ref data[0]),*length,data);
    }
    public byte* MapFile(sbyte* path,ulong* length)
    {
        lock(gate) {
            Alive();if(length==null){setErrno(14);return null;}
            try {
                string name=Decode(path);
                // musl's three default search roots resolve to pinned IANA data
                // on every OS; explicit absolute/relative paths use BCL files.
                foreach(string prefix in SearchPrefixes)if(name.StartsWith(prefix,StringComparison.Ordinal)) {
                    byte* named=Named(name[prefix.Length..],length);
                    if(named!=null)return named;
                    break;
                }
                if(name=="/etc/localtime" && OperatingSystem.IsWindows())return DefaultName(length);
                return ReadFile(name,length);
            }
            catch(Exception error) when(error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or OutOfMemoryException or OverflowException) {
                setErrno(ErrorNumber(error));return null;
            }
        }
    }
    private static readonly string[] SearchPrefixes={"/usr/share/zoneinfo/","/share/zoneinfo/","/etc/zoneinfo/"};
    private static int ErrorNumber(Exception error)=>error switch {
        FileNotFoundException or DirectoryNotFoundException=>2,
        UnauthorizedAccessException=>13,
        PathTooLongException=>36,
        DecoderFallbackException or EncoderFallbackException=>84,
        OutOfMemoryException=>12,
        OverflowException=>75,
        ArgumentException=>22,
        NotSupportedException=>95,
        _=>5
    };
    public int Unmap(void* address,ulong length)
    {
        lock(gate) {
            Alive();nuint key=(nuint)address;
            if(!maps.TryGetValue(key,out Mapping mapping) || mapping.Length!=length){setErrno(22);return -1;}
            if(mapping.References>1)maps[key]=mapping with { References=mapping.References-1 };else maps.Remove(key);
            return 0;
        }
    }
    public void Dispose()
    {
        lock(gate){if(disposed)return;disposed=true;maps.Clear();}
    }
}
