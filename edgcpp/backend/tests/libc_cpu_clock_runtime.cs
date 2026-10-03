using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Edg.Runtime;
static class Program
{
 static ulong sink;
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 [MethodImpl(MethodImplOptions.NoInlining)] static void Work() {ulong x=123;for(int i=0;i<1000000;++i)x=x*6364136223846793005UL+1;sink=x;}
 [MethodImpl(MethodImplOptions.NoInlining)] static long ReadLoop(){long total=0;for(int i=0;i<1000;++i)total+=LibcCpuClock.Clock();return total;}
 static void Main(){
  foreach(long user in new long[]{0,1,9,10,11,99,long.MaxValue})foreach(long kernel in new long[]{0,1,9,10,11,99,long.MaxValue})Check(LibcCpuClock.FromTicks(user,kernel)==(long)(((Int128)user+kernel)/10),"exact conversion");
  Check(LibcCpuClock.FromTicks(-1,0)==-1&&LibcCpuClock.FromTicks(0,-1)==-1,"invalid ticks");
  long initial=LibcCpuClock.Clock();Check(initial>=0,"CPU clock available");var timer=Stopwatch.StartNew();long previous=initial;
  do {Work();long current=LibcCpuClock.Clock();Check(current>=previous,"monotonic CPU");previous=current;}while(previous==initial&&timer.ElapsedMilliseconds<5000);
  Check(previous>initial,"CPU work observed");
  for(int i=0;i<1000;++i){var before=Environment.CpuUsage;long sample=LibcCpuClock.Clock();var after=Environment.CpuUsage;Check(sample>=LibcCpuClock.FromTicks(before.UserTime.Ticks,before.PrivilegedTime.Ticks)&&sample<=LibcCpuClock.FromTicks(after.UserTime.Ticks,after.PrivilegedTime.Ticks),"BCL CPU brackets");}
  ReadLoop();long bytes=GC.GetAllocatedBytesForCurrentThread();ReadLoop();Check(GC.GetAllocatedBytesForCurrentThread()==bytes,"allocation-free queries");
  GC.KeepAlive(sink);Console.WriteLine("PASS C CPU clock exact ticks, BCL bracket, monotonic work and zero-allocation queries");
 }
}
