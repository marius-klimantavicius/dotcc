using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("socket query assertion failed");}
static InstanceIo Owner()=>new(new Dictionary<string,ReadOnlyMemory<byte>>());
var firstOwner=Owner();try{Blink.BindHostIo(firstOwner);Check(Blink.SocketQueriesProbe()==0 && Blink.SocketQueriesPrivate()==0);}finally{Blink.UnbindHostIo();firstOwner.DisposeAsync().GetAwaiter().GetResult();}
Check(Blink.SocketQueriesUnbound()==0);
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(int setting){var worker=new Thread(()=>{var owner=Owner();try{Blink.BindHostIo(owner);int fd=Blink.SocketQueriesPrepare(setting);Check(fd>=0);barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();Check(Blink.SocketQueriesObserve(fd,setting)==0 && Blink.blink_io_close(fd)==0);}catch(Exception e){Interlocked.CompareExchange(ref failure,e,null);}finally{Blink.UnbindHostIo();owner.DisposeAsync().GetAwaiter().GetResult();}});worker.Start();return worker;}
var first=Start(0);var second=Start(1);first.Join();second.Join();if(failure!=null)throw failure;
