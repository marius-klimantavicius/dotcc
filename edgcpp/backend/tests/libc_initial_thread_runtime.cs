#nullable enable
using System;
using System.Threading;
using Edg.Runtime;
unsafe sealed class Owner {
 internal readonly LibcxxThreads<Owner> Threads;
 internal int Leases,Mode; internal ulong Initial,Key,ExpectedLast,Parent;
 internal Owner()=>Threads=new(this,&Retain,&Release,&Enter,&Leave,&Tls,&Terminate);
 static void Retain(Owner o)=>Interlocked.Increment(ref o.Leases);
 static void Release(Owner o)=>Interlocked.Decrement(ref o.Leases);
 static void Enter(Owner o){}static void Leave(Owner o){}
 static void Tls(Owner o)=>o.Threads.ReleaseThread();
 static void Terminate(Owner o,Exception e){Console.Error.Write(e);Environment.Exit(91);}
 internal static void Require(bool ok){if(!ok)throw new Exception("contract");}
 internal static void Dtor(Owner o,void*p){Console.Write((nuint)p==1?"M":"W");if((nuint)p==1&&o.Mode==3)o.Threads.CThreadExit(41);}
 internal static void Last(Owner o){Require(o.Threads.CThreadCurrent()==o.ExpectedLast);Require(Volatile.Read(ref o.Leases)==0);Console.Write("A");Console.Out.Flush();Environment.Exit(0);}
 internal static int Child(Owner o,void*p){int result;Require(o.Threads.CThreadJoin(o.Parent,&result)==0&&result==77);Console.Write("C");o.ExpectedLast=o.Threads.CThreadCurrent();return 99;}
 internal static int Worker(Owner o,void*p){int result;Require(o.Threads.CThreadJoin(o.Initial,&result)==0&&result==(o.Mode==3?41:37));Console.Write("J");o.ExpectedLast=o.Threads.CThreadCurrent();Require(o.Threads.CTssSet(o.Key,(void*)2)==0);if(o.Mode==2){ulong child;o.Parent=o.Threads.CThreadCurrent();Require(o.Threads.CThreadCreate(&child,&Child,null)==0);}return 77;}
}
unsafe class Program {
 static void Main(string[]args){var o=new Owner();o.Mode=int.Parse(args[0]);o.Threads.EnterInitialThread(&Owner.Last);o.Initial=o.ExpectedLast=o.Threads.CThreadCurrent();ulong key;Owner.Require(o.Threads.CTssCreate(&key,&Owner.Dtor)==0);o.Key=key;Owner.Require(o.Threads.CTssSet(key,(void*)1)==0);
 if(o.Mode!=0){ulong worker;Owner.Require(o.Threads.CThreadCreate(&worker,&Owner.Worker,null)==0);}
 try{o.Threads.CThreadExit(37);}catch(CThreadExitTransfer exit)when(o.Threads.IsInitialThreadExit(exit)){o.Threads.ExitInitialThread(exit);}throw new Exception("exit returned");}
}
