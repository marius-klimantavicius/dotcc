namespace Managed.Emulation.Host;
public sealed partial class InstanceIo
{
    public HostResult<int> GetSocketOption(int fd,int level,int option)=>SocketCall(fd,handle=>network.GetOption(handle,level,option));
}
