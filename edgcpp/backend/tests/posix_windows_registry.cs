using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;
using Edg.Runtime;
class Program
{
 static void Check(bool good,string text){if(!good)throw new Exception(text);}
 static PosixFlock Range(short type,long start=10,long len=10)=>new(){l_type=type,l_start=start,l_len=len};
 static void Set(object owner,SafeFileHandle fd,string path,short type,long start=10,long len=10,int command=6){var r=Range(type,start,len);PosixWindowsLocks.Control(owner,fd,path,command,ref r,null);}
 static Process Child(string path,string mode="hold"){string exe=Environment.ProcessPath!;var p=new ProcessStartInfo(exe){RedirectStandardOutput=true,RedirectStandardInput=true};if(Path.GetFileNameWithoutExtension(exe)=="dotnet")p.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory,"RegistryProbe.dll"));p.ArgumentList.Add(mode);p.ArgumentList.Add(path);return Process.Start(p)!;}
 static void Main(string[] args)
 {
  if(args.Length>0 && args[0]=="abandon")
  {
   using var file=PosixPlatform.Open(args[1],2,384);var stat=PosixPlatform.LockIdentity(file,args[1]);
   string registry=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Edg.Runtime","posix-locks-v1",stat.Device.ToString("x16")+"-"+stat.Inode.ToString("x16")+"-"+stat.High.ToString("x16")+".locks");
   string key=OperatingSystem.IsWindows()?registry.ToUpperInvariant():registry;
   using var mutex=new Mutex(false,"Edg.PosixLocks.v1."+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))),new NamedWaitHandleOptions{CurrentUserOnly=true,CurrentSessionOnly=false});
   mutex.WaitOne();File.WriteAllText(registry+".pending","interrupted snapshot");Console.WriteLine("held");Console.Out.Flush();Thread.Sleep(Timeout.Infinite);return;
  }
  if(args.Length>0){object o=new();using var file=PosixPlatform.Open(args[1],2,384);Set(o,file,args[1],1);Console.WriteLine("held");Console.Out.Flush();Console.ReadLine();PosixWindowsLocks.Close(o,file,args[1]);return;}
  string path=Path.Combine(Path.GetTempPath(),"edg-registry-"+Guid.NewGuid().ToString("N"));
  object a=new(),b=new();using var fa=PosixPlatform.Open(path,66,384);using var fb=PosixPlatform.Open(path,2,384);using var fa2=PosixPlatform.Open(path,2,384);
  try {
   Set(a,fa,path,0);Set(b,fb,path,0);try{Set(a,fa,path,1);throw new Exception("expected conflict");}catch(Exception e)when(PosixPlatform.Error(e)==11){}
   Set(b,fb,path,2);Set(a,fa,path,1);Set(a,fa,path,2,12,3);
   var query=Range(1,10,1);PosixWindowsLocks.Control(b,fb,path,5,ref query,null);Check(query.l_type==1&&query.l_start==10&&query.l_len==2,"split query");Set(b,fb,path,1,12,3);Set(b,fb,path,2,0,0);
   PosixWindowsLocks.Close(a,fa2,path);Set(b,fb,path,1);Set(b,fb,path,2,0,0);
   using(var child=Child(path)){
    Check(child.StandardOutput.ReadLine()=="held","child holds");query=Range(1);PosixWindowsLocks.Control(a,fa,path,5,ref query,null);Check(query.l_type==1&&query.l_pid==child.Id,"crossprocess query");
    try{Set(a,fa,path,1);throw new Exception("expected crossprocess conflict");}catch(Exception e)when(PosixPlatform.Error(e)==11){}
    child.Kill();child.WaitForExit();Set(a,fa,path,1);Set(a,fa,path,2,0,0);
   }
   Set(a,fa,path,1);
   using(var abandoned=Child(path,"abandon"))
   {
    Check(abandoned.StandardOutput.ReadLine()=="held","writer holds mutex");abandoned.Kill();abandoned.WaitForExit();
    query=Range(1);PosixWindowsLocks.Control(b,fb,path,5,ref query,null);Check(query.l_type==1&&query.l_pid==Environment.ProcessId,"abandoned writer preserves committed locks");
   }
   var started=new ManualResetEventSlim();var wait=Task.Run(()=>{started.Set();Set(b,fb,path,1,10,10,7);});started.Wait();Thread.Sleep(100);Check(!wait.IsCompleted,"blocks");Set(a,fa,path,2,0,0);Check(wait.Wait(5000),"unblocks");
   PosixWindowsLocks.Close(a,fa,path);PosixWindowsLocks.Close(b,fb,path);Console.WriteLine("Windows registry algorithm OK: ranges, close semantics, process conflict, crash cleanup, abandoned writer recovery, blocking");
  } finally{File.Delete(path);}
 }
}
