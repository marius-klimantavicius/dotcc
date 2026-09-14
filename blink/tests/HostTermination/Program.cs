using System;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value) { if(!value)throw new Exception("Termination assertion failed"); }
Exception? failure=null;
Thread[] workers=new Thread[2];
for(int worker=0;worker<workers.Length;worker++) {
    int status=37+worker;
    workers[worker]=new Thread(()=> {
        try {
            for(int indirect=0;indirect<2;indirect++)for(int kind=0;kind<4;kind++) {
                try {
                    if(indirect!=0)Blink.ThroughPointer(kind,status);else Blink.RunExit(kind,status);
                    throw new Exception("Noreturn C operation returned");
                } catch(HostTerminationException terminated) {
                    Check(terminated.Kind==(kind==0 ? HostTerminationKind.Exit : kind==3 ? HostTerminationKind.Abort : HostTerminationKind.ImmediateExit));
                    Check(terminated.RequestedStatus==(kind==3 ? 0 : status));
                }
            }
        } catch(Exception error) { Interlocked.CompareExchange(ref failure,error,null); }
    }) { IsBackground=true };
}
foreach(var worker in workers)worker.Start();
GC.Collect(2,GCCollectionMode.Forced,true,true);
foreach(var worker in workers)Check(worker.Join(TimeSpan.FromSeconds(10)));
if(failure!=null)throw failure;
Console.WriteLine("host termination: direct and C function-pointer calls preserve kind/status and never return: PASS");
