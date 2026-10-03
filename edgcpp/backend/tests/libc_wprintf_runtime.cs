using System;
using System.IO;
using System.Threading.Tasks;
using Edg.Runtime;
unsafe sealed class Owner {
 internal LibcFiles Files;
 internal Owner(LibcFiles files){Files=files;}
 internal static int Write(Owner owner,void*ctx,int*format,VaList*args){return owner.Files.Fputwc(0x20ac,ctx)==0x20ac?1:-1;}
 internal static int Fail(Owner owner,void*ctx,int*format,VaList*args){owner.Files.Fputwc(0xd800,ctx);return 1;}
 internal static int Throw(Owner owner,void*ctx,int*format,VaList*args){throw new InvalidOperationException("synthetic callback");}
}
unsafe class Program {
 static void Check(bool value){if(!value)throw new Exception("wide format session");}
 static void Main(){int error=0;using var output=new MemoryStream();using var files=new LibcFiles(e=>error=e,output:output,getEncoding:()=>4);var owner=new Owner(files);
 Check(files.Fgetwc(files.Stdout)==LibcFiles.Weof&&files.Ferror(files.Stdout)==1);
 Check(files.WithFileWideLock(files.Stdout,&Owner.Write,owner,files.Stdout,null,null)==1&&files.Ferror(files.Stdout)==1);
 files.Clearerr(files.Stdout);Check(files.WithFileWideLock(files.Stdout,&Owner.Fail,owner,files.Stdout,null,null)==-1&&files.Ferror(files.Stdout)==1&&error==84);
 try{files.WithFileWideLock(files.Stdout,&Owner.Throw,owner,files.Stdout,null,null);throw new Exception("missing throw");}catch(InvalidOperationException){}
 Check(files.Ferror(files.Stdout)==1);
 Check(Task.Run(()=>files.WithFileWideLock(files.Stdout,&Owner.Write,owner,files.Stdout,null,null)).Wait(5000));
 files.Clearerr(files.Stdout);Check(files.Fflush(files.Stdout)==0);Check(Convert.ToHexString(output.ToArray())=="E282ACE282AC");
 long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100;i++)Check(files.WithFileWideLock(files.Stdout,&Owner.Write,owner,files.Stdout,null,null)==1);Check(GC.GetAllocatedBytesForCurrentThread()==before);
 Console.WriteLine("wide format lock, old/new error, exceptional release, cross-thread reuse, zero steady allocation passed");}
}
