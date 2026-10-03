using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using Edg.Runtime;
class Program {
 static ulong state=0x14c85a634f981267UL,sink;
 static ulong Next(){state=unchecked(state*6364136223846793005UL+1);return state;}
 static bool NaN(Float80 x)=>(x.SignExponent&32767)==32767&&x.Significand!=0x8000000000000000UL;
 static Float80 Reference(Float80 a,Float80 b){
  bool negative=((a.SignExponent^b.SignExponent)&32768)!=0;ushort sign=(ushort)(negative?32768:0);
  int ae=a.SignExponent&32767,be=b.SignExponent&32767;
  bool az=ae!=32767&&a.Significand==0,bz=be!=32767&&b.Significand==0;
  if(NaN(a)||NaN(b)||(ae==32767&&be==32767)||(az&&bz))return new(0xc000000000000000UL,32767);
  if(ae==32767||bz)return new(0x8000000000000000UL,(ushort)(sign|32767));
  if(az||be==32767)return new(0,sign);
  BigInteger numerator=a.Significand,denominator=b.Significand;
  int ratio=(int)numerator.GetBitLength()-(int)denominator.GetBitLength();
  if(ratio>=0?numerator<(denominator<<ratio):(numerator<<-ratio)<denominator)--ratio;
  int power=Math.Max(1,ae)-Math.Max(1,be),high=ratio+power;
  if(high>16383)return new(0x8000000000000000UL,(ushort)(sign|32767));
  int quantum=Math.Max(high-63,-16445),shift=power-quantum;
  if(shift>=0)numerator<<=shift;else denominator<<=-shift;
  var q=BigInteger.DivRem(numerator,denominator,out var rem);
  if((rem<<1)>denominator||((rem<<1)==denominator&&!q.IsEven))++q;
  if(q.IsZero)return new(0,sign);
  if(q.GetBitLength()>64){q>>=1;++quantum;}
  if(quantum>16320)return new(0x8000000000000000UL,(ushort)(sign|32767));
  int exp=q<(BigInteger.One<<63)?0:quantum+16383+63;
  return new((ulong)q,(ushort)(sign|exp));
 }
 static void Equal(Float80 actual,Float80 expected){if(NaN(actual)&&NaN(expected))return;if(actual.Significand!=expected.Significand||actual.SignExponent!=expected.SignExponent)throw new Exception($"{actual.Significand:x}:{actual.SignExponent:x} != {expected.Significand:x}:{expected.SignExponent:x}");}
 static void Exercise(){var a=new Float80(0x9abc123456789defUL,16382);var b=new Float80(0xa456789123abcdefUL,16384);ulong result=0;for(int i=0;i<1000000;++i){var x=a/b;result^=x.Significand;a=new Float80(a.Significand+1,a.SignExponent);}sink=result;}
 static void Main(string[] args){int count=0;foreach(var line in File.ReadLines(args[0])){var p=line.Split(' ');ulong V(int i)=>ulong.Parse(p[i],NumberStyles.HexNumber,CultureInfo.InvariantCulture);Equal(new Float80(V(0),(ushort)V(1))/new Float80(V(2),(ushort)V(3)),new Float80(V(4),(ushort)V(5)));++count;}
  for(int i=0;i<100000;++i){var a=new Float80(Next(),(ushort)Next());var b=new Float80(Next(),(ushort)Next());Equal(a/b,Reference(a,b));}
  Exercise();long before=GC.GetAllocatedBytesForCurrentThread();Exercise();long allocated=GC.GetAllocatedBytesForCurrentThread()-before;if(allocated!=0)throw new Exception("division allocated "+allocated);
  Console.WriteLine($"native={count}; raw-reference=100000; million-divide-allocation=0; sink={sink:x}");
 }
}
