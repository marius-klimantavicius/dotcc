using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.CPython;
using VM = Managed.Interpreters.CPython;

// Real owning facade, real translated CPython, real pthread workers. No backend
// injection, stack recursion diagnostic, upstream algorithm or case changes.
internal static unsafe class Program
{
    private const ulong Minimum = 64UL * 1024 * 1024;
    private static int checks;
    private static readonly AsyncLocal<int> Ambient = new();
    private static readonly ConcurrentQueue<Observation> observations = new();
    [DllImport("libc", EntryPoint="pthread_self")] private static extern nuint HostSelf();
    [DllImport("libc", EntryPoint="pthread_getattr_np")] private static extern int HostAttributes(nuint thread, void* attributes);
    [DllImport("libc", EntryPoint="pthread_attr_getstack")] private static extern int HostStack(void* attributes, void** address, nuint* size);
    [DllImport("libc", EntryPoint="pthread_attr_destroy")] private static extern int HostDestroy(void* attributes);
    private static void Check(bool value, string label)
    { if (!value) throw new InvalidOperationException(label); Interlocked.Increment(ref checks); }
    private static T Throws<T>(Action action, string label) where T:Exception
    {
        try { action(); } catch (T error) { Interlocked.Increment(ref checks); return error; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + ": " + label);
    }
    private static ulong StackBytes()
    {
        byte* attributes = stackalloc byte[128];
        Check(HostAttributes(HostSelf(), attributes) == 0,"Linux pthread attributes");
        try { void* address; nuint size; Check(HostStack(attributes,&address,&size) == 0 && address != null,"Linux stack bounds"); return size; }
        finally { Check(HostDestroy(attributes) == 0,"host attribute cleanup"); }
    }
    private static void CheckReusable(PythonRuntime runtime)
    { using var value = runtime.Evaluate("6*7"); Check(value.AsInt64() == 42,"post-error interpreter reuse"); }
    private static void Foreign(Action action)
    {
        Exception? failure = null;
        var foreign = new Thread(() => { try { action(); } catch(Exception error) { failure=error; } });
        foreign.Start(); Check(foreign.Join(TimeSpan.FromSeconds(5)),"foreign rejection completed");
        Check(failure is InvalidOperationException,"foreign call rejected before dispatch");
    }

    private static void Cycle(string stdlib, string work, int cycle, VM.Libc.RuntimeContext owner)
    {
        int external = Environment.CurrentManagedThreadId;
        Ambient.Value = 93;
        var synchronizationContext = new RejectingContext();
        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        Check(ReferenceEquals(VM.Libc.RuntimeContext.Current,owner),"same program context spans generations");
        VM.Libc.errno = 79;
        byte[] path = System.Text.Encoding.UTF8.GetBytes(work + "\0");
        fixed(byte* text=path) Check(VM.Libc.chdir(text) == 0,"existing owner cwd configured");
        Console.WriteLine($"phase explicit cycle={cycle} initialize");
        PythonRuntime runtime = PythonRuntime.Initialize(new PythonOptions(stdlib, work));
        Console.WriteLine($"phase explicit cycle={cycle} initialized");
        Check(ReferenceEquals(VM.Libc.CaptureRuntimeContext(), owner),"external original context retained");
        Foreign(() => PythonRuntime.Initialize(new PythonOptions(stdlib,work)));
        long externalPthread = VM.Libc.pthread_self();
        long executionPthread;
        using(var value=runtime.Evaluate("__import__('_thread').get_ident()")) executionPthread=value.AsInt64();
        Check(executionPthread != externalPthread,"Python executes on private pthread identity");
        using(var value=runtime.Evaluate("__import__('_thread').get_ident()")) Check(value.AsInt64()==executionPthread,"stable execution pthread");
        using(var cwd=runtime.Evaluate("__import__('os').getcwd()")) Check(cwd.AsString()==work,"executor uses original Libc path owner");
        Check(VM.Libc.errno==79,"executor errno does not overwrite external TLS");
        PythonValue? live = runtime.FromInt64(42);
        runtime.RegisterInt64Callback("host","observe",tag =>
        {
            bool ownerCallback=tag==0 || tag==50;
            int current=Environment.CurrentManagedThreadId;
            Check((current==external)==ownerCallback,"callback origin affinity");
            Check(ReferenceEquals(VM.Libc.CaptureRuntimeContext(),owner),"callback exact existing Libc context");
            Check(Ambient.Value==(ownerCallback?93:0),"explicit execution-context isolation");
            Check(ReferenceEquals(SynchronizationContext.Current,synchronizationContext)==ownerCallback,"no arbitrary synchronization-context flow");
            Throws<InvalidOperationException>(()=>runtime.Evaluate("1"),"callback owning reentry");
            Throws<InvalidOperationException>(()=>runtime.Dispose(),"callback finalization reentry");
            if(live != null) Throws<InvalidOperationException>(()=>live.Clone(),"callback value reentry");
            ulong physical=StackBytes();
            if(!ownerCallback) Check(physical>=Minimum,"genuine Python worker physical minimum");
            (long virtualBytes,long residentBytes)=ProcessMemory();
            observations.Enqueue(new Observation(cycle,(int)tag,current,physical,virtualBytes,residentBytes));
            return tag+42;
        });
        runtime.RegisterInt64Callback("host","fail",_=>throw new ApplicationException("callback marker"));
        using(var value=runtime.Evaluate("__import__('host').observe(0)")) Check(value.AsInt64()==42,"synchronous owner callback");
        Foreign(()=>runtime.Execute("pass")); Foreign(()=>live.AsInt64()); Foreign(()=>live.Dispose()); Foreign(()=>runtime.Dispose());
        Check(live.AsInt64()==42,"foreign disposal retained live handle");
        using(var clone=live.Clone()) Check(clone.AsInt64()==42,"INCREF clone");
        using(var text=runtime.FromString("λ🐍\0tail")) Check(text.AsString()=="λ🐍\0tail","length-aware Unicode");
        using(var module=runtime.Import("math"))
        using(var sqrt=module.GetAttribute("isqrt"))
        using(var input=runtime.FromInt64(1764))
        using(var result=sqrt.Call(input)) Check(result.AsInt64()==42,"owning import/attribute/span-call/conversion");
        PythonExecutionResult output=runtime.Execute("print('stdout-marker')\nimport sys\nprint('stderr-marker',file=sys.stderr)","streams_r34.py");
        Check(output.StandardOutput.Contains("stdout-marker") && output.StandardError.Contains("stderr-marker"),"output retained");
        PythonException failure=Throws<PythonException>(()=>runtime.Execute("print('before-error')\nraise ValueError('error marker')","error_r34.py"),"Python error propagation");
        Check(failure.PythonType=="ValueError" && failure.Message.Contains("error marker") && failure.PythonTraceback.Contains("error_r34.py") && failure.StandardOutput.Contains("before-error"),"original type/frames/output");
        Check(failure.StackTrace!.Contains("PythonRuntime") && failure.StackTrace.Contains("PythonExecutionThread"),"original managed exception dispatch stack");
        CheckReusable(runtime);
        failure=Throws<PythonException>(()=>runtime.Execute("import host\nhost.fail(1)","callback_failure_r34.py"),"callback exception containment");
        Check(failure.PythonType=="RuntimeError" && failure.Message.Contains("ApplicationException: callback marker"),"callback RuntimeError diagnostic");
        CheckReusable(runtime);
        runtime.Execute("""
            import threading, host
            results=[]
            def child(): results.append(host.observe(30))
            def parent():
                results.append(host.observe(20))
                nested=threading.Thread(target=child)
                nested.start(); nested.join()
            default_worker=threading.Thread(target=lambda: results.append(host.observe(10)))
            default_worker.start(); default_worker.join()
            threading.stack_size(32768)
            assert threading.stack_size()==32768
            explicit_worker=threading.Thread(target=parent)
            explicit_worker.start(); explicit_worker.join()
            threading.stack_size(0)
            assert sorted(results)==[52,62,72],results
            ""","ordinary_workers_r34.py");
        using(var before=runtime.Evaluate("__import__('sys').gettrace() is None")) Check(before.AsInt64()==1,"initial trace hook");
        foreach(string source in new[]{"while True: pass","import host; host.wait_ms(10000)"})
        {
            using var cancellation=new CancellationTokenSource();
            cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));
            failure=Throws<PythonException>(()=>runtime.Execute(source,"cancel_r34.py",cancellation.Token),"cooperative cancellation");
            Check(failure.PythonType=="KeyboardInterrupt","real cancellation Python exception");
            CheckReusable(runtime);
            using var trace=runtime.Evaluate("__import__('sys').gettrace() is None");
            Check(trace.AsInt64()==1,"trace hook restored");
        }
        using(var cancelled=new CancellationTokenSource())
        {
            cancelled.Cancel();
            failure=Throws<PythonException>(()=>runtime.Execute("raise AssertionError('cancelled source ran')",cancellationToken:cancelled.Token),"pre-cancelled operation");
            Check(failure.PythonType=="KeyboardInterrupt","pre-cancelled operation rejected");
        }
        Throws<InvalidOperationException>(()=>runtime.Dispose(),"live values block disposal without stopping runtime");
        CheckReusable(runtime);
        live.Dispose(); live.Dispose(); live=null;
        runtime.Execute("""
            import atexit,threading,host
            atexit.register(lambda:host.observe(50))
            def final_worker():
                host.wait_ms(80)
                host.observe(40)
            pending=threading.Thread(target=final_worker)
            pending.start()
            ""","shutdown_r34.py");
        Console.WriteLine($"phase explicit cycle={cycle} finalize checks={checks}");
        runtime.Dispose(); runtime.Dispose();
        Console.WriteLine($"phase explicit cycle={cycle} finalized checks={checks}");
        Check(observations.Any(x=>x.Cycle==cycle && x.Tag==40),"worker callback remains rooted through shutdown join");
        Check(observations.Any(x=>x.Cycle==cycle && x.Tag==50),"owner callback pump remains active through finalization");
        Throws<ObjectDisposedException>(()=>runtime.Evaluate("1"),"dead runtime rejects owning entry");
        Check(ReferenceEquals(VM.Libc.RuntimeContext.Current,owner),"caller binding restored after executor teardown");
        // Acquiring a fresh scope proves old executor policy and workers drained.
        using(var fresh=owner.UseMinimumPthreadStackSize((int)Minimum)) { }
        using(var unrelated=new VM.Libc.RuntimeContext())
        using(var otherBinding=unrelated.Enter())
        {
            var rejected=Throws<InvalidOperationException>(()=>PythonRuntime.Initialize(new PythonOptions(stdlib,work)),"static program owner switch rejects before translated entry");
            Check(rejected.Message.Contains("static translated CPython program"),"context switch diagnostic");
        }
        SynchronizationContext.SetSynchronizationContext(null);
        Ambient.Value=0;
    }

    private static (long VirtualBytes,long ResidentBytes) ProcessMemory()
    {
        long virtualBytes=0,residentBytes=0;
        foreach(string line in File.ReadLines("/proc/self/status"))
        {
            if(line.StartsWith("VmSize:",StringComparison.Ordinal)) virtualBytes=long.Parse(line.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries)[1])*1024;
            if(line.StartsWith("VmRSS:",StringComparison.Ordinal)) residentBytes=long.Parse(line.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries)[1])*1024;
        }
        return (virtualBytes,residentBytes);
    }

    private static void LegacyCycle(string stdlib,string work)
    {
        Check(VM.Libc.RuntimeContext.Current==null,"legacy external binding starts null");
        int caller=Environment.CurrentManagedThreadId;
        Console.WriteLine("phase legacy initialize");
        using(var runtime=PythonRuntime.Initialize(new PythonOptions(stdlib,work)))
        {
            Console.WriteLine("phase legacy initialized");
            runtime.RegisterInt64Callback("host","legacy",tag=>
            {
                Check(VM.Libc.RuntimeContext.Current==null,"legacy callback/worker binding stays null");
                Check((Environment.CurrentManagedThreadId==caller)==(tag==90),"legacy callback affinity");
                ulong physical=StackBytes();
                if(tag==91) Check(physical>=Minimum,"legacy Python worker physical stack");
                var memory=ProcessMemory();
                observations.Enqueue(new Observation(2,(int)tag,Environment.CurrentManagedThreadId,physical,memory.VirtualBytes,memory.ResidentBytes));
                return 42;
            });
            using(var cwd=runtime.Evaluate("__import__('os').getcwd()")) Check(cwd.AsString()==Directory.GetCurrentDirectory(),"legacy process cwd preserved");
            runtime.Execute("import host,threading\nassert host.legacy(90)==42\nt=threading.Thread(target=lambda:host.legacy(91))\nt.start();t.join()","legacy_r34.py");
            CheckReusable(runtime);
        }
        Check(VM.Libc.RuntimeContext.Current==null,"legacy binding restored after teardown");
        Console.WriteLine("phase legacy finalized");
    }

    private static void ExplicitProgram(string stdlib,string work,int cycles)
    {
        using(var owner=new VM.Libc.RuntimeContext())
        using(var binding=owner.Enter())
            for(int i=0;i<cycles;++i) Cycle(stdlib,work,i,owner);
        // The explicit program lifetime has ended. A different/default context
        // must reject before dereferencing any surviving static VM pointers.
        var rejected=Throws<InvalidOperationException>(()=>PythonRuntime.Initialize(new PythonOptions(stdlib,work)),"entry after original program owner disposal");
        Check(rejected.Message.Contains("static translated CPython program"),"disposed program owner diagnostic");
    }

    private static void BadInitialization(string stdlib,string work)
    {
        string empty=Path.Combine(work,"empty-stdlib"); Directory.CreateDirectory(empty);
        Throws<PythonInitializationException>(()=>PythonRuntime.Initialize(new PythonOptions(empty,work)),"bad isolated stdlib");
        try
        {
            using var next=PythonRuntime.Initialize(new PythonOptions(stdlib,work));
            CheckReusable(next);
            Console.WriteLine("Actual initialization failure was clean; process reservation reusable.");
        }
        catch(InvalidOperationException error)
        {
            Check(error.Message.Contains("Only one CPython runtime"),"uncertain initialization retains process reservation");
            Console.WriteLine("Actual initialization failure was uncertain; process reservation correctly retained.");
        }
    }
    private static int Main(string[] args)
    {
        if(args.Length!=4) throw new ArgumentException("Expected owner|pool|bad-init, stdlib, work, report");
        string stdlib=Path.GetFullPath(args[1]),work=Path.GetFullPath(args[2]);Directory.CreateDirectory(work);
        try
        {
            if(args[0]=="json-case")
            {
                // Invoke only in a separately allocated supervisor batch after
                // ordinary actual-product contracts pass. This loads the exact
                // unchanged upstream method; no guard/limit/test adjustment.
                using var runtime=PythonRuntime.Initialize(new PythonOptions(stdlib,work));
                var result=runtime.Execute("""
                    import unittest, io
                    required_id='test.test_json.test_recursion.TestCRecursion.test_highly_nested_objects_decoding'
                    suite=unittest.defaultTestLoader.loadTestsFromName(required_id)
                    assert suite.countTestCases()==1
                    stream=io.StringIO()
                    result=unittest.TextTestRunner(stream=stream,verbosity=2).run(suite)
                    print(stream.getvalue())
                    assert result.testsRun==1 and result.wasSuccessful()
                    assert not result.skipped and not result.expectedFailures and not result.unexpectedSuccesses
                    ""","unchanged_required_json_r34.py");
                Console.Write(result.StandardOutput);Console.Error.Write(result.StandardError);
                CheckReusable(runtime);
                Check(true,"unchanged required JSON case completed and interpreter reused");
            }
            else if(args[0]=="bad-init") BadInitialization(stdlib,work);
            else if(args[0]=="pool") Task.Run(()=>{Check(Thread.CurrentThread.IsThreadPoolThread,"ThreadPool-origin caller");ExplicitProgram(stdlib,work,1);}).GetAwaiter().GetResult();
            else if(args[0]=="legacy") { LegacyCycle(stdlib,work); LegacyCycle(stdlib,work); }
            else if(args[0]=="owner")
            {
                ExceptionDispatchInfo? failure=null;
                var caller=new Thread(()=>{try{ExplicitProgram(stdlib,work,2);}catch(Exception error){failure=ExceptionDispatchInfo.Capture(error);}},256*1024){IsBackground=true};
                caller.Start();Check(caller.Join(TimeSpan.FromSeconds(60)),"small caller completed ordinary contracts");failure?.Throw();
            }
            else throw new ArgumentException("Unknown contract mode");
            var report=new ContractReport("passed",args[0],checks,typeof(PythonRuntime).Module.ModuleVersionId,typeof(VM).Module.ModuleVersionId,observations.ToList());
            File.WriteAllText(args[3],JsonSerializer.Serialize(report,ReportContext.Default.ContractReport));
            Console.WriteLine($"PASS actual {args[0]} checks={checks} observations={observations.Count}");return 0;
        }
        catch(PythonException error){Console.Error.WriteLine(error.PythonType+": "+error.Message+"\n"+error.PythonTraceback+"\n"+error.StandardError);throw;}
    }
    private sealed class RejectingContext:SynchronizationContext
    {
        public override void Post(SendOrPostCallback callback,object? state)=>throw new InvalidOperationException("Unexpected context Post");
        public override void Send(SendOrPostCallback callback,object? state)=>throw new InvalidOperationException("Unexpected context Send");
    }
}
internal sealed record Observation(int Cycle,int Tag,int ManagedThread,ulong PhysicalStackBytes,long ProcessVirtualBytes,long ProcessResidentBytes);
internal sealed record ContractReport(string Status,string Mode,int Checks,Guid FacadeMvid,Guid LibraryMvid,List<Observation> Observations);
[JsonSerializable(typeof(ContractReport))]
internal partial class ReportContext:JsonSerializerContext { }
