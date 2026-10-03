using System;using System.Runtime.InteropServices;using System.Threading;using Edg.Runtime;
unsafe class Program{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static string Name(sbyte*p)=>Marshal.PtrToStringUTF8((nint)p)!;
 static long Batch(LibcLocale locale){long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100000;++i)locale.SetCategories(6,i&63);return GC.GetAllocatedBytesForCurrentThread()-before;}
 static void Main(){using var errno=new ThreadLocal<int>(()=>73);using var a=new LibcLocale(e=>errno.Value=e);using var b=new LibcLocale(e=>errno.Value=e);
 Check(Name(a.SetCategories(6,63))=="C.UTF-8" && errno.Value==73,"all categories");
 sbyte* saved=a.SetCategories(6,1);Check(Name(saved)=="C.UTF-8;C;C;C;C;C","mixed name");
 Check(Name(a.SetCategories(2,4))=="C.UTF-8" && a.Categories(null)==5,"single category preserves others");
 Check(a.SetCategories(7,0)==null && errno.Value==22 && a.Categories(null)==5,"invalid category atomic");
 Check(a.SetCategories(6,64)==null && errno.Value==22 && a.Categories(null)==5,"invalid value atomic");
 void*p=a.NewCategories(1,1,null);Check(p!=null && a.Categories(p)==1,"new selected categories");
 Check(a.NewCategories(4,4,p)==p && a.Categories(p)==5,"base preserves other categories");
 Check(a.NewCategories(64,0,p)==null && errno.Value==22 && a.Categories(p)==5,"invalid base modification atomic");
 Check(a.NewCategories(1,64,p)==null && errno.Value==22 && a.Categories(p)==5,"invalid values atomic");
 Check(b.NewCategories(1,0,p)==null && errno.Value==22 && a.Categories(p)==5,"foreign base rejected");
 Check(a.NewCategories(1,0,(void*)(nint)(-1))==null && errno.Value==22,"global base rejected");
 a.UseLocale(p);Check(a.NewCategories(1,0,p)==null && errno.Value==16 && a.Categories(p)==5,"selected base protected");
 a.SetCategories(6,0);Check(a.Utf8 && !b.Utf8,"thread override and owner state");
 Exception? failure=null;Thread t=new(()=>{try{Check(!a.Utf8,"worker follows global");a.UseLocale(p);Check(a.Utf8,"worker selects existing");a.ReleaseThread();}catch(Exception error){failure=error;}});t.Start();t.Join();if(failure!=null)throw failure;
 a.ReleaseThread();Check(!a.Utf8,"release returns global");a.FreeLocale(p);
 // Warm the actual measured path, including CLR monitor/counter initialization.
 Batch(a);Batch(a);Check(Batch(a)==0,"zero steady-state allocation category commits");
 GC.Collect(2,GCCollectionMode.Forced,true,true);Check(Name(saved)=="C.UTF-8;C;C;C;C;C","pinned immutable names");
 a.Dispose();bool disposed=false;try{a.SetCategories(6,0);}catch(ObjectDisposedException){disposed=true;}Check(disposed,"disposed guard");
 Console.WriteLine("locale category transaction, owner/TLS/base/pinning/lifetime and zero-allocation commits passed");}
}
