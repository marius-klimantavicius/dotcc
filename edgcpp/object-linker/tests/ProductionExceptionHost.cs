// Exercise translated source against the shared production exception provider.
using System;
using System.Collections.Generic;
using Edg.Runtime;
using Tests.Production;

internal static unsafe class ProductionExceptionHost
{
    internal static int Selected, Mode;
    internal static readonly List<int> Trace = [];
    public static int Main(string[] args)
    {
        ObjectProgram.InitializeLibrary();
        if (args.Length != 0 && args[0] == "runtime") return ProductionRuntimeTests.Run();
        if (args.Length != 0 && args[0].StartsWith("runtime-failure-", StringComparison.Ordinal))
            return ProductionRuntimeTests.Fail(args[0][16..]);
#if FUNCTION_TRY
        for (Mode = 0; Mode != 6; ++Mode)
            foreach (int failure in Mode < 4 ? new[] { 0, 1, 2, 3, 7, 10, 21, 22 } : Mode == 4 ? new[] { 0, 3, 8 } : new[] { 0 })
            {
                if (Mode == 3 && (failure == 21 || failure == 22)) continue;
                Selected = failure; Trace.Clear();
                Trace.Add(400 + Core.function_try_case(Mode));
                if (CppExceptions.UncaughtExceptions != 0) throw new Exception("Function-try count leaked");
                Console.WriteLine(Mode + ":" + failure + " " + string.Join(" ", Trace));
            }
#elif CROSS_MODULE
        for (int value = 0; value != 4; ++value)
        {
            Trace.Clear(); Trace.Add(400 + Core.production_cross_catch(value));
            if (CppExceptions.UncaughtExceptions != 0) throw new Exception("Cross-module count leaked");
            Console.WriteLine(value + " " + string.Join(" ", Trace));
        }
#else
        if (args.Length != 0) { Selected = 21; Core.typed_case(4); return 1; }
        for (int mode = 0; mode != 14; ++mode)
        {
            Trace.Clear();
            try { Trace.Add(300 + Core.typed_case(mode)); }
            catch (CppException payload)
            {
                var frame = CppExceptions.EnterCatch(payload);
                CppExceptions.MarkCaught(frame);
                try
                {
                    // Modes 5 and 13 intentionally escape to the native/managed host.
                    if (mode == 5) Trace.Add(500 + ((int*)payload.ObjectAddress)[1]);
                    else if (mode == 13) Trace.Add(600 + (int)*(long*)payload.ObjectAddress);
                    else throw new Exception("Unexpected escaping source exception");
                }
                finally { CppExceptions.LeaveCatch(frame); }
                if (payload.ObjectAddress != null) throw new Exception("Escaping payload leaked");
            }
            if (CppExceptions.UncaughtExceptions != 0) throw new Exception("Uncaught count leaked");
            Console.WriteLine(mode + " " + string.Join(" ", Trace));
        }
#endif
        return 0;
    }
}
namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial void record_event(int argument0) => ProductionExceptionHost.Trace.Add(argument0);
#if !CROSS_MODULE
        public static partial int should_fail(int argument0) => argument0 == ProductionExceptionHost.Selected ? 1 : 0;
#endif
#if FUNCTION_TRY
        public static partial int current_mode() => ProductionExceptionHost.Mode;
#endif
    }
}
