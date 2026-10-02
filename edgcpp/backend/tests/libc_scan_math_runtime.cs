using System;using System.Globalization;using System.IO;using Edg.Runtime;
class Program {
 static ulong H(string x)=>ulong.Parse(x,NumberStyles.HexNumber);
 static void Main(string[] args) {
  int count=0;
  foreach(string line in File.ReadLines(args[0])) {var p=line.Split(' ');var r=LibcScanMath.Remainder(new(H(p[0]),(ushort)H(p[1])),new(H(p[2]),(ushort)H(p[3])));if(r.Significand!=H(p[4])||r.SignExponent!=H(p[5]))throw new Exception(line);++count;}
  var x=new Float80(0xfabcde0988765432,32766);var y=new Float80(0x8000000000001234,2);
  for(int i=0;i<100;++i)LibcScanMath.Remainder(x,y);
  long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;++i)LibcScanMath.Remainder(x,y);
  if(GC.GetAllocatedBytesForCurrentThread()!=before)throw new Exception("allocation");Console.WriteLine($"{count} finite binary80 remainder vectors and zero allocation passed");
 }
}
