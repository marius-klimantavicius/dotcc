using System;
using System.IO;
using System.Text;
using System.Threading;
using Edg.Runtime;
unsafe class Owner:IDisposable
{
    public readonly MemoryStream Output=new();
    public readonly LibcFiles Files;
    public readonly ManualResetEventSlim First=new(),Resume=new();
    public Exception? Error;
    public int Errno;
    public Owner(){Files=new LibcFiles(e=>Errno=e,output:Output);Files.Setvbuf(Files.Stdout,null,2,0);}
    public void Dispose(){Files.Dispose();First.Dispose();Resume.Dispose();Output.Dispose();}
}
unsafe class Program
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static int Static(void* context,sbyte* format,VaList* args) { Check((nuint)context==17 && format==null && args==null,"static signature");return 23; }
    static int Instance(Owner owner,void* context,sbyte* format,VaList* args)
    {
        Check(owner.Files.Fputc('A',owner.Files.Stdout)=='A',"first chunk");owner.First.Set();
        Check(owner.Resume.Wait(5000),"resume timeout");
        Check(owner.Files.Fputc('B',owner.Files.Stdout)=='B',"second chunk");return 2;
    }
    static int Throw(Owner owner,void* context,sbyte* format,VaList* args)=>throw new InvalidOperationException("formatter failure");
    static int Nested(Owner owner,void* context,sbyte* format,VaList* args)=>owner.Files.WithFileLock(owner.Files.Stdout,&Static,(void*)17,null,null);
    static int Fast(Owner owner,void* context,sbyte* format,VaList* args)=>19;
    static void Main()
    {
        using var owner=new Owner();using var other=new Owner();
        Check(owner.Files.WithFileLock(owner.Files.Stdout,&Static,(void*)17,null,null)==23,"static callback");
        Check(owner.Files.WithFileLock(owner.Files.Stdout,&Nested,owner,null,null,null)==23,"nested reentrant callback");
        Check(owner.Files.WithFileLock(other.Files.Stdout,&Static,(void*)17,null,null)==-1 && owner.Errno==9,"foreign owner file");
        Check(owner.Files.WithFileLock(owner.Files.Stdout,(delegate* managed<void*,sbyte*,VaList*,int>)null,null,null,null)==-1 && owner.Errno==22,"null callback");
        var formatThread=new Thread(()=>{try{Check(owner.Files.WithFileLock(owner.Files.Stdout,&Instance,owner,null,null,null)==2,"format result");}catch(Exception e){owner.Error=e;}});
        int attempted=0,completed=0;
        var rival=new Thread(()=>{Interlocked.Exchange(ref attempted,1);try{owner.Files.Fputc('X',owner.Files.Stdout);}catch(Exception e){owner.Error=e;}finally{Interlocked.Exchange(ref completed,1);}});
        formatThread.Start();Check(owner.First.Wait(5000),"first chunk timeout");rival.Start();
        try {
            Check(SpinWait.SpinUntil(()=>Volatile.Read(ref attempted)!=0,5000),"rival start timeout");
            Check(SpinWait.SpinUntil(()=>Volatile.Read(ref completed)!=0 || (rival.ThreadState & ThreadState.WaitSleepJoin)!=0,5000),"rival monitor timeout");
            Check(Volatile.Read(ref completed)==0,"rival wrote between chunks");
        } finally {owner.Resume.Set();}
        Check(formatThread.Join(5000)&&rival.Join(5000),"worker completion timeout");if(owner.Error!=null)throw owner.Error;
        Check(Encoding.ASCII.GetString(owner.Output.ToArray())=="ABX","whole-operation stream lock");
        bool caught=false;try{owner.Files.WithFileLock(owner.Files.Stdout,&Throw,owner,null,null,null);}catch(InvalidOperationException){caught=true;}Check(caught,"foreign exception propagation");
        var after=new Thread(()=>{try{Check(owner.Files.WithFileLock(owner.Files.Stdout,&Static,(void*)17,null,null)==23,"post-exception callback");}catch(Exception e){owner.Error=e;}});
        after.Start();Check(after.Join(5000),"exception leaked monitor");if(owner.Error!=null)throw owner.Error;
        for(int i=0;i<1000;++i)owner.Files.WithFileLock(owner.Files.Stdout,&Fast,owner,null,null,null);
        long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10000;++i)owner.Files.WithFileLock(owner.Files.Stdout,&Fast,owner,null,null,null);
        Check(before==GC.GetAllocatedBytesForCurrentThread(),"session callback allocation");
        Console.WriteLine("static/generic FILE lock callbacks, deterministic noninterleaving, reentry, foreign exception cleanup and zero allocation passed");
    }
}
