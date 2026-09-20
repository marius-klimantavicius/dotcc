namespace Managed.Emulation.Host;
public sealed partial class InstanceIo
{
    public HostResult<int> GetSocketOption(int fd,int level,int option)=>SocketCall(fd,handle=>network.GetOption(handle,level,option));
    public HostResult<int> SetSocketTimeout(int fd, bool receive, SocketTimeout value)
        => SocketCall(fd, handle => network.SetTimeout(handle, receive, value));
    public HostResult<SocketTimeout> GetSocketTimeout(int fd, bool receive)
        => SocketCall(fd, handle => network.GetTimeout(handle, receive));
}
