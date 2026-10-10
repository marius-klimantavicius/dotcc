using System;
using System.Collections.Generic;
using Edg.Runtime;
using Tests.ArrayTransfer;

internal sealed class OwnedArrayTransfer(int value) : Exception { internal int Value { get; } = value; }
internal static class ArrayRuntimeTransferHost
{
    internal static bool InitiallyTransfer;
    internal static int Mask;
    internal static OwnedArrayTransfer? Latest;
    internal static readonly List<int> Trace = [];
    public static int Main()
    {
        ObjectProgram.InitializeLibrary();
        var scenarios = new (bool Initial, int Mask, int Final)[] {
            (true, 0, 99), (true, (1<<5)|(1<<2)|(1<<4), 4), (true, 1<<2, 2),
            (false, 0, 0), (false, 1<<5, 5), (false, 1<<2, 2), (false, (1<<5)|(1<<2)|(1<<4), 4),
        };
        foreach (var scenario in scenarios)
        {
            InitiallyTransfer=scenario.Initial; Mask=scenario.Mask; Latest=null; Trace.Clear();
            OwnedArrayTransfer? observed=null;
            try { Core.array_runtime_transfer(); } catch (OwnedArrayTransfer error) { observed=error; }
            if (string.Join(" ",Trace)!="99 5 3 2 1 4") throw new Exception("Lost cleanup tail: "+string.Join(" ",Trace));
            if ((observed?.Value ?? 0)!=scenario.Final || !ReferenceEquals(observed,Latest)) throw new Exception("Wrong deferred transfer identity");
            Console.WriteLine((scenario.Initial?1:0)+":"+scenario.Mask+":"+scenario.Final+" "+string.Join(" ",Trace));
        }
        return 0;
    }
}
namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial void transfer_point(int argument0)
        {
            ArrayRuntimeTransferHost.Trace.Add(argument0);
            if (argument0==99 ? ArrayRuntimeTransferHost.InitiallyTransfer : (ArrayRuntimeTransferHost.Mask & (1<<argument0))!=0)
                throw ArrayRuntimeTransferHost.Latest=new OwnedArrayTransfer(argument0);
        }
    }
    public static unsafe partial class ObjectProgram
    {
        static partial void ClassifyTransfer(Exception error, ref bool isRuntimeTransfer)
            => isRuntimeTransfer=error is OwnedArrayTransfer;
    }
}
