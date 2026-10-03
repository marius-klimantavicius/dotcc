using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Edg.Runtime;

unsafe class Program
{
    struct Payload { public void* Next;public int Value; }
    static int resumes,destroys;
    static void Check(bool result,string message){if(!result)throw new Exception(message);}
    static Payload* Data(void* frame)=>(Payload*)((byte*)frame+sizeof(Coroutine.Frame)+sizeof(void*));
    static void* Allocate(Coroutine.Registry registry,delegate* managed<void*,void*> resume)
    {
        nuint size=(nuint)(sizeof(Coroutine.Frame)+sizeof(void*)+sizeof(Payload));
        void* address=NativeMemory.AllocZeroed(size);
        try {registry.Attach((Coroutine.Frame*)address,Data(address),size,resume,&Destroy);return address;}
        catch {NativeMemory.Free(address);throw;}
    }
    static void* Resume(void* address)
    {
        ++resumes;var frame=(Coroutine.Frame*)address;++Data(address)->Value;
        if(++frame->State==2)frame->Resume=null;
        return null;
    }
    static void* Transfer(void* address)
    {
        ++resumes;((Coroutine.Frame*)address)->Resume=null;return Data(address)->Next;
    }
    static void* DestroySelf(void* address)
    {
        void* next=Data(address)->Next;Destroy(address);return next;
    }
    static void Destroy(void* address)
    {
        ++destroys;Coroutine.Detach(address);NativeMemory.Free(address);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void* Rooted(out WeakReference reference)
    {
        object owner=new();reference=new WeakReference(owner);
        return Allocate(new Coroutine.Registry(owner),&Resume);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Collect(){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
    static void Positive()
    {
        var registry=new Coroutine.Registry(null);
        void* a=Allocate(registry,&Resume),b=Allocate(registry,&Resume),c=Allocate(registry,&Resume);
        Check(Coroutine.Promise(a,8,false)==Data(a),"promise lookup");
        Check(Coroutine.Promise(Data(a),8,true)==a,"from_promise");
        Check(!Coroutine.Done(a),"initial suspension");
        Coroutine.Resume(a);Check(!Coroutine.Done(a) && Data(a)->Value==1,"first suspension");
        Coroutine.Resume(a);Check(Coroutine.Done(a) && Data(a)->Value==2,"final suspension");
        Coroutine.Destroy(b);Coroutine.Destroy(a);Coroutine.Destroy(c);
        const int count=10000;
        void** chain=(void**)NativeMemory.Alloc((nuint)count,(nuint)sizeof(void*));
        for(int i=0;i<count;++i)chain[i]=Allocate(registry,&Transfer);
        for(int i=0;i<count-1;++i)Data(chain[i])->Next=chain[i+1];
        int prior=resumes;long bytes=GC.GetAllocatedBytesForCurrentThread();Coroutine.Resume(chain[0]);
        Check(resumes-prior==count && GC.GetAllocatedBytesForCurrentThread()==bytes,"allocation-free iterative symmetric transfer");
        for(int i=0;i<count;++i)Coroutine.Destroy(chain[i]);NativeMemory.Free(chain);
        void* next=Allocate(registry,&Resume),self=Allocate(registry,&DestroySelf);Data(self)->Next=next;
        Coroutine.Resume(self);Check(Data(next)->Value==1,"self-destruction before transfer");Coroutine.Destroy(next);
        void* rooted=Rooted(out var weak);Collect();Check(weak.IsAlive,"suspended frame did not root owner");
        Coroutine.Destroy(rooted);Collect();Check(!weak.IsAlive,"destroy did not release owner root");
        int before=destroys;void* leaked=Allocate(registry,&Resume);registry.Abandon();registry.Abandon();
        Check(destroys==before,"abandon invented source destruction");NativeMemory.Free(leaked);
        void* noop=Coroutine.Noop();
        Check(noop!=null && !Coroutine.Done(noop) && Coroutine.Owner(noop)==null,"noop observers");
        void* noopPromise=Coroutine.Promise(noop,1,false);
        Check(Coroutine.Promise(noopPromise,1,true)==noop,"noop promise roundtrip");
        Coroutine.Resume(noop);Coroutine.Destroy(noop);Coroutine.Resume(noop);
        Collect();Check(noop==Coroutine.Noop() && !Coroutine.Done(noop),"noop global storage lifetime");
        Console.WriteLine("coroutine frame runtime checks passed");
    }
    static void Main(string[] args)
    {
        if(args.Length!=0)
        {
            var registry=new Coroutine.Registry(null);void* address=Allocate(registry,&Resume);
            if(args[0]=="done"){Coroutine.Resume(address);Coroutine.Resume(address);Coroutine.Resume(address);}
            if(args[0]=="abandoned"){registry.Abandon();Coroutine.Resume(address);}
            if(args[0]=="null")Coroutine.Resume(null);
            throw new Exception("invalid coroutine operation returned");
        }
        Positive();
    }
}
