using System.Net;
using System.Net.Sockets;
using System.Text;
using Managed.Emulation.Host;

static void Check(bool value) { if (!value) throw new Exception("Assertion failed"); }
static T Ok<T>(HostResult<T> result) => result.Succeeded ? result.Value : throw new Exception("Unexpected " + result.Error);
static void Error<T>(GuestError expected, HostResult<T> result) { if (result.Error != expected) throw new Exception($"Expected {expected}, got {result.Error}"); }
static async Task SendAll(Socket socket, byte[] bytes)
{
    int offset=0;
    while(offset<bytes.Length) { int count=await socket.SendAsync(bytes.AsMemory(offset),SocketFlags.None); Check(count>0); offset+=count; }
}
static async Task ReadAll(Socket socket, Memory<byte> bytes)
{
    int offset=0;
    while(offset<bytes.Length) { int count=await socket.ReceiveAsync(bytes[offset..],SocketFlags.None); Check(count>0); offset+=count; }
}
static async Task ReadAllHost(InstanceIo io,int fd,Memory<byte> bytes)
{
    int offset=0;
    while(offset<bytes.Length) { int count=Ok(await io.ReadAsync(fd,bytes[offset..])); Check(count>0); offset+=count; }
}
static async Task WriteAllHost(InstanceIo io,int fd,ReadOnlyMemory<byte> bytes)
{
    int offset=0;
    while(offset<bytes.Length) { int count=Ok(await io.WriteAsync(fd,bytes[offset..])); Check(count>0); offset+=count; }
}
var image = new Dictionary<string, ReadOnlyMemory<byte>> { ["/data"] = "abcde"u8.ToArray() };
byte[] inputBytes="input"u8.ToArray();
await using var io = new InstanceIo(image, inputBytes, descriptorLimit: 8, outputLimit: 6);
inputBytes[0]=0;
byte[] buffer = new byte[32];
int file = Ok(io.OpenFile("/data", FileAccessMode.Read));
int socket = Ok(io.Socket());
Check(file == 3 && socket == 4);
int copy = Ok(io.Duplicate(file));
Check(Ok(await io.ReadAsync(file, buffer.AsMemory(0,2))) == 2 && Encoding.UTF8.GetString(buffer,0,2)=="ab");
Ok(io.Close(file));
Check(Ok(await io.ReadAsync(copy, buffer)) == 3 && Encoding.UTF8.GetString(buffer,0,3)=="cde");
Check(Ok(io.Seek(copy,0,SeekOrigin.Begin)) == 0);
Error(GuestError.NotSocket,io.Bind(copy,new(GuestEndpoint.Loopback,8080)));
Error(GuestError.IllegalSeek,io.Seek(socket,0,SeekOrigin.Begin));
int stdinCopy=Ok(io.Duplicate(0)); Ok(io.Close(0));
Check(Ok(await io.ReadAsync(stdinCopy,buffer))==5 && Encoding.UTF8.GetString(buffer,0,5)=="input");
Check(Ok(io.OpenFile("/data",FileAccessMode.Read))==0);
Check(Ok(await io.WriteAsync(1,"abcd"u8.ToArray()))==4);
Check(Ok(await io.WriteAsync(2,"EFGH"u8.ToArray()))==2);
Error(GuestError.NoSpace,await io.WriteAsync(1,"x"u8.ToArray()));
int stdoutCopy=Ok(io.Duplicate(1)); Ok(io.Close(1));
Check(Ok(await io.WriteAsync(stdoutCopy,ReadOnlyMemory<byte>.Empty))==0);
var captured=io.CapturedOutput;
Check(Encoding.UTF8.GetString(captured.StandardOutput)=="abcd" && Encoding.UTF8.GetString(captured.StandardError)=="EF");
captured.StandardOutput[0]=0;
Check(io.CapturedOutput.StandardOutput[0]=='a');
Console.WriteLine("common descriptors, file/stdin dup cursors, bounded owned output: PASS");

bool rejectedInput=false;
try { await using var invalid = new InstanceIo(image,"large"u8.ToArray(),inputLimit:4); }
catch(ArgumentOutOfRangeException) { rejectedInput=true; }
Check(rejectedInput);
await using var small=new InstanceIo(image,descriptorLimit:4);
int last=Ok(small.Socket());
Error(GuestError.TooManyFiles,small.OpenFile("/new",FileAccessMode.Write,create:true));
Error(GuestError.NoEntry,small.Stat("/new"));
Error(GuestError.TooManyFiles,small.Duplicate(1));
Ok(small.Close(last));
int writable=Ok(small.OpenFile("/new",FileAccessMode.Write,create:true));
Check(Ok(await small.WriteAsync(writable,"kept"u8.ToArray()))==4);
Error(GuestError.TooManyFiles,small.OpenFile("/new",FileAccessMode.Write,truncate:true));
Check(Ok(small.Stat("/new")).Length==4);
Console.WriteLine("shared capacity and failed create/truncate atomicity: PASS");

await using var service=new InstanceIo(image,descriptorLimit:8);
int listener=Ok(service.Socket());
Ok(service.Bind(listener,new(GuestEndpoint.Loopback,8080))); Ok(service.Listen(listener,2));
int listenerCopy=Ok(service.Duplicate(listener)); Ok(service.Close(listener));
IPEndPoint endpoint=Ok(service.Publish(listenerCopy));
using var client=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
await client.ConnectAsync(endpoint);
int peer=Ok(await service.AcceptAsync(listenerCopy)).Handle;
int peerCopy=Ok(service.Duplicate(peer)); Ok(service.Close(peer));
await SendAll(client,"ping"u8.ToArray());
Check(Ok(await service.WaitReadableAsync(peerCopy,TimeSpan.FromSeconds(2))));
await ReadAllHost(service,peerCopy,buffer.AsMemory(0,4));
Check(Encoding.UTF8.GetString(buffer,0,4)=="ping");
await WriteAllHost(service,peerCopy,"pong"u8.ToArray());
await ReadAll(client,buffer.AsMemory(0,4));
Check(Encoding.UTF8.GetString(buffer,0,4)=="pong");
Task<HostResult<int>> blocked=service.ReadAsync(peerCopy,buffer);
Ok(service.Close(peerCopy));
var closed=await blocked.WaitAsync(TimeSpan.FromSeconds(2));
Check(closed.Error is GuestError.Canceled or GuestError.BadDescriptor);
Console.WriteLine("socket dup ownership, real traffic, final close cancels receive: PASS");

Task<HostResult<AcceptedSocket>> waiting=service.AcceptAsync(listenerCopy);
await service.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(2));
Check((await waiting).Error is GuestError.Canceled or GuestError.BadDescriptor);
Check(service.OpenDescriptors==0);
await service.DisposeAsync();
Error(GuestError.BadDescriptor,service.OpenFile("/data",FileAccessMode.Read));
Check(Ok(await small.WriteAsync(1,"alive"u8.ToArray()))==5);
await io.DisposeAsync();
Check(Encoding.UTF8.GetString(io.CapturedOutput.StandardError)=="EF");
Console.WriteLine("disposal drains accept, retained bounded logs, instance isolation: PASS");
