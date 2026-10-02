using System;
using System.Globalization;
using System.IO;
using Edg.Runtime;
unsafe class Program
{
    static ulong Hex(string value) => ulong.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    static void Equal(ulong actual, ulong expected, string detail) { if(actual!=expected)throw new Exception($"{detail}: {actual:x} != {expected:x}"); }
    static void Main(string[] args)
    {
        int count=0;
        foreach(string line in File.ReadLines(args[0])) {
            var p=line.Split(' ');int exponent=-123;
            if(p[0]=="L") {
                var value=new Float80(Hex(p[1]),(ushort)Hex(p[2]));
                var f=LibcFloat.Frexp(value,&exponent);var s=LibcFloat.ScaleB(value,int.Parse(p[3]));
                Equal(f.Significand,Hex(p[4]),line);Equal(f.SignExponent,Hex(p[5]),line);
                Equal((ulong)exponent,(ulong)int.Parse(p[6]),line);Equal(s.Significand,Hex(p[7]),line);Equal(s.SignExponent,Hex(p[8]),line);
                Equal((ulong)LibcFloat.Classify(value),(ulong)int.Parse(p[9]),line);Equal((ulong)LibcFloat.SignBit(value),(ulong)int.Parse(p[10]),line);
            } else if(p[0]=="D") {
                double value=BitConverter.UInt64BitsToDouble(Hex(p[1]));
                Equal(BitConverter.DoubleToUInt64Bits(LibcFloat.Frexp(value,&exponent)),Hex(p[3]),line);
                Equal((ulong)exponent,(ulong)int.Parse(p[4]),line);
                Equal(BitConverter.DoubleToUInt64Bits(LibcFloat.ScaleB(value,int.Parse(p[2]))),Hex(p[5]),line);
                Equal((ulong)LibcFloat.Classify(value),(ulong)int.Parse(p[6]),line);Equal((ulong)LibcFloat.SignBit(value),(ulong)int.Parse(p[7]),line);
            } else {
                float value=BitConverter.UInt32BitsToSingle((uint)Hex(p[1]));
                Equal(BitConverter.SingleToUInt32Bits(LibcFloat.Frexp(value,&exponent)),Hex(p[3]),line);
                Equal((ulong)exponent,(ulong)int.Parse(p[4]),line);
                Equal(BitConverter.SingleToUInt32Bits(LibcFloat.ScaleB(value,int.Parse(p[2]))),Hex(p[5]),line);
                Equal((ulong)LibcFloat.Classify(value),(ulong)int.Parse(p[6]),line);Equal((ulong)LibcFloat.SignBit(value),(ulong)int.Parse(p[7]),line);
            }
            ++count;
        }
        foreach(ushort sign in new ushort[]{0,0x8000}) {
            var infinity=new Float80(0x8000000000000000UL,(ushort)(sign|0x7fff));
            var nan=new Float80(0xc000000000000123UL,(ushort)(sign|0x7fff));int e=-1;
            foreach(var value in new[]{infinity,nan}) {
                Equal((ulong)LibcFloat.IsFinite(value),0,"nonfinite");Equal((ulong)LibcFloat.SignBit(value),(ulong)(sign>>15),"nonfinite sign");
                var f=LibcFloat.Frexp(value,&e);Equal(f.Significand,value.Significand,"nonfinite payload");Equal((ulong)e,0,"nonfinite exponent contract");
                var s=LibcFloat.ScaleB(value,int.MinValue);Equal(s.Significand,value.Significand,"nonfinite scale payload");Equal(s.SignExponent,value.SignExponent,"nonfinite scale sign");
            }
            Equal((ulong)LibcFloat.Classify(infinity),LibcFloat.Infinite,"infinity");Equal((ulong)LibcFloat.Classify(nan),LibcFloat.Nan,"NaN");
        }
        // Classification/decomposition/scaling use bounded integer operations,
        // including enormous exponents. Verify no per-operation GC allocation.
        var ordinary=new Float80(0x8123456789abcdefUL,16383);int output=0;
        for(int i=0;i<1000;++i)ordinary=LibcFloat.ScaleB(LibcFloat.Frexp(ordinary,&output),output);
        long before=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<10000;++i)ordinary=LibcFloat.ScaleB(LibcFloat.Frexp(ordinary,&output),output);
        Equal((ulong)(GC.GetAllocatedBytesForCurrentThread()-before),0,"helper allocation");
        Console.WriteLine($"{count} native floating vectors passed; binary80 special values and zero allocation passed");
    }
}
