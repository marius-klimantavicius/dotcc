using System;using System.IO;using System.Text;using Edg.Runtime;
unsafe class Program {
static void Write(LibcFiles f,int line=23){fixed(byte*e="counter == 7\0"u8, file="synthetic.c\0"u8,fn="probe\0"u8)f.WriteAssertion((sbyte*)e,(sbyte*)file,line,(sbyte*)fn);}
static void Main(string[] args){
if(args.Length!=0){var fail=new LibcFiles(_=>{},error:args[0]=="throw"?new Broken():Console.OpenStandardError());try{fixed(byte*e="counter == 7\0"u8,file="synthetic.c\0"u8,fn="probe\0"u8)fail.AssertFail((sbyte*)e,(sbyte*)file,23,(sbyte*)fn);}catch{Console.WriteLine("caught");}finally{Console.WriteLine("unwound");}return;}
using var first=new MemoryStream();using var second=new MemoryStream();using var f=new LibcFiles(_=>{},output:second,error:first);Write(f);
if(Encoding.UTF8.GetString(first.ToArray())!="Assertion failed: counter == 7 (synthetic.c: probe: 23)\n")throw new Exception("diagnostic");
*f.StderrLocation=f.Stdout;if(f.Fwide(f.Stderr,1)!=1)throw new Exception("orientation");Write(f,int.MinValue);
if(Encoding.UTF8.GetString(second.ToArray())!="Assertion failed: counter == 7 (synthetic.c: probe: -2147483648)\n")throw new Exception("redirect/wide");
second.Capacity=100000;for(int i=0;i<10;i++)Write(f);long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10;i++)Write(f);if(GC.GetAllocatedBytesForCurrentThread()!=before)throw new Exception("temporary allocation");Console.WriteLine("owner diagnostic, orientation, numeric bound, zero temporary allocation passed");}
class Broken:Stream {public override bool CanRead=>false;public override bool CanWrite=>true;public override bool CanSeek=>false;public override long Length=>0;public override long Position{get=>0;set=>throw new NotSupportedException();}public override void Flush()=>throw new InvalidOperationException("diagnostic failure");public override int Read(byte[]b,int o,int c)=>throw new NotSupportedException();public override void Write(byte[]b,int o,int c)=>throw new InvalidOperationException("diagnostic failure");public override long Seek(long o,SeekOrigin s)=>throw new NotSupportedException();public override void SetLength(long v)=>throw new NotSupportedException();}
}
