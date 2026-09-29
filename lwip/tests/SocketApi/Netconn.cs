#if !LWIP_NOSYS
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
internal static unsafe partial class Program
{
    static void NetconnCase(VirtualPair pair,bool ipv6)
    {
        var a=pair.A.Core;var b=pair.B.Core;int port=ipv6?24102:24101;string target=ipv6?"fd00::2":"10.0.0.2";
        netconn_type type=ipv6?netconn_type.NETCONN_TCP_IPV6:netconn_type.NETCONN_TCP;
        using var listening=new ManualResetEventSlim();Exception? serverError=null;
        var server=new Thread(()=>
        {
            using var binding=b.__DotCcEnter();b.lwip_socket_thread_init();netconn* listener=null;netconn* accepted=null;
            try
            {
                ip_addr address;byte[] text=Encoding.UTF8.GetBytes(target+"\0");fixed(byte* bytes=text)Require(b.ipaddr_aton(bytes,&address)==1,"netconn address");
                listener=b.netconn_new_with_proto_and_callback(type,0,null);Require(listener!=null,"netconn listener allocation");listener->recv_timeout=10000;
                Require(b.netconn_bind(listener,&address,(ushort)port)==0,"netconn bind");
                var duplicate=b.netconn_new_with_proto_and_callback(type,0,null);Require(duplicate!=null,"duplicate allocation");
                Require(b.netconn_bind(duplicate,&address,(ushort)port)==-8,"duplicate bind ERR_USE");Require(b.netconn_delete(duplicate)==0,"duplicate delete");
                Require(b.netconn_listen_with_backlog(listener,2)==0,"netconn listen");listening.Set();
                Require(b.netconn_accept(listener,&accepted)==0&&accepted!=null,"netconn accept");accepted->recv_timeout=10000;
                int received=0;
                while(received<262144)
                {
                    netbuf* buffer;Require(b.netconn_recv(accepted,&buffer)==0&&buffer!=null,"netconn recv");
                    try
                    {
                        do
                        {
                            void* bytes;ushort length;Require(b.netbuf_data(buffer,&bytes,&length)==0,"netbuf data");
                            for(int i=0;i<length;i++)Require(((byte*)bytes)[i]==Pattern(received++),"netconn payload");
                            ulong written;Require(b.netconn_write_partly(accepted,bytes,length,NETCONN_COPY,&written)==0&&written==length,"netconn echo");
                        }while(b.netbuf_next(buffer)>=0);
                    }
                    finally{b.netbuf_delete(buffer);}
                }
                Require(b.netconn_close(accepted)==0,"netconn peer close");Require(b.netconn_delete(accepted)==0,"netconn accepted delete");accepted=null;
                Require(b.netconn_delete(listener)==0,"netconn listener delete");listener=null;
            }
            catch(Exception error){serverError=error;listening.Set();}
            finally{if(accepted!=null)b.netconn_delete(accepted);if(listener!=null)b.netconn_delete(listener);b.lwip_socket_thread_cleanup();}
        }){IsBackground=true};server.Start();
        using var caller=a.__DotCcEnter();a.lwip_socket_thread_init();netconn* client=null;
        try
        {
            Require(listening.Wait(TimeSpan.FromSeconds(10)),"netconn listen ready");if(serverError!=null)throw serverError;
            ip_addr address;byte[] text=Encoding.UTF8.GetBytes(target+"\0");fixed(byte* bytes=text)Require(a.ipaddr_aton(bytes,&address)==1,"client address");
            client=a.netconn_new_with_proto_and_callback(type,0,null);Require(client!=null,"netconn client");client->recv_timeout=10000;
            Require(a.netconn_connect(client,&address,(ushort)port)==0,"netconn connect");byte* payload=stackalloc byte[4096];int echoed=0;
            for(int sent=0;sent<262144;sent+=4096)
            {
                for(int i=0;i<4096;i++)payload[i]=Pattern(sent+i);ulong written;
                Require(a.netconn_write_partly(client,payload,4096,NETCONN_COPY,&written)==0&&written==4096,"netconn write");
                while(echoed<sent+4096)
                {
                    netbuf* buffer;Require(a.netconn_recv(client,&buffer)==0&&buffer!=null,"netconn echo recv");
                    try{do{void* bytes;ushort length;Require(a.netbuf_data(buffer,&bytes,&length)==0,"netbuf echo data");for(int i=0;i<length;i++)Require(((byte*)bytes)[i]==Pattern(echoed++),"netconn echo bytes");}while(a.netbuf_next(buffer)>=0);}
                    finally{a.netbuf_delete(buffer);}
                }
            }
            netbuf* eof=null;Require(a.netconn_recv(client,&eof)==-15&&eof==null,"netconn FIN ERR_CLSD");
            Require(a.netconn_close(client)==0,"netconn close");Require(a.netconn_delete(client)==0,"netconn delete");client=null;
            Require(server.Join(TimeSpan.FromSeconds(10)),"netconn server join");if(serverError!=null)throw serverError;
            Console.WriteLine($"PASS netconn IPv{(ipv6?6:4)} echo=262144 duplicate-bind FIN close/delete");
        }
        finally{if(client!=null)a.netconn_delete(client);a.lwip_socket_thread_cleanup();if(server.IsAlive&&!server.Join(TimeSpan.FromSeconds(10)))throw new TimeoutException("netconn caller cleanup did not release peer");}
    }
}
#endif
