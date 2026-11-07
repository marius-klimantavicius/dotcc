namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal enum FullFloatExtendedOperation:ushort
    {HorizontalAdd=0x1001,HorizontalSubtract,AlternatingSubtractAdd,Reciprocal,ReciprocalSquareRoot}
    internal static bool CanInlineFloatExtended(JitInstruction i)=>i.Operation==JitOperation.VectorFloat&&
        (i.Options&0xffff) is >=0x1001 and <=0x1005;
    internal static bool ValidateFloatExtended(JitInstruction i)
    {
        if(!CanInlineFloatExtended(i))return false;
        var op=(FullFloatExtendedOperation)(i.Options&0xffff);bool scalar=(i.Options&FloatScalar)!=0,dbl=(i.Options&FloatDouble)!=0;
        bool estimate=op>=FullFloatExtendedOperation.Reciprocal;
        if(i.Length is <3 or >15||i.Width!=128||i.Target!=0||i.Condition!=0||i.Extra!=default||
            (i.Options&~(0xffffu|FloatDouble|FloatScalar))!=0||!VectorRegister(i.Destination)||
            !(VectorRegister(i.Source)||VectorMemory(i.Source))||estimate&&dbl||!estimate&&scalar||
            i.SourceWidth!=(scalar?32:128))throw new ArgumentException("Native extended float metadata");
        if(i.Source.IsMemory)
        {
            var a=i.Address;
            if(a.Base is <-1 or >15||a.Index is <-1 or >15||a.Scale is <0 or >3||
                a.AddressWidth is not(32 or 64)||a.Segment is <-1 or >5||a.RipRelative&&(a.Base!=-1||a.Index!=-1))
                throw new ArgumentException("Native extended float address");
        }
        else if(i.Address!=default)throw new ArgumentException("Native extended float unused address");
        if(!FloatLayout.Supported)throw new PlatformNotSupportedException("Native extended float layout");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryFloatExtendedOperation(in JitInstruction i)
        {
            if(!CanInlineFloatExtended(i))return false;
            var op=(FullFloatExtendedOperation)(i.Options&0xffff);
            bool scalar=(i.Options&FloatScalar)!=0,dbl=(i.Options&FloatDouble)!=0;
            if(i.Source.IsMemory)
            {
                ScalarMiscCheckCanonical(i,i.SourceWidth);
                if(i.SourceWidth==128)VectorMoveCheckAlignment(i);
                NativeMemoryPrepare(i,FullMemoryAccess.Read,i.SourceWidth);VectorMemoryLoad(i.SourceWidth);
            }
            else VectorGuestLoad(1,i.Source.Register);
            VectorGuestLoad(0,i.Destination.Register);
            if(op>=FullFloatExtendedOperation.Reciprocal)
            {
                // RCP/RSQRT are baseline SSE, not optional SSE3/SSE4. The ISA
                // specifies an approximation independent of RC with no SIMD
                // exceptions/sticky updates; native instructions implement the
                // required denormal-input/tiny-output rules without host traps.
                FloatMove(2,0);if(scalar)Bytes(0xf3);
                Bytes(0x0f,op==FullFloatExtendedOperation.Reciprocal?(byte)0x53:(byte)0x52,0xd1);
            }
            else
            {
                int width=dbl?64:32,lanes=128/width;
                Bytes(0x66,0x0f,0xef,0xd2);FloatBeginEnvironment();
                for(int lane=0;lane<lanes;++lane)
                {
                    bool alternating=op==FullFloatExtendedOperation.AlternatingSubtractAdd;
                    int leftVector=alternating?0:lane<lanes/2?0:1;
                    int rightVector=alternating?1:leftVector;
                    int leftLane=alternating?lane:lane%(lanes/2)*2;
                    int rightLane=alternating?lane:leftLane+1;
                    bool subtract=op==FullFloatExtendedOperation.HorizontalSubtract||alternating&&(lane&1)==0;
                    FloatExtract(3,leftVector,leftLane*width/8);FloatExtract(4,rightVector,rightLane*width/8);
                    FloatDaz(3,width);FloatDaz(4,width);FloatMxcsr(2,356);
                    FloatScalarCode(subtract?(byte)0x5c:(byte)0x58,3,4,width);FloatMxcsr(3,208);
                    FloatAdditiveTiny(width);
                    FloatAdditiveOverflow(subtract,leftVector,leftLane,rightVector,rightLane,width);
                    FloatAccumulateStatus();
                    FloatToInteger(0,3,width);FloatFromInteger(5,0,width);
                    if(lane!=0)Bytes(0x66,0x0f,0x73,0xfd,(byte)(lane*width/8));
                    Bytes(0x66,0x0f,0xeb,0xd5);
                }
                FloatFinishStatus();
            }
            VectorGuestStore(2,i.Destination.Register);
            if(i.Source.IsMemory)NativeMemoryCount();return true;
        }
    }
}
