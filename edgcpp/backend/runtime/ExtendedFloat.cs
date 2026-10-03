using System;
namespace Edg.Runtime;
public static class ExtendedFloat
{
    public static int SignBit(Float128 value) => (int)(value.Bits >> 127);
    public static int IsFinite(Float128 value) => Float128.IsFinite(value) ? 1 : 0;
    public static int Classify(Float128 value) => Float128.IsNaN(value) ? 0 :
        Float128.IsInfinity(value) ? 1 : Float128.IsZero(value) ? 2 : Float128.IsSubnormal(value) ? 3 : 4;
    public static int SignBit(Half value) => BitConverter.HalfToUInt16Bits(value) >> 15;
    public static int IsFinite(Half value) => Half.IsFinite(value) ? 1 : 0;
    public static int Classify(Half value) => Half.IsNaN(value) ? 0 : Half.IsInfinity(value) ? 1 :
        value == (Half)0 ? 2 : Half.IsSubnormal(value) ? 3 : 4;
    public static int Classify(Float80 value) => LibcFloat.Classify(value);
    public static int Classify(double value) => LibcFloat.Classify(value);
    public static int Classify(float value) => LibcFloat.Classify(value);
}
