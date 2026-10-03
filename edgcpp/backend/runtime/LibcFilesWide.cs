using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Edg.Runtime;

public sealed unsafe partial class LibcFiles
{
    public const uint Weof = uint.MaxValue;
    private const int Eilseq = 84;
    // Portable fpos ABI v2: signed byte position followed by opaque decoder state.
    [StructLayout(LayoutKind.Sequential, Size=16)]
    public struct FilePosition { public long Offset; public uint State, Value; }

    private bool Orient(Entry entry,int mode)
    {
        if(entry.Closed){Fail(null,Ebadf);return false;}
        if(entry.Orientation==0){entry.Orientation=mode;entry.Utf8Encoding=getEncoding()>1;}
        if(entry.Orientation!=mode){Fail(entry,Einval);return false;}
        return true;
    }
    public int Fwide(void* handle,int mode)
    {
        var entry=Find(handle);if(entry is null)return 0;
        lock(entry.Gate) {
            if(entry.Closed){Fail(null,Ebadf);return 0;}
            if(entry.Orientation==0 && mode!=0)Orient(entry,mode>0?1:-1);
            return entry.Orientation;
        }
    }
    private int ReadByte(Entry entry) {byte value;return ReadBytes(&value,1,1,entry)==1?value:Eof;}
    private uint GetWide(Entry entry)
    {
        if(!Orient(entry,1) || !PrepareRead(entry))return Weof;
        if(entry.WidePushback>=0){uint value=(uint)entry.WidePushback;entry.WidePushback=-1;entry.WidePushbackBytes=0;return value;}
        while(true) {
            int current=ReadByte(entry);
            if(current<0){if(entry.DecodeState!=0)Fail(entry,Eilseq);return Weof;}
            uint b=(uint)current;
            if(!entry.Utf8Encoding)return b<128?b:0xdf00+b;
            if(entry.DecodeState==0) {
                if(b<128)return b;
                uint remaining,min,max;
                if(b>=0xc2 && b<=0xdf){remaining=1;min=0x80;max=0xbf;entry.DecodeValue=b&31;}
                else if(b>=0xe0 && b<=0xef){remaining=2;min=b==0xe0?0xa0U:0x80U;max=b==0xed?0x9fU:0xbfU;entry.DecodeValue=b&15;}
                else if(b>=0xf0 && b<=0xf4){remaining=3;min=b==0xf0?0x90U:0x80U;max=b==0xf4?0x8fU:0xbfU;entry.DecodeValue=b&7;}
                else {Fail(entry,Eilseq);return Weof;}
                entry.DecodeState=remaining|(min<<8)|(max<<16);continue;
            }
            uint state=entry.DecodeState;
            if(b<((state>>8)&255) || b>((state>>16)&255)) {
                // Preserve the offending next byte, as musl does after a partial sequence.
                entry.Pushback=current;entry.End=false;entry.DecodeState=entry.DecodeValue=0;
                Fail(entry,Eilseq);return Weof;
            }
            entry.DecodeValue=(entry.DecodeValue<<6)|(b&63);
            uint count=(state&255)-1;
            if(count==0){uint value=entry.DecodeValue;entry.DecodeState=entry.DecodeValue=0;return value;}
            entry.DecodeState=count|(0x80U<<8)|(0xbfU<<16);
        }
    }
    private static int EncodeWide(Entry entry,int value,Span<byte> bytes)
    {
        if((uint)value<128){bytes[0]=(byte)value;return 1;}
        if(!entry.Utf8Encoding){if((uint)value-0xdf80<=0x7f){bytes[0]=(byte)value;return 1;}return -1;}
        return Rune.TryCreate(value,out Rune rune)?rune.EncodeToUtf8(bytes):-1;
    }
    private uint PutWide(Entry entry,int value)
    {
        if(!Orient(entry,1) || !PrepareWrite(entry))return Weof;
        byte* bytes=stackalloc byte[4];int count=EncodeWide(entry,value,new Span<byte>(bytes,4));
        if(count<0){Fail(entry,Eilseq);return Weof;}
        return WriteBytes(bytes,1,(ulong)count,entry)==(ulong)count?(uint)value:Weof;
    }
    public uint Fgetwc(void* handle)
    {var entry=Find(handle);if(entry is null)return Weof;lock(entry.Gate)return GetWide(entry);}
    public uint Fputwc(int value,void* handle)
    {var entry=Find(handle);if(entry is null)return Weof;lock(entry.Gate)return PutWide(entry,value);}
    public uint Getwchar()=>Fgetwc(Stdin);
    public uint Putwchar(int value)=>Fputwc(value,Stdout);
    public uint Ungetwc(uint value,void* handle)
    {
        if(value==Weof)return Weof;var entry=Find(handle);if(entry is null)return Weof;
        lock(entry.Gate){
            if(!Orient(entry,1) || !PrepareRead(entry) || entry.WidePushback>=0)return Weof;
            Span<byte> bytes=stackalloc byte[4];int count=EncodeWide(entry,unchecked((int)value),bytes);
            if(count<0){setErrno(Eilseq);return Weof;}
            entry.WidePushback=(int)value;entry.WidePushbackBytes=count;entry.End=false;return value;
        }
    }
    public int* Fgetws(int* destination,int count,void* handle)
    {
        if(count<=0)return null;var entry=Find(handle);if(entry is null)return null;
        lock(entry.Gate){
            if(!Orient(entry,1) || !PrepareRead(entry))return null;
            if(count==1){*destination=0;return destination;}
            int index=0,epoch=entry.FailureEpoch;
            while(index<count-1){uint value=GetWide(entry);if(value==Weof)break;destination[index++]=(int)value;if(value=='\n')break;}
            if(index==0 || epoch!=entry.FailureEpoch)return null;destination[index]=0;return destination;
        }
    }
    public int Fputws(int* source,void* handle)
    {
        var entry=Find(handle);if(entry is null)return Eof;
        lock(entry.Gate){
            if(!Orient(entry,1) || !PrepareWrite(entry))return Eof;
            while(*source!=0)if(PutWide(entry,*source++)==Weof)return Eof;
            return 0;
        }
    }
    public int FgetposState(void* handle,void* position)
    {
        var entry=Find(handle);if(entry is null)return -1;
        lock(entry.Gate){long offset=Ftell(handle);if(offset<0)return -1;*(FilePosition*)position=new FilePosition{Offset=offset,State=entry.DecodeState,Value=entry.DecodeValue};return 0;}
    }
    public int FsetposState(void* handle,void* position)
    {
        FilePosition saved=*(FilePosition*)position;var entry=Find(handle);if(entry is null)return -1;
        lock(entry.Gate){if(Fseek(handle,saved.Offset,0)<0)return -1;entry.DecodeState=saved.State;entry.DecodeValue=saved.Value;return 0;}
    }
}
