#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
namespace Edg.Runtime;

// Portable bounded locale provider. No managed ambient culture or OS locale ABI.
public sealed unsafe class LibcLocale : IDisposable
{
    private sealed class Locale {public int Categories,Users;public Locale(int categories)=>Categories=categories;}
    private sealed class Selection {public nint Handle=-1;public Locale? Locale;}
    [StructLayout(LayoutKind.Sequential,Size=96)]
    private struct LConv {public sbyte* DecimalPoint,ThousandsSep,Grouping,IntCurrSymbol,CurrencySymbol,MonDecimalPoint,MonThousandsSep,MonGrouping,PositiveSign,NegativeSign;public fixed sbyte Monetary[14];}
    private readonly object gate=new();
    private readonly Action<int> setErrno;
    private readonly Dictionary<nint,Locale> handles=new();
    private readonly ThreadLocal<Selection?> thread=new(()=>null,true);
    private readonly int[] offsets=new int[64];
    private byte[]? strings;
    private LConv[]? convention;
    private int categories;
    private bool disposed;
    public LibcLocale(Action<int> setErrno)
    {
        this.setErrno=setErrno;
        string[] names=new string[64];int size=2;
        for(int mask=0;mask<64;++mask) {
            if(mask==0)names[mask]="C";else if(mask==63)names[mask]="C.UTF-8";
            else {string[] parts=new string[6];for(int i=0;i<6;++i)parts[i]=(mask&(1<<i))==0?"C":"C.UTF-8";names[mask]=string.Join(';',parts);}
            offsets[mask]=size;size+=names[mask].Length+1;
        }
        strings=GC.AllocateArray<byte>(size,pinned:true);strings[0]=(byte)'.';
        for(int i=0;i<64;++i)Encoding.ASCII.GetBytes(names[i],strings.AsSpan(offsets[i]));
        convention=GC.AllocateArray<LConv>(1,pinned:true);
        fixed(byte* bytes=strings)fixed(LConv* value=convention) {
            value->DecimalPoint=(sbyte*)bytes;
            sbyte* empty=(sbyte*)bytes+1;
            value->ThousandsSep=value->Grouping=value->IntCurrSymbol=value->CurrencySymbol=value->MonDecimalPoint=value->MonThousandsSep=value->MonGrouping=value->PositiveSign=value->NegativeSign=empty;
            for(int i=0;i<14;++i)value->Monetary[i]=127;
        }
    }
    private void Alive() {if(disposed)throw new ObjectDisposedException(nameof(LibcLocale));}
    private Selection Current {get {Alive();return thread.Value??=new Selection();}}
    public bool Utf8 {get {var selected=Current;return ((selected.Locale?.Categories??Volatile.Read(ref categories))&1)!=0;}}
    private static int Name(sbyte* p,int length)
    {
        if(length==1 && p[0]=='C')return 0;
        if(length==5 && p[0]=='P'&&p[1]=='O'&&p[2]=='S'&&p[3]=='I'&&p[4]=='X')return 0;
        if(length==7 && p[0]=='C'&&p[1]=='.'&&p[2]=='U'&&p[3]=='T'&&p[4]=='F'&&p[5]=='-'&&p[6]=='8')return 1;
        if(length==6 && p[0]=='C'&&p[1]=='.'&&p[2]=='u'&&p[3]=='t'&&p[4]=='f'&&p[5]=='8')return 1;
        return -1;
    }
    private static int Parse(sbyte* text,bool composite)
    {
        if(text==null)return -1;
        int length=0;while(text[length]!=0 && text[length]!=';' && length<8)++length;
        int first=Name(text,length);if(first<0)return -1;
        if(text[length]==0)return first==0?0:63;
        if(!composite)return -1;
        int mask=0;for(int category=0;category<6;++category) {
            length=0;while(text[length]!=0 && text[length]!=';' && length<8)++length;
            int name=Name(text,length);if(name<0)return -1;mask|=name<<category;
            if(category==5)return text[length]==0?mask:-1;
            if(text[length]!=';')return -1;text+=length+1;
        }
        return -1;
    }
    private sbyte* NamePointer(int mask) {fixed(byte* p=strings!)return (sbyte*)p+offsets[mask];}
    public sbyte* SetLocale(int category,sbyte* name)
    {
        lock(gate) {
            Alive();if((uint)category>6) {setErrno(22);return null;}
            if(name!=null) {
                int parsed=Parse(name,category==6);if(parsed<0) {setErrno(22);return null;}
                int mask=category==6?63:1<<category;
                Volatile.Write(ref categories,(categories&~mask)|(parsed&mask));
            }
            return NamePointer(category==6?categories:(categories&(1<<category))==0?0:63);
        }
    }
    private Locale? Find(void* handle)
    {
        if(handles.TryGetValue((nint)handle,out var value))return value;
        setErrno(22);return null;
    }
    private void* Allocate(int mask)
    {
        void* handle=null;
        try {handle=NativeMemory.Alloc(1);if(handle==null){setErrno(12);return null;}handles.Add((nint)handle,new Locale(mask));return handle;}
        catch(OutOfMemoryException){NativeMemory.Free(handle);setErrno(12);return null;}
    }
    public void* NewLocale(int mask,sbyte* name,void* basis)
    {
        lock(gate) {
            Alive();if((mask&~63)!=0 || basis==(void*)(nint)(-1)) {setErrno(22);return null;}
            Locale? old=basis==null?null:Find(basis);if(basis!=null && old==null)return null;
            if(old?.Users>0){setErrno(16);return null;}
            int parsed=mask==0?0:Parse(name,false);if(parsed<0){setErrno(name==null?22:2);return null;}
            int result=((old?.Categories??0)&~mask)|(parsed&mask);
            if(old!=null){old.Categories=result;return basis;}
            return Allocate(result);
        }
    }
    public void* DupLocale(void* source)
    {
        lock(gate) {
            Alive();if(source==(void*)(nint)(-1))return Allocate(categories);
            var original=Find(source);return original==null?null:Allocate(original.Categories);
        }
    }
    public void FreeLocale(void* handle)
    {
        lock(gate) {
            Alive();var value=Find(handle);if(value==null)return;
            if(value.Users!=0){setErrno(16);return;}
            // Release valid locale resources immediately. Reusing an invalid
            // freed pointer after allocator address reuse remains undefined.
            handles.Remove((nint)handle);NativeMemory.Free(handle);
        }
    }
    public void* UseLocale(void* handle)
    {
        lock(gate) {
            Alive();var selected=Current;nint old=selected.Handle;if(handle==null)return (void*)old;
            Locale? value=handle==(void*)(nint)(-1)?null:Find(handle);
            if(handle!=(void*)(nint)(-1) && value==null)return null;
            if(selected.Locale!=null)--selected.Locale.Users;
            selected.Locale=value;selected.Handle=(nint)handle;if(value!=null)++value.Users;
            return (void*)old;
        }
    }
    public int Validate(void* handle) {lock(gate){Alive();return Find(handle)==null?0:1;}}
    public void* LocaleConv() {lock(gate){Alive();fixed(LConv* p=convention!)return p;}}
    public void ReleaseThread()
    {
        lock(gate) {if(disposed || !thread.IsValueCreated)return;var selected=thread.Value;if(selected?.Locale!=null)--selected.Locale.Users;thread.Value=null;}
    }
    public void Dispose()
    {
        lock(gate) {
            if(disposed)return;disposed=true;
            foreach(var handle in handles.Keys)NativeMemory.Free((void*)handle);handles.Clear();
            thread.Dispose();strings=null;convention=null;
        }
    }
}
