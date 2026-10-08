#nullable enable
using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32.SafeHandles;
namespace Edg.Runtime;
public sealed unsafe partial class PosixContext
{
    // Shared open socket description; descriptor flags remain in Descriptor.
    internal sealed class SocketState
    {
        internal readonly Socket Value;
        internal readonly object Gate = new();
        internal volatile int Flags;
        private int references = 1;
        internal SocketState(Socket value, int flags) { Value=value; Flags=flags; }
        internal bool Closed => Volatile.Read(ref references)==0;
        internal void Retain() => Interlocked.Increment(ref references);
        internal void Release() { if (Interlocked.Decrement(ref references)==0) Value.Dispose(); }
    }
    private int SocketFailure(Exception error)
    {
        SetError(error is SocketException e ? SocketErrno(e.SocketErrorCode) : error is ObjectDisposedException ? 9 :
                 error is OutOfMemoryException ? 12 : error is NotSupportedException ? 95 : error is ArgumentException or InvalidOperationException ? 22 : 5);
        return -1;
    }
    private static int SocketErrno(SocketError error) => error switch {
        SocketError.Success=>0, SocketError.Interrupted=>4, SocketError.AccessDenied=>13, SocketError.Fault=>14,
        SocketError.InvalidArgument=>22, SocketError.TooManyOpenSockets=>24, SocketError.WouldBlock=>11,
        SocketError.InProgress=>115, SocketError.AlreadyInProgress=>114, SocketError.NotSocket=>88,
        SocketError.DestinationAddressRequired=>89, SocketError.MessageSize=>90, SocketError.ProtocolType=>91,
        SocketError.ProtocolOption=>92, SocketError.ProtocolNotSupported=>93, SocketError.SocketNotSupported=>94,
        SocketError.OperationNotSupported=>95, SocketError.AddressFamilyNotSupported=>97,
        SocketError.AddressAlreadyInUse=>98, SocketError.AddressNotAvailable=>99, SocketError.NetworkDown=>100,
        SocketError.NetworkUnreachable=>101, SocketError.NetworkReset=>102, SocketError.ConnectionAborted=>103,
        SocketError.ConnectionReset=>104, SocketError.NoBufferSpaceAvailable=>105, SocketError.IsConnected=>106,
        SocketError.NotConnected=>107, SocketError.Shutdown=>32, SocketError.TimedOut=>110,
        SocketError.ConnectionRefused=>111, SocketError.HostDown=>112, SocketError.HostUnreachable=>113,
        SocketError.OperationAborted=>9, _=>5 };
    private int RegisterSocket(Socket socket, int flags)
    {
        var state=new SocketState(socket,2|(flags&0x800));
        try {
            var descriptor=new Descriptor(new SafeFileHandle(-1,false),"",2) {Socket=state,DescriptorFlags=(flags&0x80000)!=0?1:0};
            lock(descriptorsGate) {
                if(disposed) { state.Release(); SetError(9); return -1; }
                int fd=3;while(descriptors.ContainsKey(fd))fd=checked(fd+1);
                descriptors.Add(fd,descriptor);return fd;
            }
        } catch {state.Release();throw;}
    }
    private SocketState? FindSocket(int fd)
    {
        var descriptor=Find(fd);if(descriptor is null)return null;
        lock(descriptor.Gate) {
            if(descriptor.Closed){SetError(9);return null;}
            if(descriptor.Socket is null){SetError(88);return null;}
            return descriptor.Socket;
        }
    }
    public int Socket(int domain,int type,int protocol)
    {
        if(domain is not (2 or 10)){SetError(97);return -1;}
        if((type&~(0x800|0x80000))!=1){SetError(94);return -1;}
        if(protocol is not (0 or 6)){SetError(93);return -1;}
        System.Net.Sockets.Socket? socket=null;
        try {
            // AF_INET/AF_INET6 STREAM protocol0 selects kernel TCP. Preserve
            // native default bind behavior: BCL explicit TCP Bind forces reuse.
            socket=new System.Net.Sockets.Socket(domain==2?AddressFamily.InterNetwork:AddressFamily.InterNetworkV6,SocketType.Stream,ProtocolType.Unspecified);
            if(domain==10)socket.DualMode=true; // Explicit portable Linux-derived default.
            socket.Blocking=(type&0x800)==0;
            return RegisterSocket(socket,type);
        } catch(Exception e) when(e is SocketException or ObjectDisposedException or InvalidOperationException or ArgumentException or NotSupportedException or OutOfMemoryException) {socket?.Dispose();return SocketFailure(e);}
    }
    private bool ReadSocketAddress(void* address,uint length,out IPEndPoint? endpoint)
    {
        endpoint=null;if(address==null){SetError(14);return false;}
        if(length<2){SetError(22);return false;}
        var bytes=new ReadOnlySpan<byte>(address,(int)Math.Min(length,28));
        int family=BinaryPrimitives.ReadUInt16LittleEndian(bytes);
        if(family is not (2 or 10)){SetError(97);return false;}
        if(length<(family==2?16u:28u)){SetError(22);return false;}
        if(family==10 && BinaryPrimitives.ReadUInt32BigEndian(bytes[4..])!=0){SetError(95);return false;} // flow labels need a separate supported contract
        var ip=family==2?new IPAddress(bytes.Slice(4,4)):new IPAddress(bytes.Slice(8,16),BinaryPrimitives.ReadUInt32LittleEndian(bytes[24..]));
        endpoint=new IPEndPoint(ip,BinaryPrimitives.ReadUInt16BigEndian(bytes[2..]));return true;
    }
    private int WriteSocketAddress(EndPoint? endpoint,void* address,uint* length)
    {
        if(address==null||length==null){SetError(14);return -1;}
        if(endpoint is not IPEndPoint ip){SetError(22);return -1;}
        Span<byte> packed=stackalloc byte[28];packed.Clear();bool v6=ip.AddressFamily==AddressFamily.InterNetworkV6;
        int size=v6?28:16;BinaryPrimitives.WriteUInt16LittleEndian(packed,(ushort)(v6?10:2));
        BinaryPrimitives.WriteUInt16BigEndian(packed[2..],(ushort)ip.Port);
        ip.Address.TryWriteBytes(packed[(v6?8:4)..],out _);
        if(v6)BinaryPrimitives.WriteUInt32LittleEndian(packed[24..],checked((uint)ip.Address.ScopeId));
        int n=(int)Math.Min(*length,(uint)size);packed[..n].CopyTo(new Span<byte>(address,n));*length=(uint)size;return 0;
    }
    public int Bind(int fd,void* address,uint length)
    {
        var state=FindSocket(fd);if(state is null)return -1;
        if(!ReadSocketAddress(address,length,out var endpoint))return -1;
        // Portable sockaddr families remain exact; BCL otherwise maps IPv4 on dual sockets.
        if(endpoint!.AddressFamily!=state.Value.AddressFamily){SetError(state.Value.AddressFamily==AddressFamily.InterNetwork?97:22);return -1;}
        try{state.Value.Bind(endpoint!);return 0;}catch(Exception e) when(e is SocketException or ObjectDisposedException or InvalidOperationException or ArgumentException){return SocketFailure(e);}
    }
    public int Listen(int fd,int backlog)
    {
        var state=FindSocket(fd);if(state is null)return -1;
        try{lock(state.Gate){if(!state.Value.IsBound)state.Value.Bind(new IPEndPoint(state.Value.AddressFamily==AddressFamily.InterNetwork?IPAddress.Any:IPAddress.IPv6Any,0));state.Value.Listen(backlog<0?int.MaxValue:backlog);return 0;}}catch(Exception e) when(e is SocketException or ObjectDisposedException or InvalidOperationException or ArgumentException){return SocketFailure(e);}
    }
    public int Connect(int fd,void* address,uint length)
    {
        var state=FindSocket(fd);if(state is null)return -1;
        if(!ReadSocketAddress(address,length,out var endpoint))return -1;
        // Portable sockaddr families remain exact; BCL otherwise maps IPv4 on dual sockets.
        if(endpoint!.AddressFamily!=state.Value.AddressFamily){SetError(state.Value.AddressFamily==AddressFamily.InterNetwork?97:22);return -1;}
        try{state.Value.Connect(endpoint!);return 0;}catch(SocketException e) when(e.SocketErrorCode==SocketError.WouldBlock){SetError(115);return -1;}catch(Exception e) when(e is SocketException or ObjectDisposedException or InvalidOperationException or ArgumentException){return SocketFailure(e);}
    }
    public int Accept(int fd,void* address,uint* length)
    {
        var state=FindSocket(fd);if(state is null)return -1;
        if(address!=null&&length==null){SetError(14);return -1;}
        System.Net.Sockets.Socket? accepted=null;
        try {
            accepted=state.Value.Accept();
            // Linux accept does not inherit O_NONBLOCK or FD_CLOEXEC.
            accepted.Blocking=true;
            if(address!=null&&WriteSocketAddress(accepted.RemoteEndPoint,address,length)<0){accepted.Dispose();return -1;}
            return RegisterSocket(accepted,0);
        }catch(Exception e) when(e is SocketException or ObjectDisposedException or InvalidOperationException or ArgumentException or OutOfMemoryException){accepted?.Dispose();if(state.Closed){SetError(9);return -1;}return SocketFailure(e);}
    }
    public int Getsockname(int fd,void* address,uint* length)
    {
        var state=FindSocket(fd);if(state is null)return -1;
        try{return WriteSocketAddress(state.Value.LocalEndPoint ?? new IPEndPoint(state.Value.AddressFamily==AddressFamily.InterNetwork?IPAddress.Any:IPAddress.IPv6Any,0),address,length);}catch(Exception e) when(e is SocketException or ObjectDisposedException or InvalidOperationException or ArgumentException){return SocketFailure(e);}
    }
    public int Shutdown(int fd,int how)
    {
        var state=FindSocket(fd);if(state is null)return -1;if(how is <0 or >2){SetError(22);return -1;}
        try{state.Value.Shutdown((SocketShutdown)how);return 0;}catch(Exception e) when(e is SocketException or ObjectDisposedException or InvalidOperationException){return SocketFailure(e);}
    }
    private long SocketTransfer(Descriptor descriptor,void* buffer,ulong count,bool writing,bool positioned)
    {
        SocketState state;
        lock(descriptor.Gate){if(descriptor.Closed){SetError(9);return -1;}state=descriptor.Socket!;}
        if(positioned){SetError(29);return -1;}
        if(!writing&&count==0)return 0;
        try {
            int length=(int)Math.Min(count,int.MaxValue);
            // No descriptor/state lock across blocking I/O: close/disposal must wake it.
            int transferred=writing?state.Value.Send(new ReadOnlySpan<byte>(buffer,length),SocketFlags.None):state.Value.Receive(new Span<byte>(buffer,length),SocketFlags.None);
            // BCL Dispose can wake Receive with zero; distinguish local final-close
            // cancellation from peer EOF. Preserve any already transferred bytes.
            if(transferred==0 && state.Closed){SetError(9);return -1;}
            return transferred;
        }catch(Exception e) when(e is SocketException or ObjectDisposedException or InvalidOperationException or ArgumentException){if(state.Closed){SetError(9);return -1;}return SocketFailure(e);}
    }
    private int SocketFcntl(Descriptor descriptor,int command,int value)
    {
        var state=descriptor.Socket!;
        lock(state.Gate) {
            try {
                switch(command){
                    case 1:return descriptor.DescriptorFlags;
                    case 2:descriptor.DescriptorFlags=value&1;return 0;
                    case 3:return state.Flags;
                    case 4:
                        if((value&~(3|0x800))!=0){SetError(95);return -1;}
                        state.Value.Blocking=(value&0x800)==0;state.Flags=2|(value&0x800);return 0;
                    default:SetError(22);return -1;
                }
            }catch(Exception e) when(e is SocketException or ObjectDisposedException or InvalidOperationException or ArgumentException){return SocketFailure(e);}
        }
    }
    // Candidate/test-only: no backend binding and no claim of generic POSIX dup.
    internal int DupSocket(int fd)
    {
        lock(descriptorsGate) {
            if(disposed||!descriptors.TryGetValue(fd,out var descriptor)){SetError(9);return -1;}
            lock(descriptor.Gate) {
                if(descriptor.Closed){SetError(9);return -1;}
                var state=descriptor.Socket;if(state is null){SetError(88);return -1;}
                int next=3;while(descriptors.ContainsKey(next))next=checked(next+1);
                var clone=new Descriptor(new SafeFileHandle(-1,false),"",2){Socket=state};
                descriptors.Add(next,clone);state.Retain();return next;
            }
        }
    }
    public static ushort Htons(ushort value)=>BitConverter.IsLittleEndian?BinaryPrimitives.ReverseEndianness(value):value;
    public static uint Htonl(uint value)=>BitConverter.IsLittleEndian?BinaryPrimitives.ReverseEndianness(value):value;
}
