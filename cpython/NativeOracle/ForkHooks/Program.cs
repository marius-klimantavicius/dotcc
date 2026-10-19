using System.Runtime.InteropServices;
using System.Text.Json;
using Managed.CPython;
using Managed.Interpreters;
using PyObject = Managed.Interpreters.CPython._object;

// An owning consumer of the actual selected profile PE. This observer installs
// three C-API test callbacks; every lifecycle algorithm remains in CPython.
internal static unsafe class Program
{
    private static CPython.PyModuleDef* definition;
    private static CPython.PyMethodDef* methods;
    private static readonly List<nint> strings = new();
    private static int releasedOwners;

    private static byte* Text(string text)
    {
        nint memory = Marshal.StringToCoTaskMemUTF8(text);
        strings.Add(memory);
        return (byte*)memory;
    }

    private static PyObject* InitializeModule() => CPython.PyModule_Create2(definition, 1013);

    private static PyObject* Pair(PyObject* self, PyObject* unused)
    {
        CPython.PyOS_BeforeFork();
        int pid = CPython.Libc.fork();
        int savedErrno = CPython.Libc.errno;
        CPython.PyOS_AfterFork_Parent();
        if (pid != -1 || savedErrno != CPython.Libc.EPERM)
            throw new InvalidOperationException("Actual shared fork failed its rejection contract.");
        CPython.Libc.errno = savedErrno;
        return CPython.PyErr_SetFromErrno((PyObject*)CPython.Globals.PyExc_OSError);
    }

    private static PyObject* State(PyObject* self, PyObject* unused) =>
        CPython.PyLong_FromLong(CPython.Globals._PyRuntime.stoptheworld.world_stopped);

    private static PyObject* Released(PyObject* self, PyObject* unused)
    {
        Interlocked.Increment(ref releasedOwners);
        return CPython.PyLong_FromLong(0);
    }

    private static void Install()
    {
        definition = (CPython.PyModuleDef*)NativeMemory.AllocZeroed((nuint)sizeof(CPython.PyModuleDef));
        methods = (CPython.PyMethodDef*)NativeMemory.AllocZeroed(4, (nuint)sizeof(CPython.PyMethodDef));
        if (definition == null || methods == null) throw new OutOfMemoryException();
        // Same-pin Include/object.h: PyModuleDef_HEAD_INIT has an immortal
        // header (UINT32_MAX on this 64-bit release/GIL build). Same-pin
        // Include/modsupport.h defines PYTHON_API_VERSION=1013 and METH_NOARGS
        // in Include/methodobject.h is 0x0004. The probe runner checks these
        // original header definitions before compiling this consumer.
        definition->m_base.ob_base.ob_refcnt = uint.MaxValue;
        definition->m_name = Text("fork_probe");
        definition->m_size = -1;
        definition->m_methods = methods;
        methods[0] = new() { ml_name = Text("pair"), ml_meth = &Pair, ml_flags = 4 };
        methods[1] = new() { ml_name = Text("state"), ml_meth = &State, ml_flags = 4 };
        methods[2] = new() { ml_name = Text("released"), ml_meth = &Released, ml_flags = 4 };
        if (CPython.PyImport_AppendInittab(definition->m_name, &InitializeModule) != 0)
            throw new InvalidOperationException("Control module registration failed.");
    }

    private static int Main(string[] args)
    {
        if (args.Length != 2) throw new ArgumentException("Expected explicit profile stdlib and matching native header-layout report.");
        using var layout = JsonDocument.Parse(File.ReadAllBytes(args[1]));
        int[] expected = layout.RootElement.GetProperty("native_matching_target_header_layouts")
            .GetProperty("upstream-tests-static").EnumerateArray().Select(value => value.GetInt32()).ToArray();
        CPython._is* interpreter = (CPython._is*)NativeMemory.AllocZeroed((nuint)sizeof(CPython._is));
        CPython.pyruntimestate* state = (CPython.pyruntimestate*)NativeMemory.AllocZeroed((nuint)sizeof(CPython.pyruntimestate));
        if (interpreter == null || state == null) throw new OutOfMemoryException();
        long[] actual = [sizeof(CPython._is), (byte*)&interpreter->warnings-(byte*)interpreter,
            sizeof(CPython.pyruntimestate), (byte*)&state->_main_interpreter-(byte*)state,
            sizeof(char), (byte*)&interpreter->before_forkers-(byte*)interpreter,
            (byte*)&interpreter->after_forkers_parent-(byte*)interpreter,
            (byte*)&interpreter->after_forkers_child-(byte*)interpreter];
        if (!actual.SequenceEqual(expected.Select(value => (long)value)))
        {
            Console.Error.WriteLine("debug_offsets="+ ((byte*)&state->debug_offsets-(byte*)state));
            Console.Error.WriteLine("_initialized="+ ((byte*)&state->_initialized-(byte*)state));
            Console.Error.WriteLine("preinitializing="+ ((byte*)&state->preinitializing-(byte*)state));
            Console.Error.WriteLine("preinitialized="+ ((byte*)&state->preinitialized-(byte*)state));
            Console.Error.WriteLine("core_initialized="+ ((byte*)&state->core_initialized-(byte*)state));
            Console.Error.WriteLine("initialized="+ ((byte*)&state->initialized-(byte*)state));
            Console.Error.WriteLine("_finalizing="+ ((byte*)&state->_finalizing-(byte*)state));
            Console.Error.WriteLine("_finalizing_id="+ ((byte*)&state->_finalizing_id-(byte*)state));
            Console.Error.WriteLine("interpreters="+ ((byte*)&state->interpreters-(byte*)state));
            Console.Error.WriteLine("main_thread="+ ((byte*)&state->main_thread-(byte*)state));
            Console.Error.WriteLine("main_tstate="+ ((byte*)&state->main_tstate-(byte*)state));
            Console.Error.WriteLine("xi="+ ((byte*)&state->xi-(byte*)state));
            Console.Error.WriteLine("allocators="+ ((byte*)&state->allocators-(byte*)state));
            Console.Error.WriteLine("obmalloc="+ ((byte*)&state->obmalloc-(byte*)state));
            Console.Error.WriteLine("pyhash_state="+ ((byte*)&state->pyhash_state-(byte*)state));
            Console.Error.WriteLine("threads="+ ((byte*)&state->threads-(byte*)state));
            Console.Error.WriteLine("signals="+ ((byte*)&state->signals-(byte*)state));
            Console.Error.WriteLine("autoTSSkey="+ ((byte*)&state->autoTSSkey-(byte*)state));
            Console.Error.WriteLine("trashTSSkey="+ ((byte*)&state->trashTSSkey-(byte*)state));
            Console.Error.WriteLine("orig_argv="+ ((byte*)&state->orig_argv-(byte*)state));
            Console.Error.WriteLine("parser="+ ((byte*)&state->parser-(byte*)state));
            Console.Error.WriteLine("atexit="+ ((byte*)&state->atexit-(byte*)state));
            Console.Error.WriteLine("imports="+ ((byte*)&state->imports-(byte*)state));
            Console.Error.WriteLine("ceval="+ ((byte*)&state->ceval-(byte*)state));
            Console.Error.WriteLine("gilstate="+ ((byte*)&state->gilstate-(byte*)state));
            Console.Error.WriteLine("getargs="+ ((byte*)&state->getargs-(byte*)state));
            Console.Error.WriteLine("fileutils="+ ((byte*)&state->fileutils-(byte*)state));
            Console.Error.WriteLine("faulthandler="+ ((byte*)&state->faulthandler-(byte*)state));
            Console.Error.WriteLine("tracemalloc="+ ((byte*)&state->tracemalloc-(byte*)state));
            Console.Error.WriteLine("ref_tracer="+ ((byte*)&state->ref_tracer-(byte*)state));
            Console.Error.WriteLine("stoptheworld_mutex="+ ((byte*)&state->stoptheworld_mutex-(byte*)state));
            Console.Error.WriteLine("stoptheworld="+ ((byte*)&state->stoptheworld-(byte*)state));
            Console.Error.WriteLine("preconfig="+ ((byte*)&state->preconfig-(byte*)state));
            Console.Error.WriteLine("open_code_hook="+ ((byte*)&state->open_code_hook-(byte*)state));
            Console.Error.WriteLine("open_code_userdata="+ ((byte*)&state->open_code_userdata-(byte*)state));
            Console.Error.WriteLine("audit_hooks="+ ((byte*)&state->audit_hooks-(byte*)state));
            Console.Error.WriteLine("object_state="+ ((byte*)&state->object_state-(byte*)state));
            Console.Error.WriteLine("float_state="+ ((byte*)&state->float_state-(byte*)state));
            Console.Error.WriteLine("unicode_state="+ ((byte*)&state->unicode_state-(byte*)state));
            Console.Error.WriteLine("types="+ ((byte*)&state->types-(byte*)state));
            Console.Error.WriteLine("cached_objects="+ ((byte*)&state->cached_objects-(byte*)state));
            Console.Error.WriteLine("static_objects="+ ((byte*)&state->static_objects-(byte*)state));
            Console.Error.WriteLine("_main_interpreter="+ ((byte*)&state->_main_interpreter-(byte*)state));
            throw new InvalidOperationException("Actual compiled profile layouts differ from native selected-header control: "+string.Join(",", actual));
        }
        NativeMemory.Free(state);
        NativeMemory.Free(interpreter);
        Install();
        string application = AppContext.BaseDirectory;
        string source = File.ReadAllText(Path.Combine(application, "scenarios.py"));
        Console.WriteLine("target-fork-layout matched-native-header="+string.Join(",", actual));
        for (int cycle = 0; cycle < 2; cycle++)
        {
            using (var runtime = PythonRuntime.Initialize(new PythonOptions(args[0], application)))
            {
                PythonExecutionResult result = runtime.Execute(source, "fork_hook_controls.py");
                if (!result.StandardOutput.Contains("operations=6 child-executions=0", StringComparison.Ordinal))
                    throw new InvalidOperationException("All six actual hook operations did not complete.");
                Console.Write(result.StandardOutput);
                Console.Error.Write(result.StandardError);
                using var reusable = runtime.Evaluate("6 * 7");
                if (reusable.AsInt64() != 42) throw new InvalidOperationException("Continued owning API use failed.");
            }
            if (releasedOwners != (cycle+1)*3)
                throw new InvalidOperationException($"Callback owners released {releasedOwners}, expected {(cycle+1)*3}.");
        }
        // Every test-owned native object and UTF-8 name outlives all callbacks
        // and both real interpreter finalizations. No pointer enters a product.
        NativeMemory.Free(methods);
        NativeMemory.Free(definition);
        foreach (nint text in strings) Marshal.FreeCoTaskMem(text);
        Console.WriteLine("actual-profile-hooks-passed cycles=2 operations=12 child-executions=0 callback-owners-released=6");
        return 0;
    }
}
