using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Edg.Runtime;
unsafe class Program
{
    static void Check(bool condition,string text) { if(!condition)throw new Exception(text); }
    static string Decode(sbyte* pointer) { int n=0;while(pointer[n]!=0)++n;return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(pointer,n)); }
    static void Main(string[] args)
    {
        using var errors=new LibcContext();using var otherErrors=new LibcContext();
        using var text=new LibcText(errors.SetErrno);using var other=new LibcText(otherErrors.SetErrno);
        Check(Decode(text.SetLocale(0,null))=="C" && text.MbCurMax()==1,"initial C locale");
        sbyte* message=text.Strerror(22);Check(Decode(message)=="Invalid argument","known error");
        Check(text.Strerror(-1)==text.Strerror(int.MaxValue) && Decode(text.Strerror(-1))=="No error information","unknown error");
        Check(message!=other.Strerror(22),"owner error storage alias");
        errors.SetErrno(17);text.Strerror(12);Check(*errors.ErrnoLocation()==17,"strerror changed errno");
        GC.Collect(2,GCCollectionMode.Forced,true,true);Check(message==text.Strerror(22) && Decode(message)=="Invalid argument","pinned error moved");
        byte* destination=stackalloc byte[4];
        for(int c=0;c<128;++c)Check(text.Wctomb((sbyte*)destination,c)==1 && destination[0]==c,"C ASCII");
        for(int c=0xdf80;c<=0xdfff;++c)Check(text.Wctomb((sbyte*)destination,c)==1 && destination[0]==(byte)c,"musl C byte escape");
        destination[0]=0xa5;Check(text.Wctomb((sbyte*)destination,0xe9)==-1 && destination[0]==0xa5 && *errors.ErrnoLocation()==84,"C rejects nonbyte character");
        Check(text.Wctomb(null,int.MinValue)==0,"state reset");
        for(int i=-1;i<=255;++i)Check(LibcText.IsDigit(i)==(i>=48 && i<=57?1:0),"ASCII digit");
        Check(LibcText.IsDigit(int.MinValue)==0 && LibcText.IsDigit(int.MaxValue)==0,"digit range");
        fixed(byte* locale="C.UTF-8\0"u8)Check(Decode(text.SetLocale(6,(sbyte*)locale))=="C.UTF-8","select UTF8");
        Check(text.MbCurMax()==4 && other.MbCurMax()==1,"owner locale isolation");
        fixed(byte* locale="unsupported\0"u8)Check(text.SetLocale(0,(sbyte*)locale)==null && text.MbCurMax()==4,"failed locale selection is atomic");
        fixed(byte* locale="\0"u8)Check(text.SetLocale(0,(sbyte*)locale)==null,"environment locale not silently accepted");
        Check(text.SetLocale(99,null)==null,"unsupported category");
        foreach(int invalid in new[]{-1,int.MinValue,0xd800,0xdfff,0x110000,int.MaxValue}) {
            destination[0]=0xa5;errors.SetErrno(0);
            Check(text.Wctomb((sbyte*)destination,invalid)==-1 && destination[0]==0xa5 && *errors.ErrnoLocation()==84,"musl/BCL Unicode scalar bound");
        }
        int count=0;
        foreach(string line in File.ReadLines(args[0])) {
            string[] p=line.Split(' ');int character=int.Parse(p[0]),expected=int.Parse(p[1]);
            for(int i=0;i<4;++i)destination[i]=0xa5;errors.SetErrno(0);
            int actual=text.Wctomb((sbyte*)destination,character);
            Check(actual==expected,line);
            if(actual<0)Check(*errors.ErrnoLocation()==84,line);else Check(*errors.ErrnoLocation()==0,line);
            for(int i=0;i<4;++i)Check(destination[i]==byte.Parse(p[3].AsSpan(i*2,2),NumberStyles.HexNumber,CultureInfo.InvariantCulture),line);
            ++count;
        }
        errors.SetErrno(31);Exception? failure=null;
        var thread=new Thread(()=>{try { errors.SetErrno(72);byte* temp=stackalloc byte[4];Check(text.Wctomb((sbyte*)temp,0xd800)==-1 && *errors.ErrnoLocation()==84,"thread conversion errno");Check(text.Strerror(22)==message,"immutable error pool shared within owner"); }catch(Exception e){failure=e;}});
        thread.Start();thread.Join();if(failure!=null)throw failure;Check(*errors.ErrnoLocation()==31,"thread errno isolation");
        for(int i=0;i<1000;++i)text.Wctomb((sbyte*)destination,0x10ffff);
        long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10000;++i)text.Wctomb((sbyte*)destination,0x10ffff);
        Check(GC.GetAllocatedBytesForCurrentThread()==before,"per-character allocation");
        text.Dispose();bool disposed=false;try{text.Strerror(22);}catch(ObjectDisposedException){disposed=true;}Check(disposed,"use after dispose");
        Check(Decode(other.Strerror(22))=="Invalid argument","other owner survived disposal");
        Console.WriteLine($"{count} UTF-8 native vectors passed; C byte escapes, owner/errno isolation, pinned messages and allocation checks passed");
    }
}
