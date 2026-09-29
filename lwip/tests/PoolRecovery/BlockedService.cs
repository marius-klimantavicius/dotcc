#if !LWIP_NOSYS
using System.Diagnostics;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    sealed class BlockedService : IDisposable
    {
        readonly LwipStack stack;readonly Lwip owner;readonly Thread caller;readonly IDisposable lease;
        int descriptor=-1;long result;int error;Exception? failure;bool joined;
        internal BlockedService(LwipStack stack)
        {
            this.stack=stack;owner=stack.Core;lease=owner.__DotCcRetain();
            caller=new Thread(Receive){IsBackground=true,Name="owned translated UDP receive"};
            try
            {
                using var entry=owner.__DotCcEnter();owner.lwip_socket_thread_init();
                try
                {
                    descriptor=owner.lwip_socket(AF_INET,SOCK_DGRAM,0);R(descriptor>=0,"owned socket allocate");
                    sockaddr_in address=default;address.sin_len=(byte)sizeof(sockaddr_in);address.sin_family=AF_INET;address.sin_port=owner.lwip_htons(28500);address.sin_addr.s_addr=owner.lwip_htonl(0x7f000001);
                    R(owner.lwip_bind(descriptor,(sockaddr*)&address,(uint)sizeof(sockaddr_in))==0,"owned socket bind");caller.Start();
                    var deadline=Stopwatch.StartNew();
                    while(true)
                    {
                        if(failure!=null)throw failure;ulong token=LwipSysArch.Protect(owner);int waiting;
                        try{waiting=owner.lwip_socket_dbg_get_socket(descriptor)->conn->mbox_threads_waiting;}finally{LwipSysArch.Unprotect(owner,token);}
                        if(waiting>0)break;R(deadline.Elapsed<TimeSpan.FromSeconds(5),"owned caller reached original netconn wait");Thread.Yield();
                    }
                }
                finally{owner.lwip_socket_thread_cleanup();}
            }
            catch
            {
                // A factory that fails owns its partially created descriptor/caller.
                if(descriptor>=0)Close();if(caller.IsAlive&&!caller.Join(TimeSpan.FromSeconds(5)))throw new TimeoutException("partial service caller did not drain");lease.Dispose();throw;
            }
        }
        void Receive()
        {
            using var entry=owner.__DotCcEnter();owner.lwip_socket_thread_init();
            try{byte value;result=owner.lwip_recv(descriptor,&value,1,0);error=Libc.errno;}
            catch(Exception caught){failure=caught;}
            finally{owner.lwip_socket_thread_cleanup();}
        }
        void Close()
        {
            using var entry=owner.__DotCcEnter();owner.lwip_socket_thread_init();
            try{R(owner.lwip_close(descriptor)==0,"owned shutdown invokes real socket close");descriptor=-1;}
            finally{owner.lwip_socket_thread_cleanup();}
        }
        public void Dispose()
        {
            if(joined)return;if(descriptor>=0)Close();
            R(caller.Join(TimeSpan.FromSeconds(5)),"owned blocked caller completed upstream waiter cleanup");
            if(failure!=null)throw failure;R(result==-1&&error==107,"owned cancellation follows native ENOTCONN contract");
            stack.Invoke(core=>{R(core.Globals.memp_NETCONN.stats->used==0,"owned closed netconn pool returns to baseline");R(core.Globals.memp_UDP_PCB.stats->used==0,"owned closed UDP pool returns to baseline");});
            lease.Dispose();joined=true;
        }
    }
    static void PendingService()
    {
        using var stack=new LwipStack();stack.Start();var owner=stack.Core;stack.StartService(s=>new BlockedService(s));GC.Collect();GC.WaitForPendingFinalizers();stack.Dispose();
        R(LwipSysArch.Counters(owner)==(0,0,0),"owner disposal joined pending API caller and released host resources");
        Console.WriteLine("PASS owned pending socket service: real close wakes receive, upstream accounting and pools drain before TCP/IP stop");
    }
}
#endif
