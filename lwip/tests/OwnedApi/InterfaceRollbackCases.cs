using Managed.Networking;
using System.Runtime.InteropServices;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    static int attachmentCallbacks;
    static void PreAttachOutput(Lwip owner,netif*iface,ushort reason,netif_ext_callback_args_t*arguments)
    {
        if((reason&LWIP_NSC_NETIF_ADDED)==0)return;
        Interlocked.Increment(ref attachmentCallbacks);
        LwipFrames.Attach(owner,(nint)iface,static _=>{});
    }
    static void InterfaceRollbackCases()
    {
        using var stack=new LwipStack();stack.Start();
        nint healthy=stack.AddInterface("10.9.1.1","",new byte[]{2,0,0,0,9,1},static _=>{});
        var callback=(netif_ext_callback*)NativeMemory.AllocZeroed((nuint)sizeof(netif_ext_callback));
        bool registered=false;uint memory=0,pbufs=0;attachmentCallbacks=0;
        try
        {
            stack.Invoke(core=>{memory=core.Globals.lwip_stats.mem.used;pbufs=core.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used;core.netif_add_ext_callback(callback,&PreAttachOutput);registered=true;});
            bool rejected=false;
            try{stack.AddInterface("10.9.1.2","",new byte[]{2,0,0,0,9,2},static _=>{});}catch(ArgumentException){rejected=true;}
            R(rejected&&attachmentCallbacks==1,"real NETIF_ADDED registration conflict reaches constructor rollback");
            stack.Invoke(core=>R((nint)core.Globals.netif_list==healthy&&(nint)core.Globals.netif_default==healthy&&core.Globals.lwip_stats.mem.used==memory&&core.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used==pbufs,"rollback restores original netif/default route and natural heap/pbuf baseline"));
        }
        finally
        {
            if(registered)stack.Invoke(core=>core.netif_remove_ext_callback(callback));
            NativeMemory.Free(callback);
        }
        stack.SetLinkState(healthy,false);stack.SetLinkState(healthy,true);
        Console.WriteLine("PASS interface-rollback: real duplicate output registration, stop/FIFO/free, original interface/default/pool baseline, callback unregistered before backing release");
    }
}
