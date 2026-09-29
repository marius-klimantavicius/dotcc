using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Managed.Networking;
// Test fixture only: project is never referenced by the translated product.
internal sealed unsafe partial class Tap : IDisposable
{
    [StructLayout(LayoutKind.Sequential)] struct PollFd { public int File;public short Events,Returned; }
    [LibraryImport("libc",SetLastError=true)] private static partial int ioctl(int fd,ulong request,void* argument);
    [LibraryImport("libc",SetLastError=true)] private static partial int poll(PollFd* descriptors,ulong count,int timeout);
    private readonly SafeFileHandle handle;
    private readonly FileStream stream;
    private readonly Thread reader;
    private readonly LwipStack stack;
    private readonly Pcap capture;
    private readonly object writing=new();
    private volatile bool stopping;
    private Exception? error;
    private readonly nint netif;
    internal Tap(LwipStack stack,string path)
    {
        this.stack=stack;capture=new Pcap(path);handle=File.OpenHandle("/dev/net/tun",FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite);
        byte* request=stackalloc byte[40];new Span<byte>(request,40).Clear();"lwiptest0"u8.CopyTo(new Span<byte>(request,16));*(ushort*)(request+16)=0x1002; // IFF_TAP | IFF_NO_PI
        if(ioctl((int)handle.DangerousGetHandle(),0x400454ca,request)!=0)throw new IOException("TUNSETIFF errno="+Marshal.GetLastPInvokeError());
        if(ioctl((int)handle.DangerousGetHandle(),0x400454d0,null)!=0)throw new IOException("TUNSETOFFLOAD(0) errno="+Marshal.GetLastPInvokeError());
        stream=new FileStream(handle,FileAccess.ReadWrite,1,false);
        netif=stack.AddInterface("10.9.0.2","fd09::2",new byte[]{2,0,0,0,9,2},Send);
        reader=new Thread(Read){IsBackground=true,Name="isolated test TAP"};reader.Start();
    }
    private void Send(byte[] bytes){capture.Write(bytes);lock(writing)stream.Write(bytes);}
    private void Read()
    {
        try
        {
            byte[] bytes=new byte[65536];var descriptor=new PollFd{File=(int)handle.DangerousGetHandle(),Events=1};
            while(!stopping)
            {
                int result=poll(&descriptor,1,50);if(result<0){if(Marshal.GetLastPInvokeError()==4)continue;throw new IOException("TAP poll failed");}
                if(result>0){int count=stream.Read(bytes);if(count<=0)throw new IOException("TAP EOF");capture.Write(bytes.AsSpan(0,count).ToArray());stack.Receive(netif,bytes.AsSpan(0,count));}stack.Poll();
            }
        }
        catch(Exception failure){error=failure;}
    }
    internal void Check(){if(error!=null)throw new AggregateException("TAP transport failed",error);LwipFrames.ThrowErrors(stack.Core);}
    public void Dispose(){stack.SetLinkState(netif,false);stack.Invoke(owner=>LwipFrames.Detach(owner,netif));stopping=true;if(!reader.Join(TimeSpan.FromSeconds(10)))throw new TimeoutException("TAP reader join");try{Check();}finally{stream.Dispose();handle.Dispose();capture.Dispose();}}
}
