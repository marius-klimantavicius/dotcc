using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Edg.Runtime;
static class Program {
 static ulong state=0x837720173133badfUL;
 static ulong Next(){state^=state<<13;state^=state>>7;state^=state<<17;return state;}
 static bool NaN(Float80 x)=>(x.SignExponent&32767)==32767 && x.Significand!=0x8000000000000000UL;
 static int Reference(Float80 a,Float80 b){
  int ae=a.SignExponent&32767,be=b.SignExponent&32767;
  bool an=(a.SignExponent&32768)!=0,bn=(b.SignExponent&32768)!=0;
  bool ai=ae==32767,bi=be==32767;
  if(!ai&&!bi&&a.Significand==0&&b.Significand==0)return 0;
  if(an!=bn)return an?-1:1;
  int c;
  if(ai||bi)c=ai?(bi?0:1):-1;
  else{int ap=Math.Max(1,ae),bp=Math.Max(1,be),p=Math.Min(ap,bp);c=((BigInteger)a.Significand<<(ap-p)).CompareTo((BigInteger)b.Significand<<(bp-p));}
  return an?-c:c;
 }
 [MethodImpl(MethodImplOptions.NoInlining)] static int Actual(Float80 a,Float80 b)=>(a==b?1:0)|(a!=b?2:0)|(a<b?4:0)|(a>b?8:0)|(a<=b?16:0)|(a>=b?32:0);
 static int Expected(Float80 a,Float80 b){if(NaN(a)||NaN(b))return 2;int c=Reference(a,b);return(c==0?1:2)|(c<0?4:0)|(c>0?8:0)|(c<=0?16:0)|(c>=0?32:0);}
 static void Check(Float80 a,Float80 b){if(Actual(a,b)!=Expected(a,b))throw new Exception($"compare {a.Significand:x16}:{a.SignExponent:x4} / {b.Significand:x16}:{b.SignExponent:x4}");}
 static int Main(string[] args){
  int native=0;foreach(string line in File.ReadLines(args[0])){var f=line.Split(' ');var a=new Float80(ulong.Parse(f[0],NumberStyles.HexNumber),ushort.Parse(f[1],NumberStyles.HexNumber));var b=new Float80(ulong.Parse(f[2],NumberStyles.HexNumber),ushort.Parse(f[3],NumberStyles.HexNumber));if(Actual(a,b)!=int.Parse(f[4],CultureInfo.InvariantCulture))throw new Exception("native "+line);Check(a,b);native++;}
  // Arbitrary finite encodings deliberately include pseudo-denormals and unnormal
  // numbers: preserve the provider's existing exact numerical interpretation.
  var pairs=new Float80[16384];for(int i=0;i<pairs.Length;i++)pairs[i]=new(Next(),(ushort)Next());
  for(int i=0;i<pairs.Length;i+=2){Check(pairs[i],pairs[i+1]);Check(pairs[i],pairs[i]);Check(pairs[i],new Float80(0,0));Check(pairs[i],new Float80(0,0x8000));}
  for(int e=2;e<32767;e+=71)for(int z=1;z<64;z++){
   ulong sig=Next()>>z;if(sig==0)sig=1;var a=new Float80(sig,(ushort)e);var b=new Float80(sig<<1,(ushort)(e-1));Check(a,b);Check(-a,-b);if(a!=b)throw new Exception("equivalent finite representations");
  }
  Check(new Float80(0x8000000000000000,0),new Float80(0x8000000000000000,1));
  ulong checksum=0;for(int i=0;i<20000;i++)checksum+=(uint)Actual(pairs[i&16383],pairs[(i+1)&16383]);
  long before=GC.GetAllocatedBytesForCurrentThread();
  for(int i=0;i<1000000;i++)checksum+=(uint)Actual(pairs[i&16383],pairs[(i+1)&16383]);
  long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
  if(allocated!=0)throw new Exception("comparison allocated "+allocated);
  Console.WriteLine($"{native} native vectors; exact arbitrary-encoding reference; 1000000 comparisons allocated {allocated} bytes; checksum {checksum}");return 0;
 }
}
