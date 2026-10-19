using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using L = DotCC.Libc.Libc;

internal static unsafe class Program
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Host { public byte* Name; public byte** Aliases; public int Family, Length; public byte** Addresses; }
    private static int assertions;
    private static void Check(bool ok, string description)
    { Interlocked.Increment(ref assertions); if (!ok) throw new InvalidOperationException(description); }
    private static string Text(byte* p) => Marshal.PtrToStringUTF8((nint)p)!;
    private static void Address(byte* p, string text, int port)
    {
        new Span<byte>(p,128).Clear();
        var ip=IPAddress.Parse(text); *(ushort*)p=(ushort)(ip.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork?2:10);
        p[2]=(byte)(port>>8); p[3]=(byte)port;
        ip.GetAddressBytes().CopyTo(new Span<byte>(p+(*(ushort*)p==2?4:8), *(ushort*)p==2?4:16));
        if (*(ushort*)p==10) *(uint*)(p+24)=(uint)ip.ScopeId;
    }
    private static void Contract(string path)
    {
        byte* a=stackalloc byte[128]; byte* h=stackalloc byte[128]; byte* s=stackalloc byte[64];
        delegate*<void*,uint,byte*,uint,byte*,uint,int,int> reverse=&L.getnameinfo;
        delegate*<byte*,void*> lookup=&L.gethostbyname;
        foreach(string line in File.ReadAllLines(path))
        {
            string[] v=line.Split(' ');
            if(v[0]=="NI")
            {
                string id=v[1]; uint al=16,hl=120,sl=60; int flags=3,omit=0;
                Address(a,"127.0.0.1",80);
                if(id.StartsWith("flags-")) flags=int.Parse(id[6..],CultureInfo.InvariantCulture);
                if(id.StartsWith("host-")) hl=uint.Parse(id[5..]);
                if(id.StartsWith("serv-")) sl=uint.Parse(id[5..]);
                if(id=="omit-host") omit=1; if(id=="omit-serv") omit=2; if(id=="omit-both") omit=3;
                if(id.StartsWith("zero-")) { hl=sl=0; if(id=="zero-namereqd") flags=8; }
                if(id=="short") al=15; if(id=="long") al=128; if(id=="family") *(ushort*)a=99;
                if(id.StartsWith("no-ptr-")) { Address(a,"192.0.2.254",80); flags=id=="no-ptr-required"?8:0; }
                if(id.StartsWith("port-")) { var parts=id.Split('-'); Address(a,"192.0.2.254",int.Parse(parts[1])); flags=1|(parts[2]=="1"?16:0); }
                if(id.StartsWith("v6-")) { Address(a,id=="v6-scope"?"fe80::abcd%12345":"::1",443); al=28; if(id=="v6-scope") *(uint*)(a+4)=0x12345678; }
                new Span<byte>(h,128).Fill(0xa5); new Span<byte>(s,64).Fill(0xa5); L.errno=123;
                int r=reverse(a,al,(omit&1)!=0?null:h+1,hl,(omit&2)!=0?null:s+1,sl,flags);
                string actual=$"NI {id} {r} {L.errno} {Convert.ToHexStringLower(new ReadOnlySpan<byte>(h,30))} {Convert.ToHexStringLower(new ReadOnlySpan<byte>(s,30))}";
                Check(actual==line,$"native mismatch\nexpected {line}\nactual   {actual}");
                Check(h[0]==0xa5 && h[127]==0xa5 && s[0]==0xa5 && s[63]==0xa5,"buffer guards "+id);
            }
            if(v[0]=="HOST")
            {
                byte[] name=Encoding.UTF8.GetBytes(v[1]+"\0"); L.errno=123; L.h_errno=99;
                Host* result; fixed(byte* p=name) result=(Host*)lookup(p);
                string actual=$"HOST {v[1]} {(result!=null?1:0)} {L.h_errno} {L.errno}";
                if(result!=null)
                {
                    actual+=$" {Text(result->Name)} {result->Family} {result->Length}";
                    int count=0;
                    for(byte** p=result->Aliases;*p!=null;p++) { Check(++count<100,"aliases terminated"); actual+=" alias="+Text(*p); }
                    count=0;
                    for(byte** p=result->Addresses;*p!=null;p++) { Check(++count<100,"addresses terminated"); actual+=" addr="+Convert.ToHexStringLower(new ReadOnlySpan<byte>(*p,result->Length)); }
                }
                Check(actual==line,$"native mismatch\nexpected {line}\nactual   {actual}");
            }
        }
    }
    private static Host* Lookup(string name)
    { fixed(byte* p=Encoding.UTF8.GetBytes(name+"\0")) return (Host*)L.gethostbyname(p); }
    private static void Ownership()
    {
        using var first=new L.RuntimeContext(); using var second=new L.RuntimeContext();
        L.ResolverStorage? storage=null;
        using(first.Enter())
        {
            L.errno=71; L.h_errno=91; var h=Lookup("127.0.0.1"); Check(h!=null,"lookup");
            storage=first.ThreadState.Resolver; nint original=(nint)h;
            Check(storage!.Allocation==original,"borrowed storage holder");
            string name=Text(h->Name); L.h_errno=81; L.errno=72;
            using(second.Enter())
            {
                Check(L.h_errno==0 && L.errno==0,"new context error state");
                var other=Lookup("127.0.0.2"); Check(other!=null && (nint)other!=original,"independent contexts");
                L.h_errno=82; L.errno=73; first.DisposeMustReject();
            }
            Check(L.h_errno==81 && L.errno==72 && Text(h->Name)==name,"context restoration");
            Check(Lookup("invalid!host")==null && L.h_errno==3,"real failure state");
            Check(storage.Allocation==0 && storage.Capacity==0,"failure invalidates borrowed allocation");
            Check(Lookup(new string('a',6000))==null && storage.Allocation==0,"growth then error cleanup");
            Check(Lookup("127.0.0.3")!=null,"recreate after failure");
        }
        using(second.Enter()) first.Dispose();
        Check(storage!.Allocation==0,"deterministic foreign-ambient owner disposal"); first.Dispose();
        using var third=new L.RuntimeContext(); using(third.Enter()) Check(Lookup("127.0.0.4")!=null,"fresh context after disposal");
        foreach(bool sameOwner in new[]{false,true}) Workers(sameOwner);
        var weak=RetiredThread();
        for(int i=0;i<12 && weak.IsAlive;i++) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
        Check(!weak.IsAlive,"retired thread borrowed holder finalized");
        GC.KeepAlive(retiredOwner);
        retiredOwner!.Dispose(); retiredOwner=null;
    }
    private static void DisposeMustReject(this L.RuntimeContext c)
    { try { c.Dispose(); throw new InvalidOperationException("active owner disposal accepted"); } catch(InvalidOperationException e) { Check(e.Message.Contains("active bindings"),"disposal rejects active owner"); } }
    private static void Workers(bool same)
    {
        using var first=new L.RuntimeContext(); using var second=new L.RuntimeContext(); using var barrier=new Barrier(2);
        var errors=new Exception?[2]; var pointers=new nint[2]; var holders=new L.ResolverStorage?[2];
        Thread[] ts=new Thread[2];
        for(int index=0;index<2;index++)
        {
            int i=index; ts[i]=new Thread(()=>
            {
                try
                {
                    using var bind=(same||i==0?first:second).Enter();
                    var h=Lookup(i==0?"127.0.0.11":"127.0.0.12"); Check(h!=null,"worker resolves"); pointers[i]=(nint)h;
                    holders[i]=L.RuntimeContext.Current!.ThreadState.Resolver;
                    L.h_errno=100+i; L.errno=200+i;
                    Check(barrier.SignalAndWait(TimeSpan.FromSeconds(10)),"simultaneous worker barrier");
                    Check(L.h_errno==100+i && L.errno==200+i,"worker error isolation");
                    Check(Text(h->Name)==(i==0?"127.0.0.11":"127.0.0.12"),"worker pointer lifetime");
                    GC.Collect(2,GCCollectionMode.Forced,true,true);
                    Check(Text(h->Name).Length>0,"stable native pointer through compacting GC");
                } catch(Exception e) { errors[i]=e; }
            }); ts[i].Start();
        }
        foreach(var t in ts) Check(t.Join(TimeSpan.FromSeconds(20)),"worker joined");
        foreach(var e in errors) if(e!=null) throw e;
        Check(pointers[0]!=pointers[1],"worker storage independence");
        first.Dispose(); second.Dispose();
        foreach(var h in holders) Check(h!.Allocation==0,"worker storage reclaimed by owner");
        GC.KeepAlive(ts);
    }
    private static L.RuntimeContext? retiredOwner;
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RetiredThread()
    {
        retiredOwner=new L.RuntimeContext(); WeakReference? weak=null;
        var t=new Thread(()=> { using var bind=retiredOwner.Enter(); Check(Lookup("127.0.0.19")!=null,"retiring worker lookup"); weak=new WeakReference(retiredOwner.ThreadState.Resolver); });
        t.Start(); Check(t.Join(TimeSpan.FromSeconds(10)),"retired thread join"); return weak!;
    }
    private static void ForwardOwnership()
    {
        using var owner=new L.RuntimeContext(); using var other=new L.RuntimeContext(); void* list=null;
        using(owner.Enter())
        {
            byte* hint=stackalloc byte[48]; new Span<byte>(hint,48).Clear(); *(int*)hint=4|1024|2; *(int*)(hint+8)=1;
            fixed(byte* host="127.0.0.1\0"u8) fixed(byte* service="443\0"u8)
                Check(L.getaddrinfo(host,service,hint,&list)==0,"forward numeric");
            Check(owner.NativeAllocations.Contains((nuint)list),"existing heap owns addrinfo node");
            Check(*(uint*)((byte*)list+16)==16 && Text(*(byte**)((byte*)list+32))=="127.0.0.1","native layout and canonical bytes");
            Check(*(nint*)((byte*)list+40)==0,"terminal next");
            using(other.Enter())
            {
                try { L.freeaddrinfo(list); throw new Exception("foreign free accepted"); } catch(InvalidOperationException) { Check(true,"foreign free rejected before read"); }
            }
            L.freeaddrinfo(list); Check(owner.NativeAllocations.Count==0,"all forward nodes released");
            try { L.freeaddrinfo(list); throw new Exception("repeat free accepted"); } catch(InvalidOperationException) { Check(true,"repeat free rejected before read"); }
            L.freeaddrinfo(null);
            fixed(byte* host="127.0.0.1\0"u8) fixed(byte* service="exec\0"u8)
                Check(L.getaddrinfo(host,service,hint,&list)==-2,"numeric service flag forbids name");
            Check(list==null && owner.NativeAllocations.Count==0,"error output and allocation cleanup");
            *(int*)hint=4; fixed(byte* host="127.0.0.1\0"u8) fixed(byte* service="exec\0"u8)
                Check(L.getaddrinfo(host,service,hint,&list)==0,"real NSS service forward");
            Check(((byte*)(*(byte**)((byte*)list+24)))[2]==2,"exec port512");
        }
        owner.Dispose(); Check(owner.NativeAllocations.Count==0,"owner reclaims abandoned addrinfo");
    }
    private static void NumericForms(string path)
    {
        foreach(string line in File.ReadAllLines(path))
        {
            if(!line.StartsWith("NUM [")) continue;
            int end=line.IndexOf(']'); string name=line[5..end]; string[] v=line[(end+2)..].Split(' ');
            L.errno=123; L.h_errno=99; var h=Lookup(name);
            Check((h!=null?1:0)==int.Parse(v[0]) && L.h_errno==int.Parse(v[1]) && L.errno==int.Parse(v[2]),"numeric native contract "+line+" actual="+(h!=null)+","+L.h_errno+","+L.errno);
            if(h!=null) Check(Text(h->Name)==v[4] && h->Family==int.Parse(v[5]) && h->Length==int.Parse(v[6]),"numeric hostent "+name);
        }
    }
    private static void ForwardContract(string path)
    {
        using var owner=new L.RuntimeContext(); using var binding=owner.Enter();
        byte* hints=stackalloc byte[48];
        foreach(string line in File.ReadAllLines(path))
        {
            string[] v=line.Split(' '); new Span<byte>(hints,48).Clear();
            for(int i=0;i<4;i++) ((int*)hints)[i]=int.Parse(v[4+i]);
            void* result=null; L.errno=123;
            fixed(byte* host=Encoding.UTF8.GetBytes(v[2]+"\0")) fixed(byte* service=Encoding.UTF8.GetBytes(v[3]+"\0"))
            {
                int e=L.getaddrinfo(v[2]=="-"?null:host,v[3]=="-"?null:service,hints,&result);
                string actual=string.Join(' ',v.Take(8))+$" {e} {L.errno}";
                try
                {
                    int count=0;
                    for(byte* p=(byte*)result;p!=null;p=*(byte**)(p+40))
                    {
                        Check(++count<100,"forward linked list termination");
                        Check(owner.NativeAllocations.Contains((nuint)p),"each node tracked by existing heap");
                        uint len=*(uint*)(p+16); byte* canon=*(byte**)(p+32);
                        actual+=$" node={*(int*)p},{*(int*)(p+4)},{*(int*)(p+8)},{*(int*)(p+12)},{len},{(canon==null?"-":Text(canon))},{Convert.ToHexStringLower(new ReadOnlySpan<byte>(*(byte**)(p+24),(int)len))}";
                    }
                    Check(actual==line,$"forward native mismatch\nexpected {line}\nactual   {actual}");
                }
                finally { L.freeaddrinfo(result); }
                Check(owner.NativeAllocations.Count==0,"complete forward free");
            }
        }
        var storage=new L.ResolverStorage(); storage.Allocate(1024);
        try { storage.Allocate(nuint.MaxValue-64); throw new Exception("impossible allocation accepted"); }
        catch(OutOfMemoryException) { Check(storage.Allocation==0,"allocation failure cleans prior borrowed storage"); }
        finally { storage.Dispose(); }
    }
    private static int Main(string[] args)
    {
        if(args.Length==1 && args[0]=="--clone-oom") return CloneFailure.Run();
        try { Contract(args[0]); NumericForms(args[1]); ForwardContract(args[2]); Ownership(); ForwardOwnership(); Console.WriteLine($"resolver assertions={assertions} workers=4 retired=1"); return 0; }
        catch(Exception e) { Console.Error.WriteLine(e); return 1; }
    }
}
