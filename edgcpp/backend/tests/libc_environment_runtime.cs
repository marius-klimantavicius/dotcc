using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using Edg.Runtime;
unsafe class Program
{
    static string Read(sbyte* p)=>Marshal.PtrToStringUTF8((nint)p)!;
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    [MethodImpl(MethodImplOptions.NoInlining)]
    static long ReadLoop(LibcEnvironment environment,sbyte** expected)
    {
        long before=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<10000;++i)if(environment.Snapshot()!=expected)throw new Exception("snapshot changed");
        return GC.GetAllocatedBytesForCurrentThread()-before;
    }
    static void Main()
    {
        var seed=new[]{new KeyValuePair<string,string>("TZ","Synthetic/One"),new KeyValuePair<string,string>("UNICODE","α😀"),new KeyValuePair<string,string>("EMPTY",""),new KeyValuePair<string,string>("TZ","duplicate")};
        using var first=new LibcEnvironment(seed);using var second=new LibcEnvironment(seed);
        sbyte** table=first.Snapshot();sbyte* name=table[1];
        Require(table!=second.Snapshot() && name!=second.Snapshot()[1],"owner snapshot alias");
        Require(Read(table[0])=="TZ=Synthetic/One" && Read(table[1])=="UNICODE=α😀" && Read(table[2])=="EMPTY=" && Read(table[3])=="TZ=duplicate" && table[4]==null,"UTF8 initial entries");
        GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
        Require(table==first.Snapshot() && name==table[1] && Read(name)=="UNICODE=α😀","global pointers moved");
        ReadLoop(first,table);long allocated=ReadLoop(first,table);Require(allocated==0,$"snapshot getter allocates {allocated} bytes");
        using(var empty=new LibcEnvironment(Array.Empty<KeyValuePair<string,string>>()))Require(empty.Snapshot()!=null && empty.Snapshot()[0]==null,"empty snapshot");
        bool rejected=false;try{using var bad=new LibcEnvironment(new[]{new KeyValuePair<string,string>("A","bad\0value")});}catch(ArgumentException){rejected=true;}Require(rejected,"NUL accepted");
        rejected=false;try{using var bad=new LibcEnvironment(new[]{new KeyValuePair<string,string>("A","\ud800")});}catch(EncoderFallbackException){rejected=true;}Require(rejected,"invalid UTF16 silently replaced");
        first.Dispose();first.Dispose();rejected=false;try{first.Snapshot();}catch(ObjectDisposedException){rejected=true;}Require(rejected,"disposed snapshot exposed");Require(Read(second.Snapshot()[0])=="TZ=Synthetic/One","other owner disposed");
        Console.WriteLine("independent pinned environment snapshots, exact UTF8, GC, allocation-free reads and disposal passed");
    }
}
