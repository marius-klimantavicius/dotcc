using System;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value) { if(!value)throw new Exception("Environment assertion failed"); }
var time=new ManualTime();
var host=new HostEnvironment(time,new CountingEntropy());
Blink.BindHostEnvironment(host);
Check(Blink.ClockSeconds(0)==1234 && Blink.ClockNanos(0)==567800);
time.Stamp=25_000_000;
Check(Blink.ClockSeconds(1)==2 && Blink.ClockNanos(1)==500000000);
Check(Blink.EntropySum()==120);
Check(Blink.HostErrors()==0);
byte[] large=new byte[1024];
Check(host.GetRandom(large).Value==256);
var failure=new HostEnvironment(time,new FailingEntropy());
Check(failure.GetRandom(large).Error==GuestError.Io);
Blink.UnbindHostEnvironment();
Check(Blink.ClockSeconds(0)==-1);
Blink.BindHostEnvironment(new HostEnvironment());
int result=Blink.main();
Blink.UnbindHostEnvironment();
return result;

sealed class ManualTime:TimeProvider
{
    public long Stamp;
    public override long TimestampFrequency=>10_000_000;
    public override long GetTimestamp()=>Stamp;
    public override DateTimeOffset GetUtcNow()=>DateTimeOffset.UnixEpoch.AddTicks(12_340_005_678);
}
sealed class CountingEntropy:IHostEntropy
{
    private byte next;
    public void Fill(Span<byte> destination) { foreach(ref byte value in destination)value=next++; }
}
sealed class FailingEntropy:IHostEntropy
{
    public void Fill(Span<byte> destination)=>throw new InvalidOperationException("injected provider failure");
}
