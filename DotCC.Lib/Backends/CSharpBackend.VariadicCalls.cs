#nullable enable
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using static DotCC.Ir.CExpr;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private string RenderIndirectCall(IndirectCall call)
    {
        var signature = call.Callee.Type.Unqualified as CType.Func;
        var callee = Sub(call.Callee, PPostfix);
        var arguments = new List<string>();
        var fixedCount = signature is { Variadic: true, IsNativeCallConv: false }
            ? signature.Params.Count : call.Args.Count;
        for (var index = 0; index < fixedCount && index < call.Args.Count; index++)
            arguments.Add(signature is not null && index < signature.Params.Count
                ? CoercedArg(call.Args[index], signature.Params[index])
                : Sub(DecayEnum(call.Args[index]), PAssign));
        if (signature is { Variadic: true, IsNativeCallConv: false })
        {
            // Function pointers cannot carry a `params` modifier. Supply one
            // target-typed span collection, including [] for an empty pack. C#
            // keeps its storage alive for this synchronous invocation.
            var tail = call.Args.Skip(fixedCount).Select(argument =>
                argument.Type.Unqualified is CType.Func
                    ? $"(void*)(({Cs(argument.Type)})({Expr(argument)}))"
                    : DefaultPromotedArgument(argument));
            arguments.Add("[" + string.Join(", ", tail) + "]");
        }
        return $"{callee}({string.Join(", ", arguments)})";
    }
}
