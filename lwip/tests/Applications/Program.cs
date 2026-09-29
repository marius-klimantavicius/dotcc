using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Managed.Networking;
using static Managed.Networking.Lwip;

unsafe class Program
{
    sealed class Server
    {
        public nint Index;
        public int Ssi,Cgi,Post;
        public Exception? Error;
    }
    sealed class Client
    {
        public nint Pcb;
        public bool Connected,Finished;
        public Exception? Error;
        public readonly MemoryStream Response=new();
        public readonly AutoResetEvent Progress=new(false);
    }
    static readonly ConcurrentDictionary<Lwip,Server> Servers=new();
    static void R(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    static Client State(void* arg)=>(Client)GCHandle.FromIntPtr((nint)arg).Target!;
    static void Error(Lwip owner,void* arg,sbyte error){var s=State(arg);s.Pcb=0;s.Error=new InvalidOperationException("HTTP client transport error "+error);s.Progress.Set();}
    static sbyte Connected(Lwip owner,void* arg,tcp_pcb* pcb,sbyte error){var s=State(arg);if(error!=0){s.Error=new InvalidOperationException("HTTP connect "+error);return error;}s.Connected=true;s.Progress.Set();return 0;}
    static sbyte Receive(Lwip owner,void* arg,tcp_pcb* pcb,pbuf* packet,sbyte error)
    {
        var s=State(arg);try
        {
            R(error==0,"HTTP receive status");if(packet==null){owner.tcp_arg(pcb,null);owner.tcp_recv(pcb,null);owner.tcp_err(pcb,null);R(owner.tcp_close(pcb)==0,"HTTP close after peer FIN");s.Pcb=0;s.Finished=true;return 0;}
            for(var p=packet;p!=null;p=p->next)s.Response.Write(new ReadOnlySpan<byte>(p->payload,p->len));owner.tcp_recved(pcb,packet->tot_len);return 0;
        }
        catch(Exception failure){s.Error=failure;s.Pcb=0;owner.tcp_abort(pcb);return -13;}
        finally{if(packet!=null)owner.pbuf_free(packet);s.Progress.Set();}
    }
    static void Wait(VirtualPair pair,Client s,Func<bool> done)
    {var time=Stopwatch.StartNew();while(!done()){pair.Check();if(s.Error!=null)throw s.Error;R(time.Elapsed<TimeSpan.FromSeconds(10),"bounded HTTP progress");s.Progress.WaitOne(20);}if(s.Error!=null)throw s.Error;}
    static byte[] Request(VirtualPair pair,string request,bool ipv6=false)
    {
        var client=new Client();var handle=GCHandle.Alloc(client);nint arg=GCHandle.ToIntPtr(handle);
        try
        {
            pair.A.Invoke(owner=>{ip_addr address;fixed(byte* text=Encoding.ASCII.GetBytes((ipv6?"fd00::2":"10.0.0.2")+"\0"))R(owner.ipaddr_aton(text,&address)==1,"HTTP destination");var pcb=owner.tcp_new_ip_type((byte)(ipv6?6:0));R(pcb!=null,"HTTP client allocation");client.Pcb=(nint)pcb;owner.tcp_arg(pcb,(void*)arg);owner.tcp_recv(pcb,&Receive);owner.tcp_err(pcb,&Error);R(owner.tcp_connect(pcb,&address,80,&Connected)==0,"HTTP connect submit");});
            Wait(pair,client,()=>Volatile.Read(ref client.Connected));byte[] bytes=Encoding.ASCII.GetBytes(request);
            // Deliberately send the first header fragment separately; original
            // HTTP parser must retain it before the rest of the request arrives.
            int cut=Math.Min(7,bytes.Length);foreach(var piece in new[]{bytes[..cut],bytes[cut..]}){pair.A.Invoke(owner=>{fixed(byte* p=piece){R(owner.tcp_write((tcp_pcb*)client.Pcb,p,(ushort)piece.Length,TCP_WRITE_FLAG_COPY)==0,"HTTP request fragment");R(owner.tcp_output((tcp_pcb*)client.Pcb)==0,"HTTP request output");}});pair.Drain();}
            Wait(pair,client,()=>Volatile.Read(ref client.Finished));return client.Response.ToArray();
        }
        finally{pair.A.Invoke(owner=>{if(client.Pcb!=0){owner.tcp_arg((tcp_pcb*)client.Pcb,null);owner.tcp_err((tcp_pcb*)client.Pcb,null);owner.tcp_abort((tcp_pcb*)client.Pcb);client.Pcb=0;}});handle.Free();client.Progress.Dispose();client.Response.Dispose();}
    }
    static byte[] Body(byte[] response,int code)
    {
        ReadOnlySpan<byte> split="\r\n\r\n"u8;int index=response.AsSpan().IndexOf(split);R(index>=0,"HTTP header terminator");string headers=Encoding.ASCII.GetString(response,0,index);R(headers.StartsWith("HTTP/1.",StringComparison.Ordinal)&&headers.Split(' ')[1]==code.ToString(),"HTTP status "+code);return response[(index+4)..];
    }
    static ushort Ssi(Lwip owner,int index,byte* output,int capacity,ushort part,ushort* next)
    {
        var s=Servers[owner];try{R(index==0&&part==0&&capacity>=5,"SSI callback fields");"world"u8.CopyTo(new Span<byte>(output,capacity));*next=0xffff;s.Ssi++;return 5;}catch(Exception error){s.Error=error;return 0;}
    }
    static byte* Cgi(Lwip owner,int index,int count,byte** names,byte** values)
    {
        var s=Servers[owner];try{R(index==0&&count==1&&Marshal.PtrToStringUTF8((nint)names[0])=="x"&&Marshal.PtrToStringUTF8((nint)values[0])=="one%20two","CGI raw escaped parameters (upstream contract)");s.Cgi++;return(byte*)s.Index;}catch(Exception error){s.Error=error;return null;}
    }
    static string Get(string path)=>"GET "+path+" HTTP/1.0\r\nHost: fixture\r\nConnection: close\r\n\r\n";
    static void Main(string[] args)
    {
        using var native=JsonDocument.Parse(File.ReadAllText(args[1]));R(sizeof(fs_file)==native.RootElement.GetProperty("fs_file_size").GetInt32(),"native HTTP file ABI");
        bool generated=args.Length>0&&args[0]=="http-content";var pair=new VirtualPair();var state=new Server();Servers[pair.B.Core]=state;List<nint> allocations=[];
        nint Text(string value){nint p=Marshal.StringToCoTaskMemUTF8(value);allocations.Add(p);return p;}
        nint index=Text("/index.txt"),cgiName=Text("/echo.cgi"),tag=Text("name");state.Index=index;
        tCGI* handlers=(tCGI*)NativeMemory.AllocZeroed((nuint)sizeof(tCGI));byte** tags=(byte**)NativeMemory.Alloc((nuint)sizeof(nint));handlers->pcCGIName=(byte*)cgiName;handlers->pfnCGIHandler=&Cgi;tags[0]=(byte*)tag;
        try
        {
            LwipApplications.AddContent(pair.B.Core,"/index.txt","alpha"u8);LwipApplications.AddContent(pair.B.Core,"/tag.shtml","hello <!--#name-->!"u8);LwipApplications.AddContent(pair.B.Core,"/done.txt","stored"u8);
            LwipApplications.SetPostHandler(pair.B.Core,(path,data)=>{R(path=="/submit"&&data.AsSpan().SequenceEqual("hello world"u8),"POST storage data");state.Post++;return"/done.txt";});
            pair.B.Invoke(owner=>{owner.http_set_cgi_handlers(handlers,1);owner.http_set_ssi_handler(&Ssi,tags,1);owner.httpd_init();});
            R(Body(Request(pair,Get("/index.txt")),200).AsSpan().SequenceEqual("alpha"u8),"custom HTTP content");
            R(Body(Request(pair,Get("/tag.shtml"),true),200).AsSpan().SequenceEqual("hello <!--#name-->world!"u8),"SSI insertion retains upstream tag");
            R(Body(Request(pair,Get("/echo.cgi?x=one%20two")),200).AsSpan().SequenceEqual("alpha"u8),"CGI path content");
            Body(Request(pair,Get("/absent")),404);
            R(Body(Request(pair,"POST /submit HTTP/1.0\r\nHost: fixture\r\nContent-Length: 11\r\nConnection: close\r\n\r\nhello world"),200).AsSpan().SequenceEqual("stored"u8),"POST completion content");
            if(generated)foreach(var item in new[]{("empty.txt",Array.Empty<byte>()),("odd.txt","abcde"u8.ToArray()),("multiple.txt",Enumerable.Range(0,4097).Select(i=>(byte)(i%251)).ToArray())})R(Body(Request(pair,Get("/"+item.Item1)),200).SequenceEqual(item.Item2),"translated makefsdata HTTP roundtrip "+item.Item1);
            pair.Drain();if(state.Error!=null)throw state.Error;R(state.Ssi==1&&state.Cgi==1&&state.Post==1,"actual SSI/CGI/POST invocations");LwipApplications.ThrowErrors(pair.B.Core);R(LwipApplications.Counters(pair.B.Core)==(0,0),"HTTP content/POST handles released");
            Console.WriteLine("PASS http-transactions: custom GET, IPv6 SSI, raw CGI parameters,404,POST,fragmented request headers"+(generated?",translated generator three-file roundtrip":""));
        }
        finally
        {
            pair.B.Invoke(owner=>{var listener=owner.Globals.tcp_listen_pcbs.listen_pcbs;while(listener!=null&&listener->local_port!=80)listener=listener->next;R(listener!=null,"owned HTTP listener");var altcp=(altcp_pcb*)listener->callback_arg;R(altcp!=null&&owner.altcp_close(altcp)==0,"HTTP listener shutdown");});
            Servers.TryRemove(pair.B.Core,out _);pair.Dispose();NativeMemory.Free(handlers);NativeMemory.Free(tags);foreach(nint p in allocations)Marshal.FreeCoTaskMem(p);
        }
    }
}
