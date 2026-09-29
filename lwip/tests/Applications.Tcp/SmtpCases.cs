using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;
using static Fixture;

internal static unsafe class SmtpCases
{
    sealed class State
    {
        internal readonly object Gate=new();
        internal int Calls;
        internal byte Result;
        internal ushort Server;
        internal sbyte Error;
        internal Exception? Failure;
        internal void Check(){lock(Gate)if(Failure!=null)throw new AggregateException("SMTP result callback failed",Failure);}
    }
    static void Result(Lwip owner,void* argument,byte result,ushort server,sbyte error)
    {
        var state=(State)GCHandle.FromIntPtr((nint)argument).Target!;
        try{lock(state.Gate){state.Calls++;Require(state.Calls==1,"one original SMTP result callback");state.Result=result;state.Server=server;state.Error=error;}}
        catch(Exception failure){lock(state.Gate)state.Failure=failure;}
    }
    static State Begin(Fixture fixture,ushort port,bool retained)
    {
        var state=new State();nint arg=fixture.Root(state);
        nint host=fixture.Text("10.0.0.2"),from=fixture.Text("sender@fixture.test"),to=fixture.Text("reader@fixture.test"),subject=fixture.Text("original SMTP fixture"),body=fixture.Text("line one\r\nline two");
        fixture.Pair.A.Invoke(owner=>
        {
            Require(owner.smtp_set_server_addr((byte*)host)==0,"numeric original SMTP server");owner.smtp_set_server_port(port);
            Require(owner.smtp_set_auth(null,null)==0,"original SMTP authentication disabled for cleartext fixture");
            int result=retained?owner.smtp_send_mail_static((byte*)from,(byte*)to,(byte*)subject,(byte*)body,&Result,(void*)arg)
                :owner.smtp_send_mail((byte*)from,(byte*)to,(byte*)subject,(byte*)body,&Result,(void*)arg);
            Require(result==0,"original SMTP submission");
        });
        return state;
    }
    static void Wait(Fixture fixture,State state,byte result,ushort server=0)
    {
        fixture.Wait(()=>{lock(state.Gate)return state.Calls!=0;},state.Check);
        lock(state.Gate)Require(state.Calls==1&&state.Result==result&&state.Server==server,"original SMTP terminal result");
    }
    static void Envelope(Fixture fixture,TcpPeer peer)
    {
        fixture.Wait(()=>peer.Connected,peer.Check);peer.Send(fixture,"220 fixture.test service ready\r\n");
        Require(peer.Line(fixture)=="EHLO [10.0.0.1]\r\n","original SMTP EHLO identifies client");
        peer.Send(fixture,"250 fixture.test\r\n");
        Require(peer.Line(fixture)=="MAIL FROM: <sender@fixture.test>\r\n","original SMTP envelope sender");
        peer.Send(fixture,"250 sender accepted\r\n");
        Require(peer.Line(fixture)=="RCPT TO: <reader@fixture.test>\r\n","original SMTP envelope recipient");
    }
    static void Success(Fixture fixture,ushort port,bool retained)
    {
        using var peer=new TcpPeer(fixture.Pair.B,port);var state=Begin(fixture,port,retained);
        Envelope(fixture,peer);peer.Send(fixture,"250 recipient accepted\r\n");
        Require(peer.Line(fixture)=="DATA\r\n","original SMTP DATA command");peer.Send(fixture,"354 End with CRLF dot CRLF\r\n");
        byte[] message=peer.Until(fixture,"\r\n.\r\n"u8.ToArray());
        Require(System.Text.Encoding.ASCII.GetString(message)=="From: <sender@fixture.test>\r\nTo: <reader@fixture.test>\r\nSubject: original SMTP fixture\r\n\r\nline one\r\nline two\r\n.\r\n","exact original SMTP header/body serialization");
        peer.Send(fixture,"250 message accepted\r\n");Require(peer.Line(fixture)=="QUIT\r\n","original SMTP QUIT");
        // Upstream signals success after queueing QUIT, before the peer's 221.
        Wait(fixture,state,0);peer.Send(fixture,"221 closing connection\r\n");peer.FinishSend();fixture.Wait(()=>peer.Finished,peer.Check);
        fixture.Pair.Drain();state.Check();lock(state.Gate)Require(state.Calls==1&&state.Error==0,"SMTP success callback remains single after FIN");
    }
    internal static void Run(Fixture fixture)
    {
        Success(fixture,2525,false);Success(fixture,2526,true);
        using(var refused=new TcpPeer(fixture.Pair.B,2527))
        {
            var state=Begin(fixture,2527,false);Envelope(fixture,refused);
            // A real negative server response, not a malformed input mutation.
            refused.Send(fixture,"550 Requested action not taken: mailbox unavailable\r\n");
            Wait(fixture,state,6,550);fixture.Wait(()=>refused.Finished,refused.Check);
        }
        using(var disconnected=new TcpPeer(fixture.Pair.B,2528))
        {
            var state=Begin(fixture,2528,true);fixture.Wait(()=>disconnected.Connected,disconnected.Check);
            // Ordinary server shutdown before the greeting. The static input and
            // callback root remain retained until original completion/core stop.
            disconnected.FinishSend();Wait(fixture,state,4);fixture.Wait(()=>disconnected.Finished,disconnected.Check);
        }
        // SMTP has no public session/cancel handle. Do not fake product cancel
        // by traversing PCBs or freeing callback roots while its session is live.
    }
}
