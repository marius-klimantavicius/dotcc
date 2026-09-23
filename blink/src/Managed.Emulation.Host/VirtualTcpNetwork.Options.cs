using System.Net.Sockets;
namespace Managed.Emulation.Host;
public sealed partial class VirtualTcpNetwork
{
    public HostResult<int> GetOption(int handle,int level,int option)
    {
        lock(sync)
        {
            if(!Find(handle,out var entry))return Fail<int>(GuestError.BadDescriptor);
            try
            {
                int value;
                if(level==1 && option==4)
                {
                    value=(int)entry.PendingError;
                    entry.PendingError=GuestError.None;
                    // Connected transport errors may also arrive after connect.
                    // BCL reports SocketError values, never host errno numbers.
                    if(entry.Connection==ConnectionState.Connected)
                    {
                        var error=(SocketError)(int)entry.Socket.GetSocketOption(SocketOptionLevel.Socket,SocketOptionName.Error)!;
                        if(value==0 && error!=SocketError.Success)value=(int)ConvertError(new SocketException((int)error));
                    }
                }
                else if(level==1 && option==3)value=(int)entry.Socket.SocketType;
                else if(level==1 && option==2)value=(int)entry.Socket.GetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress)!;
                else if(level==1 && option==7)value=entry.Socket.SendBufferSize;
                else if(level==1 && option==8)value=entry.Socket.ReceiveBufferSize;
                else if(level==6 && option==1)value=entry.Socket.NoDelay?1:0;
                else return Fail<int>((GuestError)(level is 1 or 6?92:95));
                return HostResult<int>.Success(value);
            }
            catch(SocketException e){return Fail<int>(ConvertError(e));}
        }
    }
}
