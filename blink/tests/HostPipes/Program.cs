using System;
using System.Collections.Generic;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool ok){if(!ok)throw new Exception("pipe boundary assertion failed");}
var image=new Dictionary<string,ReadOnlyMemory<byte>>();
var io=new InstanceIo(image);
try {
 Blink.BindHostIo(io);Check(Blink.PipeProbe()==0 && Blink.PipePrivate()==0);
 GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
 Check(io.OpenDescriptors==3 && io.PipeBytes==0);Blink.UnbindHostIo();
} finally {Blink.UnbindHostIo();io.DisposeAsync().AsTask().GetAwaiter().GetResult();}
var full=new InstanceIo(image,descriptorLimit:4);
try {Blink.BindHostIo(full);Check(Blink.PipeFailure(24)==0 && full.OpenDescriptors==3 && full.PipeBytes==0);}
finally{Blink.UnbindHostIo();full.DisposeAsync().AsTask().GetAwaiter().GetResult();}
var limited=new InstanceIo(image,pipeByteLimit:0);
try{Blink.BindHostIo(limited);Check(Blink.PipeFailure(12)==0 && limited.OpenDescriptors==3);}
finally{Blink.UnbindHostIo();limited.DisposeAsync().AsTask().GetAwaiter().GetResult();}
PipeModelChecks.Run().GetAwaiter().GetResult();
