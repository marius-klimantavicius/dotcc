using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;
using static Fixture;

internal static unsafe class LwiperfCases
{
    sealed record Observation(int Kind,uint Bytes,uint Milliseconds,ushort Local,ushort Remote);
    sealed class State
    {
        internal readonly object Gate=new();
        internal readonly List<Observation> Reports=new();
        internal Exception? Failure;
        internal void Check(){lock(Gate)if(Failure!=null)throw new AggregateException("lwiperf report failed",Failure);}
    }
    static void Report(Lwip owner,void* argument,lwiperf_report_type kind,ip_addr* local,ushort localPort,ip_addr* remote,ushort remotePort,uint bytes,uint milliseconds,uint bandwidth)
    {
        var state=(State)GCHandle.FromIntPtr((nint)argument).Target!;
        try{lock(state.Gate){Require(local!=null&&remote!=null,"lwiperf report endpoints");state.Reports.Add(new((int)kind,bytes,milliseconds,localPort,remotePort));}}
        catch(Exception failure){lock(state.Gate)state.Failure=failure;}
    }
    internal static void Run(Fixture fixture)
    {
        var client=new State();nint clientArg=fixture.Root(client);
        using(var peer=new TcpPeer(fixture.Pair.B,5001))
        {
            // Bound capture even when the original sender fills successive windows.
            // Inspect the first 48 bytes, then count copied output without retaining it.
            var prefix=new List<byte>();object gate=new();peer.Discard=true;
            peer.Observe=bytes=>{lock(gate){int take=Math.Min(48-prefix.Count,bytes.Length);for(int i=0;i<take;i++)prefix.Add(bytes.Span[i]);}};
            uint now=LwipSysArch.Now(fixture.Pair.A.Core);LwipSysArch.SetClock(fixture.Pair.A.Core,()=>Volatile.Read(ref now));
            fixture.Pair.A.Invoke(owner=>
            {
                ip_addr address;Fixture.Address(owner,"10.0.0.2",&address);
                Require(owner.lwiperf_start_tcp_client(&address,5001,lwiperf_client_type.LWIPERF_CLIENT,&Report,(void*)clientArg)!=null,"original lwiperf client starts");
            });
            fixture.Wait(()=>{lock(gate)return prefix.Count==48;},()=>{peer.Check();client.Check();});
            lock(gate)
            {
                byte[] header=prefix.ToArray();Require(header.AsSpan(0,24).SequenceEqual(header.AsSpan(24,24)),"original lwiperf repeats initial settings");
                Require(BinaryPrimitives.ReadUInt32BigEndian(header)==0&&BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20))==-1000,"original unidirectional ten-second settings");
            }
            // The public API fixes duration at 10 seconds. Advance only its
            // existing host clock; original sent/poll/send_more close the session.
            now=unchecked(now+10001);fixture.Pair.A.Invoke(owner=>owner.sys_check_timeouts());
            fixture.Wait(()=>{lock(client.Gate)return client.Reports.Count!=0;},client.Check);
            fixture.Wait(()=>peer.Finished,peer.Check);
            lock(client.Gate)Require(client.Reports.Count==1&&client.Reports[0].Kind==1&&client.Reports[0].Bytes>=48&&client.Reports[0].Milliseconds>=10000,"original client time-limit completion report");
            Require(peer.Received>=48,"independent sink received original lwiperf data");
        }
        var server=new State();nint serverArg=fixture.Root(server);
        fixture.Pair.B.Invoke(owner=>{ip_addr address;Fixture.Address(owner,"10.0.0.2",&address);Require(owner.lwiperf_start_tcp_server(&address,5002,&Report,(void*)serverArg)!=null,"original lwiperf server starts");});
        // A second original client qualifies ordinary server completion. Keep the
        // server callback root until owner stop: original public abort is not yet
        // qualified to close/unregister its listener safely.
        var second=new State();nint secondArg=fixture.Root(second);
        uint later=LwipSysArch.Now(fixture.Pair.A.Core);LwipSysArch.SetClock(fixture.Pair.A.Core,()=>Volatile.Read(ref later));
        int observedData=0;uint? firstSequence=null;
        fixture.Pair.Observe=(toB,frame)=>
        {
            // Observe valid emitted IPv4/TCP traffic; never alter transport input.
            if(!toB||frame.Length<54||frame[12]!=8||frame[13]!=0||frame[23]!=6)return;
            int ipHeader=(frame[14]&15)*4,ipLength=BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(16));
            int tcp=14+ipHeader;if(ipHeader<20||frame.Length<14+ipLength||ipLength<ipHeader+20)return;
            int header=(frame[tcp+12]>>4)*4,payload=ipLength-ipHeader-header;
            if(header<20||payload<=0||BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(tcp+2))!=5002)return;
            uint sequence=BinaryPrimitives.ReadUInt32BigEndian(frame.AsSpan(tcp+4));firstSequence??=sequence;
            int extent=unchecked((int)(sequence-firstSequence.Value))+payload;
            // Output runs on A's serialized core; sequence extent counts a
            // retransmitted range once and IPv4 length excludes Ethernet padding.
            if(extent>Volatile.Read(ref observedData))Interlocked.Exchange(ref observedData,extent);
        };
        fixture.Pair.A.Invoke(owner=>
        {
            ip_addr address;Fixture.Address(owner,"10.0.0.2",&address);
            Require(owner.lwiperf_start_tcp_client(&address,5002,lwiperf_client_type.LWIPERF_CLIENT,&Report,(void*)secondArg)!=null,"original client connects to original server");
        });
        fixture.Wait(()=>Volatile.Read(ref observedData)>=48,second.Check);
        Console.WriteLine($"{{\"case\":\"lwiperf-clock-point\",\"observed_payload_extent\":{Volatile.Read(ref observedData)},\"advance_ms\":10001}}");
        later=unchecked(later+10001);fixture.Pair.A.Invoke(owner=>owner.sys_check_timeouts());
        fixture.Wait(()=>{lock(second.Gate)return second.Reports.Count!=0;},second.Check);
        fixture.Wait(()=>{lock(server.Gate)return server.Reports.Count!=0;},server.Check);
        lock(second.Gate)Require(second.Reports.Count==1&&second.Reports[0].Kind==1&&second.Reports[0].Bytes>=48,$"second original client completion: {string.Join(";",second.Reports)}");
        lock(server.Gate)Require(server.Reports.Count==1&&server.Reports[0].Kind==0&&server.Reports[0].Bytes>=24&&server.Reports[0].Local==5002,"original lwiperf server report after normal peer FIN");
        fixture.Pair.Observe=null;
        // This is whole-owner teardown coverage, not a safe lwiperf_abort claim.
    }
}
