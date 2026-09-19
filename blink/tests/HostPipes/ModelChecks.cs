using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation.Host;
public static class PipeModelChecks
{
    private static void Check(bool value,string message){if(!value)throw new Exception("pipe: "+message);}
    public static async Task Run()
    {
        var empty=new Dictionary<string,ReadOnlyMemory<byte>>();
        await using(var io=new InstanceIo(empty,pipeCapacity:4096,pipeByteLimit:8192))
        {
            var pair=io.Pipe(2048|524288);Check(pair.Succeeded,"create");var p=pair.Value;
            Check(io.GetDescriptorFlags(p.Read).Value==1 && io.GetDescriptorFlags(p.Write).Value==1,"cloexec");
            Check(io.GetStatusFlags(p.Read).Value==2048 && io.GetStatusFlags(p.Write).Value==2049,"access flags");
            byte[] large=Enumerable.Range(0,5000).Select(i=>(byte)i).ToArray();
            Check((await io.ReadAsync(p.Read,new byte[1])).Error==GuestError.Again,"empty nonblock");
            Check((await io.WriteAsync(p.Write,large)).Value==4096,"real short nonblock write");
            Check((await io.WriteAsync(p.Write,new byte[1])).Error==GuestError.Again,"full nonblock");
            byte[] first=new byte[1];Check((await io.ReadAsync(p.Read,first)).Value==1 && first[0]==large[0],"read prefix");
            Check((await io.WriteAsync(p.Write,new byte[2])).Error==GuestError.Again,"atomic small write cannot partially fit");
            Check((await io.WriteAsync(p.Write,large)).Value==1,"large short write with one byte free");
            byte[] remaining=new byte[4096];Check((await io.ReadAsync(p.Read,remaining)).Value==4096,"drain");
            Check(remaining.AsSpan(0,4095).SequenceEqual(large.AsSpan(1,4095)) && remaining[4095]==large[0],"ring order");
            Check(io.SetStatusFlags(p.Write,0).Succeeded,"clear nonblock");
            int duplicate=io.Duplicate(p.Write).Value;
            Check(io.SetStatusFlags(duplicate,2048).Succeeded && (io.GetStatusFlags(p.Write).Value&2048)!=0,"shared flags");
            Check(io.SetStatusFlags(p.Read,0).Succeeded,"blocking read");
            var waiting=io.ReadAsync(p.Read,new byte[1]);Check(!waiting.IsCompleted,"wait empty");
            Check(io.Close(p.Write).Succeeded && !waiting.IsCompleted,"dup retains writer");
            Check(io.Close(duplicate).Succeeded && (await waiting.WaitAsync(TimeSpan.FromSeconds(3))).Value==0,"last writer EOF");
            var poll=await io.PollAsync([new(p.Read,1)],0);Check(poll.Value.Events[0]==16,"EOF HUP");
            Check(io.Close(p.Read).Succeeded && io.PipeBytes==0,"release buffer");
        }
        await using(var io=new InstanceIo(empty,pipeCapacity:4096,pipeByteLimit:8192))
        {
            var p=io.Pipe().Value;
            var oldRead=io.ReadAsync(p.Read,new byte[1]);Check(!oldRead.IsCompleted,"pending old read");
            Check(io.Close(p.Read).Succeeded,"close leased read fd");
            var newer=io.Pipe(2048).Value;Check(newer.Read==p.Read,"fd reuse");
            Check((await io.WriteAsync(p.Write,"O"u8.ToArray())).Value==1 && (await oldRead).Value==1,"leased end completes");
            Check((await io.ReadAsync(newer.Read,new byte[1])).Error==GuestError.Again,"new fd did not capture old operation");
            Check((await io.WriteAsync(p.Write,new byte[1])).Error==GuestError.BrokenPipe,"last reader EPIPE");
            Check((await io.WriteAsync(p.Write,ReadOnlyMemory<byte>.Empty)).Value==0,"zero after no readers");
            Check((await io.WriteAsync(newer.Read,ReadOnlyMemory<byte>.Empty)).Error==GuestError.BadDescriptor,"wrong end zero");
            var poll=await io.PollAsync([new(p.Write,4)],0);Check(poll.Value.Events[0]==12,"no readers OUT ERR");
            Check(io.Close(p.Write).Succeeded && io.Close(newer.Read).Succeeded && io.Close(newer.Write).Succeeded && io.PipeBytes==0,"all pipes released");
        }
        await using(var io=new InstanceIo(empty,pipeCapacity:4096))
        {
            var p=io.Pipe().Value;byte[] fill=new byte[4095];Array.Fill(fill,(byte)'x');
            Check((await io.WriteAsync(p.Write,fill)).Value==4095,"prefill");
            var small=io.WriteAsync(p.Write,"ab"u8.ToArray());Check(!small.IsCompleted,"small blocks atomically");
            Check((await io.ReadAsync(p.Read,new byte[1])).Value==1 && (await small.WaitAsync(TimeSpan.FromSeconds(3))).Value==2,"small resumes");
            byte[] all=new byte[4096];Check((await io.ReadAsync(p.Read,all)).Value==4096 && all[4094]=='a' && all[4095]=='b',"atomic write data");
            using var canceled=new CancellationTokenSource();
            var big=io.WriteAsync(p.Write,new byte[8192],canceled.Token);Check(!big.IsCompleted,"large blocks after partial");
            canceled.Cancel();Check((await big.WaitAsync(TimeSpan.FromSeconds(3))).Value==4096,"cancel returns transferred prefix");
            Check((await io.ReadAsync(p.Read,new byte[4096])).Value==4096,"prefix retained");
            using var readCancel=new CancellationTokenSource();var read=io.ReadAsync(p.Read,new byte[1],readCancel.Token);readCancel.Cancel();
            Check((await read.WaitAsync(TimeSpan.FromSeconds(3))).Error==GuestError.Canceled,"cancel empty read");
            Check(io.PendingPipeOperations==0,"operation drain");
        }
        await using(var io=new InstanceIo(empty,pipeCapacity:4096,pipeByteLimit:4096,pipeOperationLimit:1))
        {
            var p=io.Pipe().Value;int count=io.OpenDescriptors;
            Check(io.Pipe().Error==GuestError.NoMemory && io.OpenDescriptors==count,"byte budget atomic failure");
            var read=io.ReadAsync(p.Read,new byte[1]);
            Check((await io.ReadAsync(p.Read,new byte[1])).Error==GuestError.Again,"bounded waits");
            var poll=io.PollAsync([new(p.Read,1)],-1);Check(!poll.IsCompleted,"poll waits");
            var readable=io.WaitReadableAsync(p.Read,TimeSpan.MaxValue);Check(!readable.IsCompleted,"long readiness wait");
            await io.DisposeAsync();
            Check((await read).Error==GuestError.Canceled && (await poll).Error==GuestError.Canceled,"dispose cancels waiters");
            Check(io.PipeBytes==0 && io.PendingPipeOperations==0,"dispose drains memory");
            Check((await readable).Error==GuestError.Canceled,"dispose long readiness wait");
        }
        await using(var io=new InstanceIo(empty,descriptorLimit:4))
            Check(io.Pipe().Error==GuestError.TooManyFiles && io.OpenDescriptors==3 && io.PipeBytes==0,"two fd allocation atomic");
        await using(var io=new InstanceIo(empty,pipeCapacity:4096))
        {
            var p=io.Pipe().Value;var data=new byte[4096];await io.WriteAsync(p.Write,data);
            var write=io.WriteAsync(p.Write,new byte[1]);Check(!write.IsCompleted,"blocked writer");
            Check(io.Close(p.Read).Succeeded && (await write.WaitAsync(TimeSpan.FromSeconds(3))).Error==GuestError.BrokenPipe,"reader closure wakes blocked writer");
        }
        // Independent writers share a stream, but each <=PIPE_BUF packet stays contiguous.
        await using(var io=new InstanceIo(empty,pipeCapacity:4096))
        {
            var p=io.Pipe().Value;
            async Task Produce(byte marker){byte[] packet=Enumerable.Repeat(marker,100).ToArray();for(int i=0;i<50;++i)Check((await io.WriteAsync(p.Write,packet)).Value==100,"atomic producer");}
            var a=Produce(1);var b=Produce(2);byte[] collected=new byte[10000];int received=0;
            while(received<collected.Length){var read=await io.ReadAsync(p.Read,collected.AsMemory(received,Math.Min(137,collected.Length-received)));Check(read.Value>0,"consumer");received+=read.Value;}
            await Task.WhenAll(a,b).WaitAsync(TimeSpan.FromSeconds(3));
            for(int i=0;i<100;++i)Check(collected.AsSpan(i*100,100).ToArray().All(value=>value==collected[i*100]),"packet atomicity");
            Check(collected.Count(value=>value==1)==5000 && collected.Count(value=>value==2)==5000,"producer counts");
        }
    }
}
