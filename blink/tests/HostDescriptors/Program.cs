using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("descriptor assertion failed");}
static InstanceIo Owner(int limit)=>new(new Dictionary<string,ReadOnlyMemory<byte>>(),descriptorLimit:limit);
var normal=Owner(16);try{Blink.BindHostIo(normal);Check(Blink.DescriptorProbe()==0);}finally{Blink.UnbindHostIo();normal.DisposeAsync().GetAwaiter().GetResult();}
var full=Owner(6);try{Blink.BindHostIo(full);Check(Blink.DescriptorPrivate()==0 && full.OpenDescriptors==3);}finally{Blink.UnbindHostIo();full.DisposeAsync().GetAwaiter().GetResult();}
Check(Blink.DescriptorUnbound()==0);
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(){var worker=new Thread(()=>{var owner=Owner(6);try{Blink.BindHostIo(owner);barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();Check(Blink.DescriptorPrivate()==0);}catch(Exception e){Interlocked.CompareExchange(ref failure,e,null);}finally{Blink.UnbindHostIo();owner.DisposeAsync().GetAwaiter().GetResult();}});worker.Start();return worker;}
var first=Start();var second=Start();first.Join();second.Join();if(failure!=null)throw failure;
var socketOwner=Owner(16);
try {
  int original=socketOwner.Socket().Value;
  var endpoint=new GuestEndpoint(GuestEndpoint.Loopback,43210);
  Check(socketOwner.Bind(original,endpoint).Succeeded);
  Check(socketOwner.DuplicateTo(original,9).Succeeded && socketOwner.Close(original).Succeeded);
  Check(socketOwner.Listen(9,1).Succeeded);
  int replacement=socketOwner.Socket().Value;
  Check(socketOwner.Bind(replacement,endpoint).Error==GuestError.AddressInUse);
  GC.Collect(2,GCCollectionMode.Forced,true,true);
  Check(socketOwner.DuplicateTo(replacement,9).Succeeded);
  Check(socketOwner.Bind(9,endpoint).Succeeded && socketOwner.Close(replacement).Succeeded && socketOwner.Listen(9,1).Succeeded);
  Check(socketOwner.Close(9).Succeeded);
  Check(socketOwner.DuplicateTo(1,2).Succeeded);
  Check(socketOwner.WriteAsync(2,new byte[]{65}).GetAwaiter().GetResult().Value==1);
  var output=socketOwner.CapturedOutput;Check(output.StandardOutput.Length==1 && output.StandardError.Length==0);
} finally {socketOwner.DisposeAsync().GetAwaiter().GetResult();}
