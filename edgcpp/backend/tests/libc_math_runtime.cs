using System;
using System.Globalization;
using System.IO;
using Edg.Runtime;
class Program
{
 static ulong Hex(string value)=>ulong.Parse(value,NumberStyles.HexNumber,CultureInfo.InvariantCulture);
 static void Require(bool condition,string detail) {if(!condition)throw new Exception(detail);}
 static void Main(string[] args) {
  int error=0,count=0;var math=new LibcMath(value=>error=value);
  foreach(string line in File.ReadLines(args[0])) {
   var p=line.Split(' ');var value=new Float80(Hex(p[0]),(ushort)Hex(p[1]));error=71;
   var actual=new[]{math.Sqrt(value),LibcMath.Abs(value),LibcMath.Truncate(value),LibcMath.Floor(value),
    LibcMath.Ceiling(value),LibcMath.Round(value),LibcMath.CopySign(value,new Float80(0x8000000000000000,0xbfff))};
   for(int i=0;i<actual.Length;++i)Require(actual[i].Significand==Hex(p[2+2*i]) && actual[i].SignExponent==Hex(p[3+2*i]),
    $"operation {i}, vector {count}: {actual[i].Significand:x16} {actual[i].SignExponent:x4} versus {line}");
   Require(error==int.Parse(p[16]),$"errno vector {count}");++count;
  }
  Require(LibcMath.Abs(-123)==123 && LibcMath.Abs(-12345678901234567L)==12345678901234567L,"integer abs");
  Require(BitConverter.SingleToUInt32Bits(LibcMath.CopySign(BitConverter.UInt32BitsToSingle(0x7fc00012),-0f))==0xffc00012,"float NaN payload");
  Require(BitConverter.DoubleToUInt64Bits(LibcMath.CopySign(BitConverter.UInt64BitsToDouble(0x7ff8000000000123),-0d))==0xfff8000000000123,"double NaN payload");
  var x=new Float80(0x8123456789abcdef,16383);
  for(int i=0;i<1000;++i)x=math.Sqrt(x);
  long before=GC.GetAllocatedBytesForCurrentThread();
  for(int i=0;i<10000;++i)x=math.Sqrt(x);
  Require(GC.GetAllocatedBytesForCurrentThread()==before,"sqrt allocates");
  Console.WriteLine($"{count} native binary80 vectors passed for seven operations; errno, payloads and zero allocation passed");
 }
}
