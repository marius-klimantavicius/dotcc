using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Edg.Runtime;

static unsafe class Program
{
 static void Check(bool value,string label) { if(!value)throw new Exception(label); }
 static byte[] V1() {
  byte[] b=new byte[54];"TZif"u8.CopyTo(b);BinaryPrimitives.WriteUInt32BigEndian(b.AsSpan(36),1);BinaryPrimitives.WriteUInt32BigEndian(b.AsSpan(40),4);
  BinaryPrimitives.WriteInt32BigEndian(b.AsSpan(44),1234);"TST\0"u8.CopyTo(b.AsSpan(50));return b;
 }
 static byte[] V2(string footer) {
  byte[] v1=V1(),tail=Encoding.ASCII.GetBytes("\n"+footer+"\n"),b=new byte[v1.Length*2+tail.Length];v1[4]=(byte)'2';v1.CopyTo(b,0);v1.CopyTo(b,v1.Length);tail.CopyTo(b,v1.Length*2);return b;
 }
 static byte* Map(LibcTimezone service,string name,out ulong size) {
  byte[] path=Encoding.UTF8.GetBytes(name+"\0");ulong length=0;byte* result;
  fixed(byte* p=path)result=service.MapFile((sbyte*)p,&length);size=length;return result;
 }
 [MethodImpl(MethodImplOptions.NoInlining)] static void ReadPinned(byte* p,int length) {
  for(int i=0;i<10000;++i)Check(LibcTimezone.ValidateTzif(new ReadOnlySpan<byte>(p,length)),"hot validation");
 }
 static int Main(string[] args) {
  string directory=args[0];Directory.CreateDirectory(directory);int error=0,otherError=0;
  using var one=new LibcTimezone(x=>error=x);using var two=new LibcTimezone(x=>otherError=x);
  using Stream resource=typeof(LibcTimezoneData).Assembly.GetManifestResourceStream("Edg.Runtime.Tzdb.bin")!;
  byte[] bundle=new byte[resource.Length];resource.ReadExactly(bundle);uint count=BinaryPrimitives.ReadUInt32LittleEndian(bundle.AsSpan(8));int cursor=12;
  for(int i=0;i<count;++i) {
   int n=BinaryPrimitives.ReadUInt16LittleEndian(bundle.AsSpan(cursor));cursor+=2;string name=Encoding.ASCII.GetString(bundle,cursor,n);cursor+=n+8;
   byte* a=Map(one,"/usr/share/zoneinfo/"+name,out ulong size),b=Map(two,"/share/zoneinfo/"+name,out ulong second);
   Check(a!=null&&a==b&&size==second,"named "+name);Check(LibcTimezone.ValidateTzif(new ReadOnlySpan<byte>(a,(int)size)),"valid "+name);
   GC.Collect();GC.WaitForPendingFinalizers();Check(a[0]=='T'&&a[1]=='Z',"pinned "+name);
   Check(one.Unmap(a,size)==0&&two.Unmap(b,size)==0,"release "+name);
  }
  byte* utc=Map(one,"/etc/zoneinfo/UTC",out ulong utcSize),again=Map(one,"/etc/zoneinfo/UTC",out ulong againSize);
  Check(utc==again&&utcSize==againSize,"deduplicate");Check(one.Unmap(utc,utcSize)==0&&one.Unmap(utc,utcSize)==0,"reference count");
  Check(one.Unmap(utc,utcSize)==-1&&error==22,"stale mapping");Check(two.Unmap(utc,utcSize)==-1&&otherError==22,"owner check");
  ReadPinned(utc,(int)utcSize);long before=GC.GetAllocatedBytesForCurrentThread();ReadPinned(utc,(int)utcSize);Check(GC.GetAllocatedBytesForCurrentThread()==before,"allocation-free validation");
  Check(LibcTimezone.ValidateTzif(V1()),"v1");
  foreach(string footer in new[]{"","UTC0","EST5EDT,M3.2.0,M11.1.0","<+03>-3","AEST-10AEDT,M10.1.0,M4.1.0/3","STD0DST,J60/-1,J300/167:59:59","STD0DST,60,300"})Check(LibcTimezone.ValidateTzif(V2(footer)),"footer "+footer);
  foreach(string footer in new[]{"M","UTC","UTC999999999999","EST5EDT,M13.2.0,M11.1.0","EST5EDT,M3.6.0,M11.1.0","EST5EDT,M3.2.7,M11.1.0","EST5EDT,J0,J300","EST5EDT,366,300","EST5EDT,M3.2.0","EST5EDT,M3.2.0/168,M11.1.0","EST5EDT,M3.2.0/2:60,M11.1.0","EST5EDT,M3.2.0/2:30:60,M11.1.0","EST5EDT,M3.2.0,M11.1.0\nBAD","EST5EDT,M3.2.0,M11.1.0\0"})Check(!LibcTimezone.ValidateTzif(V2(footer)),"reject footer "+footer);
  byte[] valid=V2("UTC0");for(int i=0;i<valid.Length;++i)Check(!LibcTimezone.ValidateTzif(valid.AsSpan(0,i)),"truncated "+i);
  foreach(int field in new[]{20,24,28,32,36,40}) {byte[] bad=(byte[])valid.Clone();BinaryPrimitives.WriteUInt32BigEndian(bad.AsSpan(field),uint.MaxValue);Check(!LibcTimezone.ValidateTzif(bad),"overflow field "+field);}
  byte[] invalid=V1();invalid[49]=4;Check(!LibcTimezone.ValidateTzif(invalid),"bad abbreviation");invalid=V1();invalid[48]=2;Check(!LibcTimezone.ValidateTzif(invalid),"bad DST flag");
  string file=Path.Combine(directory,"synthetic-v1.tzif");File.WriteAllBytes(file,V1());byte* mapped=Map(one,file,out ulong length);
  Check(mapped!=null&&length==54&&mapped[4]=='1',"private v1 normalization");Check(File.ReadAllBytes(file)[4]==0,"file unchanged");
  GC.Collect();Check(mapped[44]==0&&mapped[47]==0xd2,"private view pinned");Check(one.Unmap(mapped,length)==0,"private release");
  string malformed=Path.Combine(directory,"malformed.tzif");File.WriteAllBytes(malformed,new byte[44]);error=123;Check(Map(one,malformed,out _)==null&&error==123,"invalid data preserves errno");
  Check(Map(one,Path.Combine(directory,"missing.tzif"),out _)==null&&error==2,"missing file errno");
  sbyte* broken=stackalloc sbyte[2];broken[0]=unchecked((sbyte)0xff);broken[1]=0;ulong zero=0;Check(one.MapFile(broken,&zero)==null&&error==84,"invalid UTF8");
  Parallel.For(0,100,i=>{one.Enter();try{byte* p=Map(one,"/usr/share/zoneinfo/Europe/Vilnius",out ulong size);Check(p!=null&&one.Unmap(p,size)==0,"parallel map");}finally{one.Exit();}});
  one.Dispose();bool disposed=false;try{one.Enter();}catch(ObjectDisposedException){disposed=true;}Check(disposed,"disposed lock");
  Console.WriteLine("PASS timezone boundary "+count+" names, malformed TZif, owner mappings, pinned GC, zero-allocation validation");return 0;
 }
}
