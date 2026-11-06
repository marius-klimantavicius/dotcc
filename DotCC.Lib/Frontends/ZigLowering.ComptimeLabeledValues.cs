#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly Stack<LabeledBlockTarget> _sourceComptimeValueBlocks = new();

    private sealed class SourceComptimeValueBreak(LabeledBlockTarget target, IrBuilder.ComptimeValue value) : Exception
    {
        internal LabeledBlockTarget Target { get; } = target;
        internal IrBuilder.ComptimeValue Value { get; } = value;
    }

    // Namespace initializers run in source order. Publishing each real statement
    // lets a later array extent read fields changed by earlier comptime calls.
    private IrBuilder.ComptimeValue EvalSourceComptimeLabeledBlock(string label, Item body, CType resultType)
    {
        using var symbols = EnterSymbolScope();
        using var comptime = EnterComptime();
        var session = _ir.BeginComptimeSession();
        var target = new LabeledBlockTarget
        {
            Label = label, Sink = resultType, ResultType = resultType, EndLabel = "__comptime_value_end",
            Temp = new Symbol { Name = "__comptime_value", Kind = SymKind.Var, Type = resultType },
        };
        _labeledBlocks.Push(target);
        _sourceComptimeValueBlocks.Push(target);
        _loweringForComptimeEval++;
        try
        {
            ExecuteTopComptimeStmt(body, session);
            throw new IrUnsupportedException($"zig: comptime labeled block '{label}' did not yield a value");
        }
        catch (SourceComptimeValueBreak completed) when (ReferenceEquals(completed.Target, target))
        {
            return completed.Value;
        }
        finally
        {
            _loweringForComptimeEval--;
            _sourceComptimeValueBlocks.Pop();
            _labeledBlocks.Pop();
            _ir.EndComptimeSession(session);
        }
    }

    private LabeledBlockTarget? SourceComptimeValueTarget(string label)
    {
        var visible = _labeledBlocks.FirstOrDefault(target => target.Label == label);
        return visible is not null && _sourceComptimeValueBlocks.Contains(visible) ? visible : null;
    }

    private void ExecuteSourceComptimeValueBreak(LabeledBlockTarget target, Item expression, IrBuilder.ComptimeSession session)
    {
        using var hoist = EnterFreshHoist();
        var value = LowerExprSink(expression, target.Sink);
        CStmt returned = new Return(value);
        if (_hoist is { Count: > 0 } before) returned = new Block([.. before, returned]);
        var result = _ir.RunComptimeSessionStmt(session, TopComptimeStores(returned));
        if (result is not { Returned: true, Value: { } known })
            throw new IrUnsupportedException($"zig: comptime break value for '{target.Label}' did not evaluate"
                + (_ir.ComptimeMiss is { } why ? " (" + why + ")" : ""));
        throw new SourceComptimeValueBreak(target, known);
    }
}
