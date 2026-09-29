using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
using static Fixture;

internal static unsafe class MqttCases
{
    sealed class State
    {
        internal nint Client;
        internal readonly object Gate=new();
        internal readonly List<int> Connections=new();
        internal readonly List<int> Requests=new();
        internal readonly MemoryStream Payload=new();
        internal string? Topic;
        internal uint Expected;
        internal bool Last;
        internal Exception? Failure;
        internal void Check(){lock(Gate)if(Failure!=null)throw new AggregateException("MQTT callback failed",Failure);}
    }
    static State Get(void* arg)=>(State)GCHandle.FromIntPtr((nint)arg).Target!;
    static void Connection(Lwip owner,mqtt_client_s* client,void* arg,mqtt_connection_status_t status)
    {
        var state=Get(arg);try{lock(state.Gate){Require((nint)client==state.Client,"MQTT callback client identity");state.Connections.Add((int)status);}}
        catch(Exception error){lock(state.Gate)state.Failure=error;}
    }
    static void Request(Lwip owner,void* arg,sbyte result)
    {var state=Get(arg);try{lock(state.Gate)state.Requests.Add(result);}catch(Exception error){lock(state.Gate)state.Failure=error;}}
    static void Published(Lwip owner,void* arg,byte* topic,uint length)
    {
        var state=Get(arg);try{lock(state.Gate){state.Topic=Marshal.PtrToStringUTF8((nint)topic);state.Expected=length;state.Payload.SetLength(0);state.Last=length==0;}}
        catch(Exception error){lock(state.Gate)state.Failure=error;}
    }
    static void Data(Lwip owner,void* arg,byte* data,ushort length,byte flags)
    {
        var state=Get(arg);try{lock(state.Gate){Require(state.Payload.Length+length<=4096,"bounded copied MQTT delivery");state.Payload.Write(new ReadOnlySpan<byte>(data,length));if((flags&1)!=0)state.Last=true;}}
        catch(Exception error){lock(state.Gate)state.Failure=error;}
    }
    internal static byte[] Frame(Fixture fixture,TcpPeer peer)
    {
        int size=0;
        fixture.Wait(()=>
        {
            byte[] bytes=peer.Snapshot();if(bytes.Length<2)return false;
            int length=0,multiplier=1,index=1;
            do
            {
                if(index>=bytes.Length)return false;
                byte item=bytes[index++];length+=(item&127)*multiplier;
                if((item&128)==0){size=index+length;return bytes.Length>=size;}
                multiplier*=128;Require(index<=4,"valid original MQTT remaining length");
            }while(true);
        },peer.Check);
        return peer.Take(size);
    }
    internal static byte[] Body(byte[] frame)
    {int start=1;while((frame[start++]&128)!=0){}return frame[start..];}
    internal static byte[] Packet(byte header,ReadOnlySpan<byte> body)
    {
        var bytes=new List<byte>{header};int remaining=body.Length;
        do{byte digit=(byte)(remaining%128);remaining/=128;if(remaining!=0)digit|=128;bytes.Add(digit);}while(remaining!=0);
        bytes.AddRange(body.ToArray());return bytes.ToArray();
    }
    static void Connected(Fixture fixture,TcpPeer peer,State state,nint argument,ushort port,byte connack=0)
    {
        nint id=fixture.Text("dotcc-client");
        fixture.Pair.A.Invoke(owner=>
        {
            ip_addr address;Fixture.Address(owner,"10.0.0.2",&address);
            mqtt_connect_client_info_t info=default;info.client_id=(byte*)id;info.keep_alive=0;
            Require(owner.mqtt_client_connect((mqtt_client_s*)state.Client,&address,port,&Connection,(void*)argument,&info)==0,"original MQTT connect submission");
        });
        var frame=Frame(fixture,peer);Require(frame[0]==0x10,"original MQTT CONNECT type");var body=Body(frame);
        Require(body.AsSpan().SequenceEqual(new byte[]{0,4,(byte)'M',(byte)'Q',(byte)'T',(byte)'T',4,2,0,0,0,12}.Concat("dotcc-client"u8.ToArray()).ToArray()),"original MQTT clean-session CONNECT fields");
        int before;lock(state.Gate)before=state.Connections.Count;
        peer.Send(fixture,new byte[]{0x20,2,0,connack});
        fixture.Wait(()=>{lock(state.Gate)return state.Connections.Count==before+1;},state.Check);
        lock(state.Gate)Require(state.Connections[^1]==connack,"ordinary MQTT CONNACK result");
    }
    internal static void Run(Fixture fixture)
    {
        var state=new State();nint arg=fixture.Root(state);nint topic=fixture.Text("dotcc/in"),outTopic=fixture.Text("dotcc/out");
        fixture.Pair.A.Invoke(owner=>
        {
            state.Client=(nint)owner.mqtt_client_new();Require(state.Client!=0,"original MQTT client allocation");
            owner.mqtt_set_inpub_callback((mqtt_client_s*)state.Client,&Published,&Data,(void*)arg);
        });
        fixture.BeforeStop(()=>fixture.Pair.A.Invoke(owner=>
        {
            if(state.Client==0)return;
            owner.mqtt_disconnect((mqtt_client_s*)state.Client);
            owner.mqtt_set_inpub_callback((mqtt_client_s*)state.Client,null,null,null);
            owner.mqtt_client_free((mqtt_client_s*)state.Client);state.Client=0;
        }));
        using(var peer=new TcpPeer(fixture.Pair.B,1883))
        {
            Connected(fixture,peer,state,arg,1883);
            fixture.Pair.A.Invoke(owner=>Require(owner.mqtt_sub_unsub((mqtt_client_s*)state.Client,(byte*)topic,1,&Request,(void*)arg,1)==0,"original MQTT subscribe"));
            byte[] request=Frame(fixture,peer),body=Body(request);Require(request[0]==0x82&&body.AsSpan(2).SequenceEqual(new byte[]{0,8}.Concat("dotcc/in"u8.ToArray()).Append((byte)1).ToArray()),"original SUBSCRIBE topic and QoS");
            peer.Send(fixture,new byte[]{0x90,3,body[0],body[1],1});
            fixture.Wait(()=>{lock(state.Gate)return state.Requests.Count==1;},state.Check);
            lock(state.Gate)Require(state.Requests[0]==0,"SUBACK completion");
            byte[] payload="copied notification"u8.ToArray();
            byte[] incoming=new byte[]{0,8}.Concat("dotcc/in"u8.ToArray()).Concat(new byte[]{0,57}).Concat(payload).ToArray();
            peer.Send(fixture,Packet(0x32,incoming));
            var ack=Frame(fixture,peer);Require(ack.AsSpan().SequenceEqual(new byte[]{0x40,2,0,57}),"original inbound QoS1 PUBACK");
            fixture.Wait(()=>{lock(state.Gate)return state.Last;},state.Check);
            lock(state.Gate)Require(state.Topic=="dotcc/in"&&state.Expected==payload.Length&&state.Payload.ToArray().SequenceEqual(payload),"copied MQTT topic/payload delivery");
            fixture.Pair.A.Invoke(owner=>{fixed(byte* bytes=payload)Require(owner.mqtt_publish((mqtt_client_s*)state.Client,(byte*)outTopic,bytes,(ushort)payload.Length,1,0,&Request,(void*)arg)==0,"original QoS1 publish");});
            var published=Frame(fixture,peer);body=Body(published);Require(published[0]==0x32&&BinaryPrimitives.ReadUInt16BigEndian(body)==9&&body.AsSpan(2,9).SequenceEqual("dotcc/out"u8)&&body.AsSpan(13).SequenceEqual(payload),"original outbound PUBLISH fields");
            peer.Send(fixture,new byte[]{0x40,2,body[11],body[12]});
            fixture.Wait(()=>{lock(state.Gate)return state.Requests.Count==2;},state.Check);
            fixture.Pair.A.Invoke(owner=>Require(owner.mqtt_sub_unsub((mqtt_client_s*)state.Client,(byte*)topic,0,&Request,(void*)arg,0)==0,"original unsubscribe"));
            request=Frame(fixture,peer);body=Body(request);Require(request[0]==0xa2,"original UNSUBSCRIBE");
            peer.Send(fixture,new byte[]{0xb0,2,body[0],body[1]});
            fixture.Wait(()=>{lock(state.Gate)return state.Requests.Count==3;},state.Check);
            lock(state.Gate)Require(state.Requests.All(result=>result==0),"original MQTT request results");
            peer.FinishSend();fixture.Wait(()=>{lock(state.Gate)return state.Connections.Count==2;},state.Check);
            lock(state.Gate)Require(state.Connections[1]==256,"ordinary broker disconnect notification");
            fixture.Wait(()=>peer.Finished,peer.Check);
        }
        using(var refused=new TcpPeer(fixture.Pair.B,1884))
        {
            Connected(fixture,refused,state,arg,1884,3);fixture.Wait(()=>refused.Finished,refused.Check);
            fixture.Pair.A.Invoke(owner=>Require(owner.mqtt_client_is_connected((mqtt_client_s*)state.Client)==0,"refused MQTT connection is disconnected"));
        }
        using(var pending=new TcpPeer(fixture.Pair.B,1885))
        {
            Connected(fixture,pending,state,arg,1885);
            fixture.Pair.A.Invoke(owner=>Require(owner.mqtt_sub_unsub((mqtt_client_s*)state.Client,(byte*)topic,1,&Request,(void*)arg,1)==0,"pending subscription admitted"));
            Require(Frame(fixture,pending)[0]==0x82,"broker receives pending subscription");
            int requests,connections;lock(state.Gate){requests=state.Requests.Count;connections=state.Connections.Count;}
            fixture.Pair.A.Invoke(owner=>owner.mqtt_disconnect((mqtt_client_s*)state.Client));
            fixture.Wait(()=>pending.Finished,pending.Check);fixture.Pair.Drain();
            // Upstream deletes pending requests and suppresses disconnect callback.
            // This explicitly records the contract; a product Task owner must settle
            // its own pending Tasks rather than await either original callback.
            lock(state.Gate)Require(state.Requests.Count==requests&&state.Connections.Count==connections,"explicit original disconnect suppresses callbacks and drains pending request state");
        }
        state.Check();
    }
}
