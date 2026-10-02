#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Edg.Runtime;

// Portable LP64 clock boundary. No native timespec layout crosses this adapter.
public sealed unsafe class LibcClock
{
    [StructLayout(LayoutKind.Sequential,Size=16)]
    public struct Timespec { public long Seconds,Nanoseconds; }
    private readonly Action<int> setErrno;
    public LibcClock(Action<int> setErrno)=>this.setErrno=setErrno;

    // Exposed for exact synthetic boundary tests. Floor division is required for
    // negative pre-epoch times; a floating-point timestamp loses low bits.
    public static Timespec FromCounter(long counter,long frequency)
    {
        if(frequency<=0)throw new ArgumentOutOfRangeException(nameof(frequency));
        long seconds=counter/frequency,remainder=counter%frequency;
        if(remainder<0){--seconds;remainder+=frequency;}
        return new Timespec {Seconds=seconds,Nanoseconds=(long)((Int128)remainder*1_000_000_000/frequency)};
    }
    private static Timespec Realtime()=>FromCounter(DateTimeOffset.UtcNow.UtcTicks-DateTimeOffset.UnixEpoch.UtcTicks,TimeSpan.TicksPerSecond);
    public int ClockGettime(int clock,void* result)
    {
        if(clock!=0 && clock!=1){setErrno(22);return -1;}
        if(result==null){setErrno(14);return -1;}
        Timespec value;
        if(clock==0)value=Realtime();
        else {
            // A DateTime fallback would violate the monotonic contract.
            if(!Stopwatch.IsHighResolution){setErrno(95);return -1;}
            long counter=Stopwatch.GetTimestamp();
            if(counter<0){setErrno(75);return -1;}
            value=FromCounter(counter,Stopwatch.Frequency);
        }
        *(Timespec*)result=value;return 0;
    }
    public int TimespecGet(void* result,int basis)
    {
        if(basis!=1)return 0;
        return ClockGettime(0,result)==0?1:0;
    }
    public long Time(long* result)
    {
        long seconds=Realtime().Seconds;
        if(result!=null)*result=seconds;
        return seconds;
    }
    // Difference in Int128 first: casting operands separately would discard
    // adjacent seconds at large time_t values; subtracting long could overflow.
    public static double Difftime(long end,long beginning)=>(double)((Int128)end-beginning);
}
