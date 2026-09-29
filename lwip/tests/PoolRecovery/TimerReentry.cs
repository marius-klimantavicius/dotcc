using System.Diagnostics;
using System.Runtime.InteropServices;
using Managed.Networking;
unsafe partial class Program
{
    sealed class TimerState(LwipStack stack)
    {
        internal readonly LwipStack Stack=stack;
        internal readonly ManualResetEventSlim Complete=new();
        internal Exception? Error;
    }
    static void TimerReentry(Lwip owner,void*argument)
    {
        var state=(TimerState)GCHandle.FromIntPtr((nint)argument).Target!;
        try
        {
            R(ReferenceEquals(owner,state.Stack.Core),"timer callback owner");
            Reject(()=>state.Stack.Invoke(_=>{}),"independent timer callback cannot synchronously redispatch");
            Reject(state.Stack.Dispose,"independent timer callback cannot dispose own worker");
            Reject(state.Stack.Poll,"independent timer callback cannot recursively poll");
        }
        catch(Exception error){state.Error=error;}finally{state.Complete.Set();}
    }
    static void TimerCallbackReentry()
    {
        using var stack=new LwipStack();stack.Start();var state=new TimerState(stack);var root=GCHandle.Alloc(state);
        try
        {
            stack.Invoke(owner=>owner.sys_timeout(1,&TimerReentry,(void*)GCHandle.ToIntPtr(root)));
            var clock=Stopwatch.StartNew();while(!state.Complete.IsSet&&clock.Elapsed<TimeSpan.FromSeconds(5)){stack.Poll();Thread.Sleep(1);}
            R(state.Complete.IsSet,"original timeout callback completes without reentry deadlock");if(state.Error!=null)throw state.Error;
            stack.Invoke(_=>{});Console.WriteLine("PASS original sys_timeout callback rejects facade reentry outside Invoke and leaves core usable");
        }
        finally
        {
            // Barrier removes an outstanding timeout or waits for its already-running callback.
            stack.Invoke(owner=>owner.sys_untimeout(&TimerReentry,(void*)GCHandle.ToIntPtr(root)));
            root.Free();state.Complete.Dispose();
        }
    }
}
