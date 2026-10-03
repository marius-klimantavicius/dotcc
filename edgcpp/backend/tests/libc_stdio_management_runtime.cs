using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Edg.Runtime;
unsafe class Program {
 static void Check(bool value,string what){if(!value)throw new Exception(what);}
 static byte[] C(string value)=>Encoding.UTF8.GetBytes(value+"\0");
 static void* Open(LibcFiles f,string path,string mode){fixed(byte*p=C(path),m=C(mode))return f.Fopen((sbyte*)p,(sbyte*)m);}
 static void* Reopen(LibcFiles f,string? path,string mode,void*file){fixed(byte*p=path==null?null:C(path),m=C(mode))return f.Freopen((sbyte*)p,(sbyte*)m,file);}
 sealed class Nonseek : MemoryStream {public override bool CanSeek=>false;}
 sealed class Blocking : MemoryStream {
  internal readonly ManualResetEventSlim Entered=new(false),Proceed=new(false);
  public override void Write(ReadOnlySpan<byte> bytes){Entered.Set();Check(Proceed.Wait(10000),"write unblock");base.Write(bytes);}
 }
 [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
 static (LibcFiles,WeakReference) DisposedName(){var owner=new LibcFiles(_=>{});Check(owner.Tmpnam(null)!=null,"disposal name");var weak=owner.TemporaryNameWeakReference();owner.Dispose();return(owner,weak);}
 static int Main(string[]args){string dir=Path.GetFullPath(args[0]);Directory.CreateDirectory(dir);string temp=Path.Combine(dir,"temp");Directory.CreateDirectory(temp);Environment.SetEnvironmentVariable("TMPDIR",temp);Environment.SetEnvironmentVariable("TMP",temp);Environment.SetEnvironmentVariable("TEMP",temp);
 int error=0;using var output=new MemoryStream();using var diagnostics=new MemoryStream();ulong encoding=1;using var f=new LibcFiles(e=>error=e,output:output,error:diagnostics,workingDirectory:dir,getEncoding:()=>encoding);
 if(args.Length>1){void*t=f.Tmpfile();Check(t!=null,"exit tmpfile");f.Fputc(88,t);f.Fflush(t);Check(Directory.GetFileSystemEntries(temp).Length==0,"unlinked before process exit");Environment.Exit(37);}
 void*file=Open(f,"first.bin","w+");Check(file!=null,"fopen");sbyte*buffer=stackalloc sbyte[16];Check(f.Setvbuf(file,buffer,0,16)==0,"user buffer");Check(f.Fputc(65,file)==65,"old pending");Check(Reopen(f,"second.bin","w+",file)==file&&File.ReadAllText(Path.Combine(dir,"first.bin"))=="A","identity and old flush");Check(f.Fwide(file,0)==0&&f.Ferror(file)==0&&f.Feof(file)==0,"new association state");Check(f.Fputc(66,file)==66&&f.Fflush(file)==0&&File.ReadAllText(Path.Combine(dir,"second.bin"))=="B","new association output");
 Check(Reopen(f,null,"a+",file)==file&&f.Fseek(file,0,0)==0&&f.Fputc(67,file)==67&&f.Fflush(file)==0&&File.ReadAllText(Path.Combine(dir,"second.bin"))=="BC","enable atomic append");
 Check(Reopen(f,null,"r+",file)==file&&f.Fseek(file,0,0)==0&&f.Fputc(88,file)==88&&f.Fflush(file)==0&&File.ReadAllText(Path.Combine(dir,"second.bin"))=="XC","disable atomic append");
 Check(f.Fwide(file,0)<0&&Reopen(f,"second.bin","r",file)==file&&f.Fwide(file,0)==0&&f.Fgetwc(file)==88,"orientation reset");
 Check(Reopen(f,"missing/none.bin","r",file)==null&&error==2,"failed reopen errno");Check(f.Fgetc(file)==-1&&error==9&&f.Fclose(file)==-1,"failed reopen closes original");
 void*standard=f.Stdout;Check(Reopen(f,"standard.bin","w",standard)==standard&&f.Stdout==standard,"standard cell identity");Check(f.Fputc(90,standard)==90&&f.Fflush(standard)==0&&output.CanWrite,"host stream remains owned by host");
 Check(f.Fwide(f.Stderr,1)==1,"orient stderr C");encoding=4;fixed(byte*message=C("prefix"),text=C("error"))f.Perror(f.Stderr,(sbyte*)message,(sbyte*)text);Check(f.Fwide(f.Stderr,0)==1&&f.Fputwc(0xdf80,f.Stderr)==0xdf80,"perror orientation/encoding preserved");Check(Convert.ToHexString(diagnostics.ToArray())=="7072656669783A206572726F720A80","perror bytes");
 using(var input=new Nonseek())using(var nonseek=new LibcFiles(e=>error=e,input:input)){Check(nonseek.Fgetc(nonseek.Stdin)==-1&&nonseek.Feof(nonseek.Stdin)==1,"nonseek EOF");nonseek.Fputc(1,nonseek.Stdin);nonseek.Rewind(nonseek.Stdin);Check(error==29&&nonseek.Ferror(nonseek.Stdin)==0&&nonseek.Feof(nonseek.Stdin)==1,"rewind failed seek clears error only");}
 sbyte* name=f.Tmpnam(null);Check(name!=null,"tmpnam");string firstName=new string(name);Check(firstName.Length==19&&!File.Exists(Path.Combine(dir,firstName)),"tmpnam availability/length");Check(f.Tmpnam(null)==name&&new string(name)!=firstName,"tmpnam static storage/distinct names");sbyte* userName=stackalloc sbyte[20];Check(f.Tmpnam(userName)==userName&&new string(userName)!=new string(name),"tmpnam caller storage");
 using(var other=new LibcFiles(e=>error=e,workingDirectory:dir)){Check(other.Tmpnam(null)!=name,"tmpnam owner storage");}
 void*tfile=f.Tmpfile();Check(tfile!=null&&Directory.GetFileSystemEntries(temp).Length==0,"anonymous temp");Check(f.Fputc(255,tfile)==255&&f.Fseek(tfile,0,0)==0&&f.Fgetc(tfile)==255&&f.Fclose(tfile)==0,"temp byte IO");
 using(var blocked=new Blocking())using(var concurrent=new LibcFiles(e=>error=e,output:blocked,workingDirectory:dir)){
  concurrent.Setbuf(concurrent.Stdout,null);var write=Task.Run(()=>concurrent.Fputc(65,concurrent.Stdout));Check(blocked.Entered.Wait(5000),"blocked writer entered");
  var reopen=Task.Run(()=>(nint)Reopen(concurrent,"concurrent.bin","w",concurrent.Stdout));Check(!reopen.Wait(100),"freopen waits FILE gate");blocked.Proceed.Set();Check(write.Result==65&&reopen.Result==(nint)concurrent.Stdout,"concurrent completion");Check(concurrent.Fputc(66,concurrent.Stdout)==66&&concurrent.Fflush(concurrent.Stdout)==0&&File.ReadAllText(Path.Combine(dir,"concurrent.bin"))=="B","queued reopen observed");}
 // Service disposal marks the owner unavailable before waiting for stream
 // gates. A pending reopen must either fail closed or publish an association
 // which disposal subsequently closes, without closing the host's old stream.
 using(var blocked=new Blocking()){
  var closing=new LibcFiles(e=>error=e,output:blocked,workingDirectory:dir);void* token=closing.Stdout;closing.Setbuf(token,null);
  var writer=Task.Run(()=>closing.Fputc(65,token));Check(blocked.Entered.Wait(5000),"dispose writer entered");
  var reopening=Task.Run(()=>(nint)Reopen(closing,"closing.bin","w",token));
  var disposal=Task.Run(()=>closing.Dispose());Check(!disposal.Wait(100),"dispose waits FILE gate");blocked.Proceed.Set();Task.WaitAll(writer,reopening,disposal);
  Check(blocked.CanWrite&&closing.Fputc(1,token)==-1,"dispose closes reopened association only");
  string path=Path.Combine(dir,"closing.bin");if(File.Exists(path)){using var exclusive=File.Open(path,FileMode.Open,FileAccess.ReadWrite,FileShare.None);}
 }
 var released=DisposedName();GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();GC.Collect(2,GCCollectionMode.Forced,true,true);Check(!released.Item2.IsAlive,"disposed owner releases pinned tmpnam storage");GC.KeepAlive(released.Item1);
 Console.WriteLine("stdio reopen identity/append/orientation/failure/locking, perror, rewind, anonymous temporary file passed");return 0;
 }
}

namespace Edg.Runtime {public sealed unsafe partial class LibcFiles {internal WeakReference TemporaryNameWeakReference()=>new WeakReference(temporaryName!);}}
