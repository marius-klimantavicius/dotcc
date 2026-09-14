using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value) { if(!value)throw new Exception("terminal assertion failed"); }
static InstanceIo Owner()=>new(new Dictionary<string,ReadOnlyMemory<byte>> { ["/seed"]="seed"u8.ToArray() });
var host=Owner();Blink.BindHostIo(host);
foreach(int fd in new[]{0,1,2}) Check(Blink.TerminalDescriptor(fd,25)==0 && Blink.TerminalPrivate(fd)==0);
int file=host.OpenFile("/seed",FileAccessMode.Read).Value;
int socket=host.Socket().Value;
int duplicate=host.Duplicate(file).Value;
Check(Blink.TerminalDescriptor(file,25)==0 && Blink.TerminalDescriptor(socket,25)==0 && Blink.TerminalDescriptor(duplicate,25)==0);
Check(host.Close(file).Succeeded && Blink.TerminalDescriptor(file,9)==0 && Blink.TerminalDescriptor(duplicate,25)==0);
Check(Blink.TerminalDescriptor(-1,9)==0 && Blink.TerminalDescriptor(1234567,9)==0);
Check(Blink.SpeedProbe()==0);
host.DisposeAsync().AsTask().GetAwaiter().GetResult();Check(Blink.TerminalDescriptor(1,9)==0);
Blink.UnbindHostIo();Check(Blink.TerminalDescriptor(1,19)==0);
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(bool close){var worker=new Thread(()=>{
  var owner=Owner();
  try{
    Blink.BindHostIo(owner);if(close)Check(owner.Close(1).Succeeded);
    barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();
    Check(Blink.TerminalDescriptor(1,close?9:25)==0);
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
  finally{Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
});worker.Start();return worker;}
var a=Start(false);var b=Start(true);a.Join();b.Join();if(failure!=null)throw failure;
Console.WriteLine("nonterminal descriptor policy: PASS");
