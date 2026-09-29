#if LWIP_HAS_SOCKETS
using Managed.Networking;
using System.Collections.Concurrent;
using System.Net;
unsafe partial class Program
{
    sealed class PausedScheduler:TaskScheduler
    {
        readonly ConcurrentQueue<Task> queued=new();
        protected override void QueueTask(Task task)=>queued.Enqueue(task);
        protected override bool TryExecuteTaskInline(Task task,bool previouslyQueued)=>false;
        protected override IEnumerable<Task>? GetScheduledTasks()=>queued.ToArray();
        internal void RunOne(){R(queued.TryDequeue(out var task),"one accepted scheduled task");R(TryExecuteTask(task!),"execute queued original-call worker");}
    }
    static void ScheduledAdmissionCases()
    {
        foreach(bool netconn in new[]{false,true})
        {
            using var a=new LwipStack();using var b=new LwipStack();a.Start();b.Start();var scheduler=new PausedScheduler();
            var survivor=LwipSockets.Start(b).CreateUdp();survivor.Bind(new(IPAddress.Parse("127.0.0.1"),29980));
            Task pending;Func<bool> closing;
            if(netconn)
            {
                var service=LwipNetconns.Start(a);service.Scheduler=scheduler;var connection=service.CreateUdp();connection.Bind(new(IPAddress.Parse("127.0.0.1"),29981));
                pending=connection.ReceiveAsync();closing=()=>Volatile.Read(ref connection.Closing);
                R(connection.Scheduled==1&&connection.Active==0,"netconn accounted before original Call starts");
            }
            else
            {
                var service=LwipSockets.Start(a);service.Scheduler=scheduler;var socket=service.CreateUdp();socket.Bind(new(IPAddress.Parse("127.0.0.1"),29981));
                pending=socket.ReceiveAsync();closing=()=>Volatile.Read(ref socket.Closing);
                R(socket.Scheduled==1&&socket.Active==0,"socket accounted before original Call starts");
            }
            R(!pending.IsCompleted,"returned caller task queued before worker admission");
            var dispose=Task.Run(a.Dispose);
            R(SpinWait.SpinUntil(closing,TimeSpan.FromSeconds(5)),"owner disposal reaches actual handle close");
            R(!dispose.IsCompleted&&!pending.IsCompleted,"owner disposal waits for accepted unscheduled caller");
            scheduler.RunOne();bool rejected=false;try{Await(pending);}catch(ObjectDisposedException){rejected=true;}
            R(rejected&&pending.IsCompleted,"queued task settles through normal closed-handle admission");Await(dispose);
            var read=survivor.ReceiveFromAsync();R(Await(survivor.SendToAsync("B scheduled survivor"u8.ToArray(),new(IPAddress.Parse("127.0.0.1"),29980)))==20,"owner B send after A scheduled shutdown");
            R(Await(read).Bytes.AsSpan().SequenceEqual("B scheduled survivor"u8),"owner B continues after A queued caller settles");survivor.Dispose();
        }
        Console.WriteLine("PASS owned-api: socket/netconn scheduling admission retains owner and joins returned Tasks before disposal");
    }
}
#endif
