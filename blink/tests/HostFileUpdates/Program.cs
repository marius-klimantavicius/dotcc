using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("file update assertion failed");}
static InstanceIo Owner(long limit)=>new(new Dictionary<string,ReadOnlyMemory<byte>>{{"/seed",new byte[]{1,2,3}}},writableLimit:limit);
var normal=Owner(64);try{Blink.BindHostIo(normal);Check(Blink.FileUpdateProbe()==0);}finally{Blink.UnbindHostIo();normal.DisposeAsync().GetAwaiter().GetResult();}
var small=Owner(8);try{Blink.BindHostIo(small);Check(Blink.FileUpdatePrivate()==0);}finally{Blink.UnbindHostIo();small.DisposeAsync().GetAwaiter().GetResult();}
Check(Blink.FileUpdateUnbound()==0);
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(int length){var worker=new Thread(()=>{var owner=Owner(8);try{Blink.BindHostIo(owner);barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();Check(Blink.FileUpdateWorker(length)==0);}catch(Exception e){Interlocked.CompareExchange(ref failure,e,null);}finally{Blink.UnbindHostIo();owner.DisposeAsync().GetAwaiter().GetResult();}});worker.Start();return worker;}
var first=Start(4);var second=Start(7);first.Join();second.Join();if(failure!=null)throw failure;
