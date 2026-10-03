using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using Edg.Runtime;
class Program {
 static ulong state=0x91264371ac681245UL,sink;
 static ulong Next(){state=unchecked(state*6364136223846793005UL+1);return state;}
 static bool NaN(Float80 x)=>(x.SignExponent&32767)==32767&&x.Significand!=0x8000000000000000UL;
 static void Equal(Float80 a,Float80 b){if(NaN(a)&&NaN(b))return;if(a.Significand!=b.Significand||a.SignExponent!=b.SignExponent)throw new Exception("integer/double to binary80 mismatch");}
 static ulong Bits(Float80 value,int precision,int min,int max,int bias,int expbits){
  bool neg=(value.SignExponent&32768)!=0;int exponent=value.SignExponent&32767;
  ulong sign=neg?1UL<<(precision-1+expbits):0,inf=((1UL<<expbits)-1)<<(precision-1);
  if(NaN(value))return sign|inf|(1UL<<(precision-2));if(exponent==32767)return sign|inf;if(value.Significand==0)return sign;
  int power=Math.Max(1,exponent)-16383-63,high=(int)((BigInteger)value.Significand).GetBitLength()-1+power;
  if(high>max)return sign|inf;
  int quantum=Math.Max(high-(precision-1),min-(precision-1)),drop=quantum-power;BigInteger mantissa=value.Significand;
  if(drop<=0)mantissa<<=-drop;else{mantissa=BigInteger.DivRem(mantissa,BigInteger.One<<drop,out var rem);var half=BigInteger.One<<(drop-1);if(rem>half||(rem==half&&!mantissa.IsEven))++mantissa;}
  if(mantissa.GetBitLength()>precision){mantissa>>=1;++quantum;}
  if(quantum+precision-1>max)return sign|inf;
  int exp=mantissa<(BigInteger.One<<(precision-1))?0:quantum+precision-1+bias;
  return sign|((ulong)exp<<(precision-1))|((ulong)mantissa&((1UL<<(precision-1))-1));
 }
 static void SameBits(ulong actual,ulong expected,int precision,int expbits){ulong mask=((1UL<<expbits)-1)<<(precision-1),frac=(1UL<<(precision-1))-1;if((actual&mask)==mask&&(expected&mask)==mask&&(actual&frac)!=0&&(expected&frac)!=0)return;if(actual!=expected)throw new Exception($"conversion bits {actual:x} != {expected:x}");}
 static void Out(Float80 value,ulong d,ulong f,ulong h){SameBits(BitConverter.DoubleToUInt64Bits((double)value),d,53,11);SameBits(BitConverter.SingleToUInt32Bits((float)value),f,24,8);SameBits(BitConverter.HalfToUInt16Bits((Half)value),h,11,5);}
 static void CheckInteger<T>(Float80 value,BigInteger expected,BigInteger min,BigInteger max,Func<Float80,T> cast,Func<T,BigInteger> convert){
  try{var got=cast(value);if(expected<min||expected>max||convert(got)!=expected)throw new Exception("integer conversion range/value");}
  catch(OverflowException){if(expected>=min&&expected<=max)throw;}
 }
 static void Integers(Float80 a){int power=Math.Max(1,a.SignExponent&32767)-16383-63;BigInteger v=power>=0?(BigInteger)a.Significand<<power:(BigInteger)a.Significand>>-power;if((a.SignExponent&32768)!=0)v=-v;
  CheckInteger(a,v,(BigInteger)Int128.MinValue,(BigInteger)Int128.MaxValue,x=>(Int128)x,x=>(BigInteger)x);
  CheckInteger(a,v,BigInteger.Zero,(BigInteger)UInt128.MaxValue,x=>(UInt128)x,x=>(BigInteger)x);
  CheckInteger(a,v,long.MinValue,long.MaxValue,x=>(long)x,x=>(BigInteger)x);CheckInteger(a,v,0,ulong.MaxValue,x=>(ulong)x,x=>(BigInteger)x);
  CheckInteger(a,v,int.MinValue,int.MaxValue,x=>(int)x,x=>(BigInteger)x);CheckInteger(a,v,0,uint.MaxValue,x=>(uint)x,x=>(BigInteger)x);
  CheckInteger(a,v,short.MinValue,short.MaxValue,x=>(short)x,x=>(BigInteger)x);CheckInteger(a,v,0,ushort.MaxValue,x=>(ushort)x,x=>(BigInteger)x);
  CheckInteger(a,v,sbyte.MinValue,sbyte.MaxValue,x=>(sbyte)x,x=>(BigInteger)x);CheckInteger(a,v,0,byte.MaxValue,x=>(byte)x,x=>(BigInteger)x);
 }
 static void Exercise(){ulong acc=0;for(int i=0;i<1000000;++i){Float80 a=(Int128)i;Float80 b=(UInt128)(uint)i;Float80 c=(long)-i;Float80 d=(ulong)(uint)i;Float80 e=(double)i+0.5;acc^=a.Significand^b.Significand^c.Significand^d.Significand^e.Significand;acc^=(ulong)(UInt128)b^(ulong)(long)c^(ulong)(uint)b^(ulong)(int)a^(ulong)(Int128)a;acc^=BitConverter.DoubleToUInt64Bits((double)e)^BitConverter.SingleToUInt32Bits((float)e)^BitConverter.HalfToUInt16Bits((Half)e);}sink=acc;}
 static void Main(string[] args){int count=0;foreach(var line in File.ReadLines(args[0])){var p=line.Split(' ');ulong V(int i)=>ulong.Parse(p[i],NumberStyles.HexNumber,CultureInfo.InvariantCulture);if(p[0]=="u"||p[0]=="i"){UInt128 u=((UInt128)V(1)<<64)|V(2);Equal(p[0]=="u"?(Float80)u:(Float80)unchecked((Int128)u),new Float80(V(3),(ushort)V(4)));}else if(p[0]=="d")Equal((Float80)BitConverter.UInt64BitsToDouble(V(1)),new Float80(V(2),(ushort)V(3)));else if(p[0]=="U"){if((UInt128)new Float80(V(1),(ushort)V(2))!=(((UInt128)V(3)<<64)|V(4)))throw new Exception("native UInt128 conversion");}else if(p[0]=="I"){if((Int128)new Float80(V(1),(ushort)V(2))!=unchecked((Int128)(((UInt128)V(3)<<64)|V(4))))throw new Exception("native Int128 conversion");}else Out(new Float80(V(1),(ushort)V(2)),V(3),V(4),V(5));++count;}
  for(int i=0;i<100000;++i){var a=new Float80(Next(),(ushort)Next());Out(a,Bits(a,53,-1022,1023,1023,11),Bits(a,24,-126,127,127,8),Bits(a,11,-14,15,15,5));if(i<2000)Integers(a);}
  foreach(int bit in new[]{0,7,8,15,16,31,32,63,64,126,127,128})foreach(ulong sig in new[]{0x8000000000000000UL,0x8000000000000001UL,0xffffffffffffffffUL})foreach(ushort sign in new ushort[]{0,32768})Integers(new Float80(sig,(ushort)(sign|16383+bit)));
  Integers(new Float80(0,0x7fff));Integers(new Float80(1,0));Integers(new Float80(1,0x8000));Integers(new Float80(0xc000000000000000UL,0xbffe));
  Equal((Float80)Int128.MinValue,new Float80(0x8000000000000000UL,0xC07E));Equal((Float80)long.MinValue,new Float80(0x8000000000000000UL,0xC03E));
  Exercise();long before=GC.GetAllocatedBytesForCurrentThread();Exercise();long allocated=GC.GetAllocatedBytesForCurrentThread()-before;if(allocated!=0)throw new Exception("conversions allocated "+allocated);Console.WriteLine($"native={count}; reference=100000; million-conversion-groups-allocation=0; sink={sink:x}");
 }
}
