using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value) { if(!value)throw new Exception("C I/O assertion failed"); }
var host=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>> { ["/seed"]="abcdef"u8.ToArray() }, "in!"u8.ToArray());
Blink.BindHostIo(host);
GC.Collect(2,GCCollectionMode.Forced,true,true);
int result=Blink.main();
Check(result==0);
Check(Blink.MissingProbe()==-1);
Check(Blink.StreamProbe()==0);
var captured=host.CapturedOutput;
Check(Encoding.UTF8.GetString(captured.StandardOutput)=="onetwo");
Check(Encoding.UTF8.GetString(captured.StandardError)=="err");
Blink.UnbindHostIo();
Check(Blink.MissingProbe()==-1);
host.DisposeAsync().AsTask().GetAwaiter().GetResult();
Exception? workerFailure=null;
Thread[] workers=new Thread[2];
for(int i=0;i<workers.Length;i++) workers[i]=new Thread(()=> {
    try {
        var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>> { ["/seed"]="abcdef"u8.ToArray() });
        Blink.BindHostIo(owner);
        Check(Blink.FileProbe()==0 && Blink.MissingProbe()==-1);
        Blink.UnbindHostIo();
        owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
    } catch(Exception failure) { Interlocked.CompareExchange(ref workerFailure,failure,null); }
});
foreach(var worker in workers)worker.Start();
foreach(var worker in workers)worker.Join();
if(workerFailure!=null)throw workerFailure;
return result;
