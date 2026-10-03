using System;
using System.IO;
using System.Runtime.InteropServices;
using Edg.Runtime;
unsafe class Program
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(string[] args){
  int error=71;ulong encoding=4;
  using(var input=File.OpenRead(args[0]))using(var output=new FileStream(args[1],FileMode.CreateNew,FileAccess.Write))
  using(var files=new LibcFiles(e=>error=e,input:input,output:output,getEncoding:()=>encoding)){
   Check(files.Fwide(files.Stdin,0)==0 && files.Fwide(files.Stdout,0)==0,"initial orientation");
   Check(files.Fwide(files.Stdin,1)>0 && files.Fwide(files.Stdout,1)>0,"wide orientation");
   encoding=1; // The FILE encoding remains captured UTF8, independent of locale.
   for(int scalar=0;scalar<=0x10ffff;++scalar){if(scalar>=0xd800&&scalar<=0xdfff)continue;Check(files.Fgetwc(files.Stdin)==(uint)scalar,$"decode {scalar:x}");Check(files.Fputwc(scalar,files.Stdout)==(uint)scalar,$"encode {scalar:x}");}
   Check(files.Fgetwc(files.Stdin)==LibcFiles.Weof&&files.Feof(files.Stdin)==1&&files.Ferror(files.Stdin)==0,"clean EOF");
   Check(files.Ungetwc(0x20ac,files.Stdin)==0x20ac&&files.Feof(files.Stdin)==0,"wide pushback clears EOF");
   long at=files.Ftell(files.Stdin);Check(at==input.Length-3&&files.Fgetwc(files.Stdin)==0x20ac&&files.Ftell(files.Stdin)==input.Length,"pushback byte position");
   Check(files.Fgetc(files.Stdin)==-1&&error==22&&files.Ferror(files.Stdin)==1,"byte input on wide stream rejected");files.Clearerr(files.Stdin);
   Check(files.Fputc('x',files.Stdout)==-1&&error==22,"byte output on wide stream rejected");files.Clearerr(files.Stdout);
   Check(files.Fwide(files.Stdin,-1)>0,"orientation cannot be changed");
   Check(files.Fflush(files.Stdout)==0,"flush wide output");
  }
  Check(File.ReadAllBytes(args[0]).AsSpan().SequenceEqual(File.ReadAllBytes(args[1])),"every scalar native bytes");
  encoding=1;error=71;
  using(var input=new MemoryStream(new byte[]{65,0x80,0xff,0}))using(var output=new MemoryStream())
  using(var files=new LibcFiles(e=>error=e,input:input,output:output,getEncoding:()=>encoding)){
   Check(files.Fgetwc(files.Stdin)==65,"C ASCII");encoding=4;
   Check(files.Fgetwc(files.Stdin)==0xdf80&&files.Fgetwc(files.Stdin)==0xdfff&&files.Fgetwc(files.Stdin)==0,"C byte escapes captured");
   encoding=1;Check(files.Fputwc(0xdf80,files.Stdout)==0xdf80&&files.Fputwc(0xdfff,files.Stdout)==0xdfff,"C output escapes");
   Check(files.Fputwc(0x20ac,files.Stdout)==LibcFiles.Weof&&error==84,"C unrepresentable");files.Clearerr(files.Stdout);
   Check(files.Fflush(files.Stdout)==0&&output.ToArray().AsSpan().SequenceEqual(new byte[]{0x80,0xff}),"C escape bytes");
  }
  byte[] map=File.ReadAllBytes(args[2]);byte[] allBytes=new byte[256];for(int i=0;i<256;++i)allBytes[i]=(byte)i;
  using(var input=new MemoryStream(allBytes))using(var output=new MemoryStream())
  using(var files=new LibcFiles(e=>error=e,input:input,output:output,getEncoding:()=>1)){
   for(int i=0;i<256;++i){int expected=System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(map.AsSpan(i*4,4));Check(files.Fgetwc(files.Stdin)==(uint)expected,"original musl C input");Check(files.Fputwc(expected,files.Stdout)==(uint)expected,"original musl C output");}
   Check(files.Fflush(files.Stdout)==0&&output.ToArray().AsSpan().SequenceEqual(allBytes),"original musl all byte roundtrip");
  }
  encoding=4;
  using(var input=new MemoryStream())using(var files=new LibcFiles(e=>error=e,input:input,getEncoding:()=>encoding)){
   input.Write(new byte[]{0xe2,0x82});input.Position=0;
   Check(files.Fgetwc(files.Stdin)==LibcFiles.Weof&&error==84&&files.Feof(files.Stdin)==1,"truncated multibyte");
   LibcFiles.FilePosition saved;Check(files.FgetposState(files.Stdin,&saved)==0&&saved.Offset==2&&saved.State!=0,"position retains decoder state");
   input.Position=input.Length;input.WriteByte(0xac);files.Clearerr(files.Stdin);
   Check(files.FsetposState(files.Stdin,&saved)==0&&files.Fgetwc(files.Stdin)==0x20ac,"decoder replay after restoring position");
   Check(files.Fseek(files.Stdin,0,0)==0&&files.Fgetwc(files.Stdin)==0x20ac,"seek resets decoder");
  }
  using(var input=new MemoryStream(new byte[]{0xe2,(byte)'X',(byte)'Y'}))using(var files=new LibcFiles(e=>error=e,input:input,getEncoding:()=>4)){
   Check(files.Fgetwc(files.Stdin)==LibcFiles.Weof&&error==84,"invalid continuation");files.Clearerr(files.Stdin);
   Check(files.Fgetwc(files.Stdin)=='X'&&files.Fgetwc(files.Stdin)=='Y',"offending byte remains readable");
  }
  using(var input=new MemoryStream(new byte[]{(byte)'A',10,0xe2,0x82,0xac}))using(var output=new MemoryStream())
  using(var files=new LibcFiles(e=>error=e,input:input,output:output,getEncoding:()=>4)){
   int* buffer=stackalloc int[8];Check(files.Fgetws(buffer,8,files.Stdin)==buffer&&buffer[0]=='A'&&buffer[1]==10&&buffer[2]==0,"getws newline");
   Check(files.Fgetws(buffer,8,files.Stdin)==buffer&&buffer[0]==0x20ac&&buffer[1]==0,"getws UTF8");
   Check(files.Fputws(buffer,files.Stdout)==0&&files.Fflush(files.Stdout)==0&&output.ToArray().AsSpan().SequenceEqual(new byte[]{0xe2,0x82,0xac}),"putws UTF8");
  }
  using(var input=new MemoryStream(new byte[]{65,66}))using(var files=new LibcFiles(e=>error=e,input:input,getEncoding:()=>4)){
   Check(files.Fgetc(files.Stdin)==65&&files.Fwide(files.Stdin,0)<0,"byte orientation");
   Check(files.Fgetwc(files.Stdin)==LibcFiles.Weof&&error==22,"wide input on byte stream rejected");
  }
  using(var input=new MemoryStream(new byte[]{65}))using(var owner=new LibcFiles(e=>error=e,input:input,getEncoding:()=>4))using(var other=new LibcFiles(e=>error=e)){
   Check(other.Fgetwc(owner.Stdin)==LibcFiles.Weof&&error==9,"foreign owner");
   for(int i=0;i<1000;++i){owner.Fseek(owner.Stdin,0,0);owner.Fgetwc(owner.Stdin);}
   long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10000;++i){owner.Fseek(owner.Stdin,0,0);owner.Fgetwc(owner.Stdin);}
   Check(GC.GetAllocatedBytesForCurrentThread()==before,"wide read steady-state allocation");
  }
  Console.WriteLine("all Unicode scalar native byte roundtrip; orientation/captured locale/C escapes/pushback/errors/decoder position replay/strings/owner/zero-allocation passed");
 }
}
