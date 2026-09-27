using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Managed.Interpreters;
using VM = Managed.Interpreters.QuickJs;

internal static unsafe class Program
{
    private static int passed;
    private static int sharedFinalizers;
    private static readonly List<string> javascriptCases = [];
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    private static void Case(string id, Action test)
    {
        test();
        passed++;
        Console.WriteLine("PASS " + id);
    }
    private static void Throws<T>(Action action, string fragment = "") where T : Exception
    {
        try { action(); }
        catch (T exception)
        {
            Check(exception.Message.Contains(fragment, StringComparison.OrdinalIgnoreCase), $"Expected '{fragment}', got {exception.Message}");
            return;
        }
        throw new Exception($"Expected {typeof(T).Name}: {fragment}");
    }
    private static void Evaluate(QuickJsContext context, string source, string name = "<test>", bool module = false)
    {
        using var value = context.Evaluate(source, name, module);
    }
    private static string Text(QuickJsContext context, string source)
    {
        using var value = context.Evaluate(source);
        return value.ToString();
    }
    private static void Clean(QuickJsRuntime runtime)
    {
        runtime.Dispose();
        Check(runtime.AllocatedBlocks == 0 && runtime.AllocatedBytes == 0, "Owned allocations survived runtime disposal.");
    }
    private static VM.JSValue Print(VM.JSContext* context, VM.JSValue self, int count, VM.JSValue* args)
    {
        try
        {
            var parts = new string[count];
            for (int i = 0; i < count; i++) parts[i] = QuickJsContext.String(context, args[i]);
            string line = string.Join(' ', parts);
            if (line.StartsWith("CASE PASS ", StringComparison.Ordinal)) javascriptCases.Add(line[10..]);
            Console.WriteLine(line);
            return new() { tag = 3 };
        }
        catch (Exception error) { return QuickJsContext.ThrowManaged(context, error); }
    }
    private static VM.JSValue Collect(VM.JSContext* context, VM.JSValue self, int count, VM.JSValue* args)
    {
        VM.JS_RunGC(VM.JS_GetRuntime(context));
        return new() { tag = 3 };
    }
    private static VM.JSValue Identity(VM.JSContext* context, VM.JSValue self, int count, VM.JSValue* args)
        => count == 1 ? VM.JS_DupValue(context, args[0]) : new() { tag = 3 };
    private static void Register(QuickJsContext context, string name,
        delegate*<VM.JSContext*, VM.JSValue, int, VM.JSValue*, VM.JSValue> function)
    {
        QuickJsText.CheckName(name);
        context.Runtime.Enter();
        scoped var nameMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            nameMarshaller.FromManaged(name, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);
            var key = nameMarshaller.ToUnmanaged();
            var value = VM.JS_NewCFunction2(context.NativeContext, function, key, 1, VM.JSCFunctionEnum.JS_CFUNC_generic, 0);
            if (value.tag == VM.JS_TAG_EXCEPTION) throw QuickJsContext.TakeException(context.NativeContext);
            var global = VM.JS_GetGlobalObject(context.NativeContext);
            try
            {
                if (VM.JS_SetPropertyStr(context.NativeContext, global, key, value) < 0)
                    throw QuickJsContext.TakeException(context.NativeContext);
            }
            finally { VM.JS_FreeValue(context.NativeContext, global); }
        }
        finally
        {
            nameMarshaller.Free();
            context.Runtime.Leave();
        }
    }
    private static void Setup(QuickJsContext context)
    {
        Register(context, "print", &Print);
        Register(context, "__collect", &Collect);
        Evaluate(context, "globalThis.console={log:print}; globalThis.std={gc:__collect}; function __case(id,fn){fn();print('CASE PASS '+id);}");
    }
    private static void Behavior(string root)
    {
        Case("atomics-engine-fixtures", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Setup(context);
            javascriptCases.Clear();
            Evaluate(context, File.ReadAllText(Path.Combine(root, "tests/fixtures/atomics.js")), "atomics.js");
            Check(javascriptCases.Count == 5 && javascriptCases.Distinct().Count() == 5, "Expected all five Atomics operation cases.");
            runtime.Enter();
            try { VM.JS_SetCanBlock(runtime.NativeRuntime, 1); }
            finally { runtime.Leave(); }
            Evaluate(context, File.ReadAllText(Path.Combine(root, "tests/fixtures/atomics-wait.js")), "atomics-wait.js");
            Check(javascriptCases.Count == 6 && javascriptCases[^1] == "atomics-bounded-wait", "Bounded wait fixture did not execute.");
            Clean(runtime);
        });
        Case("fixed-engine-fixtures", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Setup(context);
            javascriptCases.Clear();
            Evaluate(context, File.ReadAllText(Path.Combine(root, "tests/fixtures/core.js")), "core.js");
            runtime.DrainJobs();
            Check(javascriptCases.Count == 10 && javascriptCases.Distinct().Count() == 10, "Expected 10 distinct authored cases.");
            Clean(runtime);
        });
        Case("modules-and-job-order", () =>
        {
            using var runtime = new QuickJsRuntime(new Dictionary<string, string>
            {
                ["sum"] = "export const sum = xs => xs.reduce((a,b)=>a+b,0);",
                ["a"] = "import {g} from 'b'; export function f(){return 2}; export const value=g();",
                ["b"] = "import {f} from 'a'; export function g(){return f()+1};"
            });
            using var context = runtime.CreateContext();
            Evaluate(context, "import {value} from 'a'; import {sum} from 'sum'; globalThis.result=sum([value,4]);", "main.mjs", true);
            runtime.DrainJobs();
            Check(Text(context, "result") == "7", "Cyclic module or sum failed.");
            Check(runtime.ModuleLoads == 3, "Module loader count mismatch.");
            Evaluate(context, "globalThis.jobs=[];Promise.resolve().then(()=>jobs.push(1));Promise.resolve().then(()=>jobs.push(2));");
            Check(runtime.DrainJobs() == 2 && Text(context, "jobs.join()") == "1,2", "Promise job ordering mismatch.");
            Clean(runtime);
        });
        Case("owning-text-and-buffer-boundary", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            const string original = "A\0\ud83d\ude00\ud800";
            using var text = context.CreateString(original);
            Check(text.ToString() == original, "CreateString lost UTF-16 code units.");
            context.SetGlobal("text", text);
            Check(Text(context, "text.length===5 && text.charCodeAt(1)===0 && text.charCodeAt(4)===0xd800") == "true", "String boundary changed JS code units.");
            Throws<ArgumentException>(() => { using var invalid = context.GetGlobal("bad\0name"); });
            byte[] input = [1, 2, 3, 4];
            using var buffer = context.CreateArrayBuffer(input);
            input[0] = 99;
            context.SetGlobal("buffer", buffer);
            Check(Text(context, "new Uint8Array(buffer)[0]") == "1", "Input buffer was borrowed instead of copied.");
            byte[] output = buffer.ToArrayBuffer();
            Check(output.SequenceEqual(new byte[] { 1, 2, 3, 4 }), "Buffer round trip changed bytes.");
            output[1] = 77;
            Check(Text(context, "new Uint8Array(buffer)[1]") == "2", "Output copy mutation changed engine storage.");
            using var empty = context.CreateArrayBuffer(ReadOnlySpan<byte>.Empty);
            Check(empty.ToArrayBuffer().Length == 0, "Empty buffer contract failed.");
            runtime.Enter();
            try { VM.JS_DetachArrayBuffer(context.NativeContext, buffer.Raw); }
            finally { runtime.Leave(); }
            Throws<QuickJsException>(() => buffer.ToArrayBuffer(), "detach");
            Clean(runtime);
        });
        Case("string-boundary", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            using var value = context.Evaluate("'A\\0\\ud83d\\ude00\\ud800'");
            Check(value.ToString() == "A\0\ud83d\ude00\ud800", "UTF-16/NUL string return mismatch.");
            using var input = context.Evaluate("'A\ud83d\ude00\ud800'");
            Check(input.ToString() == "A\ud83d\ude00\ud800", "UTF-16 string input mismatch.");
            Check(Text(context, "let 𐐀=7; 𐐀") == "7", "Supplementary Unicode identifier mismatch.");
            Clean(runtime);
        });
    }
    private static void SharedReleased(VM.JSRuntime* runtime, void* opaque, void* backing)
        => Interlocked.Increment(ref sharedFinalizers);

    private static void InstallShared(QuickJsContext context, nint backing, bool big)
    {
        context.Runtime.Enter();
        try
        {
            VM.JSValue buffer = VM.JS_NewArrayBuffer(context.NativeContext, (byte*)backing, 16, &SharedReleased, null, 1);
            if (buffer.tag == 6) throw QuickJsContext.TakeException(context.NativeContext);
            VM.JSValue global = VM.JS_GetGlobalObject(context.NativeContext);
            try
            {
                fixed (byte* name = "shared\0"u8)
                    if (VM.JS_SetPropertyStr(context.NativeContext, global, name, buffer) < 0)
                        throw QuickJsContext.TakeException(context.NativeContext);
            }
            finally { VM.JS_FreeValue(context.NativeContext, global); }
            VM.JS_SetCanBlock(context.Runtime.NativeRuntime, 1);
        }
        finally { context.Runtime.Leave(); }
        Evaluate(context, "globalThis.waitBigInt=" + (big ? "true" : "false"));
    }

    private static void AtomicsThreads(string root, bool big)
    {
        string script = File.ReadAllText(Path.Combine(root, "tests/fixtures/atomics-worker.js"));
        nint backing = (nint)NativeMemory.AllocZeroed(16);
        if (backing == 0) throw new OutOfMemoryException();
        sharedFinalizers = 0;
        Task<string>? worker = null;
        try
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            InstallShared(context, backing, big);
            worker = Task.Run(() =>
            {
                using var owner = new QuickJsRuntime();
                using var guest = owner.CreateContext();
                InstallShared(guest, backing, big);
                string result = Text(guest, script);
                Clean(owner);
                return result;
            });
            Evaluate(context, "for(let i=0;i<2000;i++)Atomics.add(new Int32Array(shared),2,1);");
            var deadline = System.Diagnostics.Stopwatch.StartNew();
            string notified = "0";
            while (notified == "0" && deadline.Elapsed < TimeSpan.FromSeconds(10))
            {
                notified = Text(context, "Atomics.notify(waitBigInt?new BigInt64Array(shared):new Int32Array(shared),0,1)");
                if (notified == "0") Thread.Sleep(1);
            }
            Check(notified == "1", "Notify did not wake exactly one actual cross-runtime waiter.");
            Check(worker.GetAwaiter().GetResult() == "ok", "Actual Atomics.wait was not notified.");
            Check(Text(context, "Atomics.load(new Int32Array(shared),2)") == "4000", "Concurrent atomic increments were lost.");
            Check(Text(context, "Atomics.notify(new Int32Array(shared),0)") == "0", "Completed waiter remained linked.");
            Clean(runtime);
            Check(sharedFinalizers == 2, "Both runtime buffer views must release exactly once.");
        }
        finally
        {
            // The backing allocation outlives both independently owned runtimes,
            // including exception paths. The suite process has its own deadline.
            try { worker?.GetAwaiter().GetResult(); }
            finally { NativeMemory.Free((void*)backing); }
        }
    }

    private static void Lifecycle(string root)
    {
        Case("atomics-cross-runtime-int32", () => AtomicsThreads(root, false));
        Case("atomics-cross-runtime-bigint64", () => AtomicsThreads(root, true));
        Case("module-normalization-and-failures", () =>
        {
            using var runtime = new QuickJsRuntime(new Dictionary<string, string>
            {
                ["dir/shared.js"] = "globalThis.loads=(globalThis.loads||0)+1;export const item={};",
                ["broken"] = "export const answer = ;",
                ["throws"] = "throw Error('module evaluation failure');"
            });
            using var context = runtime.CreateContext();
            Evaluate(context, "import {item as a} from './dir/shared.js';import {item as b} from 'dir/shared.js';globalThis.same=a===b;", "root.mjs", true);
            runtime.DrainJobs();
            Check(Text(context, "same && loads===1") == "true" && runtime.ModuleLoads == 1, "Module normalization/cache identity mismatch.");
            Throws<QuickJsException>(() => Evaluate(context, "import 'broken';", "broken-main.mjs", true), "SyntaxError");
            Evaluate(context, "import 'throws';", "throwing-main.mjs", true);
            Throws<QuickJsException>(() => runtime.DrainJobs(), "module evaluation failure");
            Check(Text(context, "6*7") == "42", "Context did not recover from module failures.");
            Clean(runtime);
        });
        Case("module-recursion-limit", () =>
        {
            var modules = new Dictionary<string, string>();
            for (int i = 0; i < 512; i++) modules[$"m{i}.js"] = i == 511 ? "export const last=1;" : $"import './m{i+1}.js';";
            using var runtime = new QuickJsRuntime(modules);
            using var context = runtime.CreateContext();
            Throws<QuickJsException>(() => Evaluate(context, modules["m0.js"], "m0.js", true), "stack");
            Check(Text(context, "6*7") == "42", "Context did not recover from module depth rejection.");
            Clean(runtime);
        });
        Case("syntax-type-missing-module-errors", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Throws<QuickJsException>(() => Evaluate(context, "let = ;"), "SyntaxError");
            Throws<QuickJsException>(() => Evaluate(context, "null.foo()"), "TypeError");
            Throws<QuickJsException>(() => Evaluate(context, "import 'absent';", "missing.mjs", true), "Unknown in-memory module");
            Check(Text(context, "6*7") == "42", "Runtime did not recover after ordinary errors.");
            Clean(runtime);
        });
        Case("callback-errors-and-reentry", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            context.RegisterFunction("hostFail", _ => throw new InvalidOperationException("managed callback failure"));
            Check(Text(context, "try { hostFail(); false } catch(e) { e.message.includes('managed callback failure') }") == "true", "Managed callback exception was not catchable.");
            context.RegisterFunction("hostReenter", _ => { Evaluate(context, "1"); return 0; });
            Check(Text(context, "try { hostReenter(); false } catch(e) { e.message.includes('reentry') }") == "true", "Callback reentry was not rejected.");
            Clean(runtime);
        });
        Case("callbacks-outlive-source-context", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var a = runtime.CreateContext();
            using var b = runtime.CreateContext();
            a.RegisterFunction("plusOne", args => args[0] + 1);
            using (var function = a.GetGlobal("plusOne")) b.SetGlobal("retained", function);
            a.Dispose();
            Check(Text(b, "retained(41)") == "42", "Callback lost after source facade disposal.");
            Clean(runtime);
        });
        Case("pending-callback-outlives-context", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            int count = 0;
            context.RegisterFunction("notify", _ => ++count);
            Evaluate(context, "Promise.resolve().then(()=>notify())");
            context.Dispose();
            runtime.DrainJobs();
            Check(count == 1, "Pending callback lost after context disposal.");
            Clean(runtime);
        });
        Case("promise-rejections", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Evaluate(context, "Promise.reject(Error('handled')).catch(()=>{});");
            runtime.DrainJobs();
            Evaluate(context, "Promise.reject(Error('unhandled rejection control'));");
            Throws<QuickJsException>(() => runtime.DrainJobs(), "unhandled");
            Clean(runtime);
        });
        Case("rejection-roots-and-conversion-errors", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Setup(context);
            Evaluate(context, "Promise.reject('first unreachable'); Promise.reject('second unreachable'); std.gc(); Promise.reject('later handled').catch(()=>{});");
            try { runtime.DrainJobs(); throw new Exception("Unreachable rejections were lost."); }
            catch (QuickJsException error)
            {
                Check(error.Message.Contains("first unreachable") && error.Message.Contains("second unreachable") && !error.Message.Contains("later handled"), "Rejected promise identities were lost or aliased.");
            }
            Evaluate(context, "Promise.reject({toString(){throw Error('conversion failure')}})");
            Throws<QuickJsException>(() => runtime.DrainJobs(), "Unhandled");
            runtime.Enter();
            try { Check(VM.JS_HasException(context.NativeContext) == 0, "Rejection reason conversion left a pending exception."); }
            finally { runtime.Leave(); }
            using var value = context.Evaluate("({toString(){throw Error('conversion failure')}})");
            Throws<QuickJsException>(() => value.ToString());
            runtime.Enter();
            try { Check(VM.JS_HasException(context.NativeContext) == 0, "Value conversion left a pending exception."); }
            finally { runtime.Leave(); }
            Check(Text(context, "6*7") == "42", "Runtime did not recover after reason/value conversion errors.");
            Clean(runtime);
        });
        Case("values-and-cross-runtime", () =>
        {
            using var first = new QuickJsRuntime();
            using var second = new QuickJsRuntime();
            using var a = first.CreateContext();
            using var b = second.CreateContext();
            using var value = a.Evaluate("({answer:42})");
            using var copy = value.Duplicate();
            value.Dispose(); value.Dispose();
            using var property = copy.GetProperty("answer");
            Check(property.ToDouble() == 42, "Duplicate lost its reference.");
            Throws<ArgumentException>(() => b.SetGlobal("bad", copy), "runtime");
            a.SetGlobal("good", copy);
            Check(Text(a, "good.answer") == "42", "SetGlobal did not retain ownership.");
            Clean(first); Clean(second);
            Throws<ObjectDisposedException>(() => copy.ToString());
            first.Dispose(); second.Dispose();
        });
        Case("cycles-and-disposal", () =>
        {
            for (int i = 0; i < 3; i++)
            {
                using var runtime = new QuickJsRuntime();
                using var context = runtime.CreateContext();
                Evaluate(context, "globalThis.cycles=[];for(let i=0;i<1000;i++){let x={};x.self=x;cycles.push(x)}");
                Evaluate(context, "cycles=null");
                long before = runtime.AllocatedBlocks;
                runtime.CollectGarbage();
                Check(runtime.AllocatedBlocks < before, "Real cycle collection did not release unreachable owned allocations.");
                Clean(runtime);
            }
        });
        Case("memory-limit", () =>
        {
            using var runtime = new QuickJsRuntime(memoryLimit: 2 * 1024 * 1024);
            using var context = runtime.CreateContext();
            Throws<QuickJsException>(() => Evaluate(context, "globalThis.items=[];for(let i=0;i<1000000;i++)items.push({i,pad:'x'.repeat(128)});"), "memory");
            Clean(runtime);
        });
        Case("callback-error-under-memory-limit", () =>
        {
            const ulong limit = 2 * 1024 * 1024;
            using var runtime = new QuickJsRuntime(memoryLimit: limit);
            using var context = runtime.CreateContext();
            context.RegisterFunction("hostHugeError", _ => throw new Exception(new string('x', 1024 * 1024)));
            long pressureBytes = (long)limit - runtime.AllocatedBytes - 128 * 1024;
            Check(pressureBytes > 0, "Insufficient baseline room for bounded OOM callback test.");
            using var pressure = context.CreateArrayBuffer(new byte[checked((int)pressureBytes)]);
            string error = Text(context, "try { hostHugeError(); 'missing error' } catch(e) { String(e) }");
            Check(error.Contains("memory", StringComparison.OrdinalIgnoreCase), "Huge managed exception message did not produce a catchable memory error.");
            pressure.Dispose();
            runtime.CollectGarbage();
            Check(Text(context, "6*7") == "42", "Runtime did not recover from callback message allocation failure.");
            Clean(runtime);
        });
        Case("recursion-limit", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Throws<QuickJsException>(() => Evaluate(context, "function recurse(){return recurse()+1} recurse();"), "stack");
            Check(Text(context, "1+1") == "2", "Runtime did not recover from recursion.");
            Clean(runtime);
        });
        Case("cancellation", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            using var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            Throws<QuickJsException>(() => { using var result = context.Evaluate("while(true){}", cancellationToken: token.Token); }, "interrupt");
            Check(Text(context, "1+1") == "2", "Runtime did not recover from interruption.");
            Clean(runtime);
        });
        Case("regexp-cancellation", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            using var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            Throws<QuickJsException>(() => { using var result = context.Evaluate("/(a+)+$/.test('a'.repeat(40)+'!')", cancellationToken: token.Token); });
            Check(token.IsCancellationRequested, "RegExp failed before cancellation was requested.");
            Check(Text(context, "1+1") == "2", "Runtime did not recover from RegExp interruption.");
            Clean(runtime);
        });
        Case("parser-recursion-limit", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            string input = new string('(', 20000) + "1" + new string(')', 20000);
            Throws<QuickJsException>(() => Evaluate(context, input), "stack");
            Check(Text(context, "1+1") == "2", "Runtime did not recover from parser recursion.");
            Clean(runtime);
        });
        Case("bounded-job-drain", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Evaluate(context, "globalThis.running=true; function tick(){if(running)Promise.resolve().then(tick)} tick();");
            Throws<QuickJsException>(() => runtime.DrainJobs(10), "budget");
            Evaluate(context, "running=false");
            runtime.DrainJobs();
            Clean(runtime);
        });
        Case("job-cancellation", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Evaluate(context, "Promise.resolve().then(()=>{while(true){}})");
            using var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            Throws<QuickJsException>(() => runtime.DrainJobs(cancellationToken: token.Token), "interrupt");
            Check(Text(context, "6*7") == "42", "Context did not recover from interrupted Promise job.");
            Clean(runtime);
        });
        Case("independent-concurrent-runtimes", () =>
        {
            using var start = new Barrier(2);
            var classIds = new uint[2];
            Task Work(int id) => Task.Run(() =>
            {
                using var runtime = new QuickJsRuntime();
                using var context = runtime.CreateContext();
                if (!start.SignalAndWait(TimeSpan.FromSeconds(15))) throw new Exception("Independent runtime barrier timeout.");
                uint classId = QuickJsHost.NewClassId();
                classIds[id - 1] = classId;
                runtime.Enter();
                try
                {
                    fixed (byte* name = "ConcurrentHostClass\0"u8)
                    {
                        var definition = new VM.JSClassDef { class_name = name };
                        Check(VM.JS_NewClass(runtime.NativeRuntime, classId, &definition) == 0, "Host class registration failed.");
                    }
                    var instance = VM.JS_NewObjectClass(context.NativeContext, (int)classId);
                    if (instance.tag == 6) throw QuickJsContext.TakeException(context.NativeContext);
                    try { Check(VM.JS_GetClassID(instance) == classId, "Host instance lost its class ID."); }
                    finally { VM.JS_FreeValue(context.NativeContext, instance); }
                }
                finally { runtime.Leave(); }
                Evaluate(context, $"globalThis.identity={id}; for(let i=0;i<1000;i++)Math.sqrt(i);");
                Check(Text(context, "identity") == id.ToString(), "Runtime globals leaked.");
                Clean(runtime);
            });
            Task.WaitAll(Work(1), Work(2));
            Check(classIds[0] != 0 && classIds[1] != 0 && classIds[0] != classIds[1], "Concurrent host class IDs collided.");
        });
        Case("disposal-drains-active-callback", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            using var disposing = new ManualResetEventSlim();
            context.RegisterFunction("block", _ => { entered.Set(); if (!release.Wait(TimeSpan.FromSeconds(15))) throw new Exception("Callback drain timeout"); return 42; });
            var execution = Task.Run(() =>
            {
                try { context.Evaluate("block()"); } // Runtime owns and releases this outstanding value.
                catch (QuickJsException error) when (error.Message.Contains("interrupt", StringComparison.OrdinalIgnoreCase)) { }
            });
            Check(entered.Wait(TimeSpan.FromSeconds(15)), "Disposal callback did not start.");
            var disposal = Task.Run(() => { disposing.Set(); runtime.Dispose(); });
            try
            {
                Check(disposing.Wait(TimeSpan.FromSeconds(15)), "Disposal task did not start.");
                Check(!disposal.Wait(TimeSpan.FromMilliseconds(100)), "Runtime disposal freed an active callback.");
            }
            finally { release.Set(); }
            Task.WaitAll(execution, disposal);
            Check(runtime.AllocatedBlocks == 0 && runtime.AllocatedBytes == 0, "Callback-draining disposal leaked.");
        });
        Case("same-runtime-concurrent-entry", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            context.RegisterFunction("block", _ => { entered.Set(); if (!release.Wait(TimeSpan.FromSeconds(15))) throw new Exception("Callback wait timeout"); return 42; });
            var task = Task.Run(() => Text(context, "block()"));
            try
            {
                Check(entered.Wait(TimeSpan.FromSeconds(15)), "Callback did not start.");
                Throws<InvalidOperationException>(() => Evaluate(context, "1"), "Concurrent");
            }
            finally { release.Set(); }
            Check(task.GetAwaiter().GetResult() == "42", "Active callback result changed.");
            Clean(runtime);
        });
    }
    private static void Upstream(string root, string source)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/Upstream/cases.json")));
        foreach (var file in document.RootElement.GetProperty("files").EnumerateObject())
        {
            Case(file.Name, () =>
            {
                string text = File.ReadAllText(Path.Combine(source, "tests", file.Name));
                string[] expected = file.Value.EnumerateArray().Select(x => file.Name + ":" + x.GetString()).ToArray();
                var entrypoints = new Regex(@"^(test(?:_\w+)?)\(([^\n]*)\);$", RegexOptions.Multiline);
                string[] observed = entrypoints.Matches(text).Select(m => m.Groups[1].Value).Where(x => x != "test_finalization_registry").Select(x => file.Name + ":" + x).ToArray();
                Check(observed.SequenceEqual(expected), "Upstream entrypoint inventory drifted.");
                text = entrypoints.Replace(text, m => m.Groups[1].Value == "test_finalization_registry"
                    ? "// Excluded: OS timer event loop."
                    : $"__case('{file.Name}:{m.Groups[1].Value}',()=>{{{m.Value}}});");
                using var runtime = new QuickJsRuntime();
                using var context = runtime.CreateContext();
                Setup(context); javascriptCases.Clear();
                Evaluate(context, text, file.Name);
                runtime.DrainJobs();
                Check(javascriptCases.SequenceEqual(expected), "Missing, repeated or reordered upstream cases.");
                Clean(runtime);
            });
        }
        Case("test_cyclic_import.js", () =>
        {
            var modules = new Dictionary<string, string>();
            foreach (string file in new[] { "test_cyclic_import.js", "fixture_cyclic_import.js", "assert.js" })
                modules[file] = File.ReadAllText(Path.Combine(source, "tests", file));
            using var runtime = new QuickJsRuntime(modules);
            using var context = runtime.CreateContext();
            Evaluate(context, modules["test_cyclic_import.js"], "test_cyclic_import.js", true);
            runtime.DrainJobs();
            Clean(runtime);
        });
        Console.WriteLine("SKIP test_builtin.js:test_finalization_registry requires excluded os.setTimeout event loop");
    }
    private static void Abi()
    {
        Case("actual-jsvalue-layout", () =>
        {
            Check(sizeof(VM.JSValue) == 16, "JSValue size differs from native 16.");
            Check(Marshal.OffsetOf<VM.JSValue>(nameof(VM.JSValue.tag)).ToInt64() == 8, "JSValue tag offset differs from native 8.");
            Check(sizeof(VM.JSValueUnion) == 8, "JSValueUnion size differs from native 8.");
        });
        Case("allocator-layout-and-contract", () =>
        {
            Check(sizeof(VM.JSMallocState) == 32, "JSMallocState native size is 32.");
            Check(Marshal.OffsetOf<VM.JSMallocState>(nameof(VM.JSMallocState.malloc_count)).ToInt64() == 0
                && Marshal.OffsetOf<VM.JSMallocState>(nameof(VM.JSMallocState.malloc_size)).ToInt64() == 8
                && Marshal.OffsetOf<VM.JSMallocState>(nameof(VM.JSMallocState.malloc_limit)).ToInt64() == 16
                && Marshal.OffsetOf<VM.JSMallocState>(nameof(VM.JSMallocState.opaque)).ToInt64() == 24, "JSMallocState field offsets differ from native.");
            VM.JSMallocState state = new() { malloc_limit = 128 };
            Check(QuickJsHost.Allocate(&state, 0) == null && QuickJsHost.Allocate(&state, ulong.MaxValue) == null, "Zero/overflow allocation contract.");
            Check(QuickJsHost.UsableSize(null) == 0, "Null usable-size contract.");
            byte* pointer = (byte*)QuickJsHost.Allocate(&state, 32);
            Check(pointer != null, "Small allocation failed.");
            try
            {
                Check(((nuint)pointer & 15) == 0, "Allocator alignment must be 16 bytes.");
                Check(state.malloc_count == 1 && state.malloc_size == 48 && QuickJsHost.UsableSize(pointer) == 32, "Allocator accounting mismatch.");
                for (int i = 0; i < 32; i++) pointer[i] = (byte)(i ^ 0x5a);
                Check(QuickJsHost.Allocate(&state, 100) == null && QuickJsHost.Reallocate(&state, pointer, 200) == null
                    && QuickJsHost.Reallocate(&state, pointer, ulong.MaxValue) == null, "Memory limits/overflow did not reject.");
                Check(state.malloc_count == 1 && state.malloc_size == 48 && QuickJsHost.UsableSize(pointer) == 32, "Failed realloc changed allocation.");
                for (int i = 0; i < 32; i++) Check(pointer[i] == (byte)(i ^ 0x5a), "Failed realloc destroyed old bytes.");
                byte* replacement = (byte*)QuickJsHost.Reallocate(&state, pointer, 64);
                Check(replacement != null, "Growing realloc failed.");
                pointer = replacement;
                Check(state.malloc_count == 1 && state.malloc_size == 80 && QuickJsHost.UsableSize(pointer) == 64, "Growing realloc accounting mismatch.");
                for (int i = 0; i < 32; i++) Check(pointer[i] == (byte)(i ^ 0x5a), "Growing realloc lost bytes.");
                Check(QuickJsHost.Reallocate(&state, pointer, 0) == null, "Zero-size realloc must free.");
                pointer = null;
            }
            finally { QuickJsHost.Free(&state, pointer); }
            Check(state.malloc_count == 0 && state.malloc_size == 0, "Allocator leaked state.");
        });
        Case("direct-aggregate-callback", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Register(context, "identity", &Identity);
            using var function = context.GetGlobal("identity");
            using var text = context.CreateString("A\0B");
            runtime.Enter();
            try
            {
                VM.JSValue shortBigInt = VM.JS_NewBigInt64(context.NativeContext, long.MaxValue);
                VM.JSValue heapBigInt = VM.JS_NewBigUint64(context.NativeContext, ulong.MaxValue);
                try
                {
                    Check(shortBigInt.tag == 7 && heapBigInt.tag == -9, "Actual BigInt constructor tags disagree with native.");
                    VM.JSValue[] inputs = [new() { tag = 0, u = new() { uint64 = unchecked((uint)-42) } },
                        new() { tag = 8, u = new() { float64 = -0.0 } },
                        new() { tag = 8, u = new() { float64 = double.NaN } }, shortBigInt, heapBigInt, text.Raw];
                    foreach (VM.JSValue value in inputs)
                    {
                        VM.JSValue argument = value;
                        VM.JSValue result = VM.JS_Call(context.NativeContext, function.Raw, new() { tag = 3 }, 1, &argument);
                        try { Check(result.tag == value.tag && result.u.uint64 == value.u.uint64, "Direct callback changed aggregate payload/tag."); }
                        finally { VM.JS_FreeValue(context.NativeContext, result); }
                    }
                }
                finally { VM.JS_FreeValue(context.NativeContext, shortBigInt); VM.JS_FreeValue(context.NativeContext, heapBigInt); }
            }
            finally { runtime.Leave(); }
            Clean(runtime);
        });
        Case("aggregate-callback-payloads", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Register(context, "identity", &Identity);
            Evaluate(context, "for(const v of [-42,-0,NaN,42n,9223372036854775807n,1n<<80n,'A\\0B',{}])if(!Object.is(v,identity(v)))throw Error('aggregate callback mismatch');");
            context.RegisterFunction("hostDouble", args => args[0] * 2);
            Check(Text(context, "hostDouble(21)") == "42", "Managed callback mismatch.");
            using var shortBigInt = context.Evaluate("9223372036854775807n");
            using var heapBigInt = context.Evaluate("1n<<80n");
            Check(shortBigInt.Raw.tag == 7 && heapBigInt.Raw.tag == -9, "BigInt tags differ from native.");
            Clean(runtime);
        });
        Case("allocator-accounting", () =>
        {
            using var runtime = new QuickJsRuntime();
            using var context = runtime.CreateContext();
            Check(runtime.AllocatedBlocks > 0 && runtime.AllocatedBytes > 0, "Allocator counters missing.");
            Evaluate(context, "globalThis.a=new ArrayBuffer(1024*1024)");
            Check(runtime.AllocatedBytes > 1024*1024, "Buffer allocation not accounted.");
            Evaluate(context, "a=null"); runtime.CollectGarbage();
            Clean(runtime);
        });
    }
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length < 2) throw new ArgumentException("Usage: QuickJs.Behavior SUITE CAMPAIGN_ROOT [SOURCE]");
            switch (args[0])
            {
                case "behavior": Behavior(args[1]); break;
                case "lifecycle": Lifecycle(args[1]); break;
                case "upstream": Upstream(args[1], args[2]); break;
                case "abi": Abi(); break;
                case "fail": using (var runtime = new QuickJsRuntime()) using (var context = runtime.CreateContext()) Evaluate(context, "throw Error('deliberate assertion failure')"); break;
                default: throw new ArgumentException("Unknown suite " + args[0]);
            }
            Console.WriteLine($"SUMMARY passed={passed} failed=0");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            Console.Error.WriteLine($"SUMMARY passed={passed} failed=1");
            return 1;
        }
    }
}
