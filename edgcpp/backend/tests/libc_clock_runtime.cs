using System;using System.Diagnostics;using System.IO;using Edg.Runtime;
unsafe class Program {
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);}
 static void Main(string[] args){
  int count=0;foreach(string line in File.ReadLines(args[0])){
   string[] values=line.Split(' ');long counter=long.Parse(values[0]),frequency=long.Parse(values[1]);
   var actual=LibcClock.FromCounter(counter,frequency);Check(actual.Seconds==long.Parse(values[2]) && actual.Nanoseconds==long.Parse(values[3]),"exact counter conversion");++count;
  }
  int error=71,otherError=91;var clock=new LibcClock(x=>error=x);var other=new LibcClock(x=>otherError=x);
  LibcClock.Timespec value=new(){Seconds=123,Nanoseconds=456};
  Check(clock.ClockGettime(77,&value)==-1 && error==22 && value.Seconds==123 && value.Nanoseconds==456,"invalid id leaves destination");
  Check(clock.ClockGettime(0,null)==-1 && error==14,"null error");
  error=71;Check(clock.TimespecGet(&value,77)==0 && error==71 && value.Seconds==123,"invalid base preserves errno/destination");
  Check(other.ClockGettime(77,&value)==-1 && otherError==22 && error==71,"owner error isolation");
  long before=DateTimeOffset.UtcNow.ToUnixTimeSeconds();Check(clock.ClockGettime(0,&value)==0 && error==71,"realtime");long after=DateTimeOffset.UtcNow.ToUnixTimeSeconds();Check(value.Seconds>=before && value.Seconds<=after && value.Nanoseconds>=0 && value.Nanoseconds<1000000000,"realtime range");
  LibcClock.Timespec prior=default;Check(clock.ClockGettime(1,&prior)==0,"monotonic available");
  for(int i=0;i<10000;++i){Check(clock.ClockGettime(1,&value)==0,"monotonic result");Check(value.Seconds>prior.Seconds || value.Seconds==prior.Seconds && value.Nanoseconds>=prior.Nanoseconds,"monotonic order");prior=value;}
  Check(LibcClock.Difftime(long.MaxValue,long.MaxValue-1)==1 && LibcClock.Difftime(long.MinValue+1,long.MinValue)==1,"adjacent large time_t");
  Check(LibcClock.Difftime(long.MaxValue,long.MinValue)==18446744073709551616d && LibcClock.Difftime(long.MinValue,long.MaxValue)==-18446744073709551616d,"wide time_t difference");
  long stamp=0;Check(clock.Time(&stamp)==stamp && stamp>=before,"time output");Check(clock.TimespecGet(&value,1)==1,"TIME_UTC");
  for(int i=0;i<10000;++i)clock.ClockGettime(0,&value);
  long allocated=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100000;++i){clock.ClockGettime(0,&value);clock.ClockGettime(1,&value);}
  Check(GC.GetAllocatedBytesForCurrentThread()==allocated && error==71,"steady-state allocation/errno");
  Console.WriteLine($"{count} exact counter oracle vectors; realtime/monotonic/time, errors/owner isolation, extreme difftime and zero allocations passed");
 }
}
