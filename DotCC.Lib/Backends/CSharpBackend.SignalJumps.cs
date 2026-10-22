#nullable enable

using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private string? EmitSignalJumpArm(StringBuilder sb, CExpr environment, int ind, string pad)
    {
        if (environment is not Call { Callee: IrBuilder.SignalJumpEnvironmentCallee } call
            || call.Args.Count is not (2 or 3)) return null;
        var buffer = call.Args[0];
        var saveMask = call.Args[1];

        // Materialize each operand before rendering the next: Hoist may emit
        // side effects, and neither operand may be re-evaluated on restart.
        var activation = $"__signalJump{_clCounter++}";
        var armPad = pad;
        bool conditional = call.Args.Count == 3;
        if (conditional)
        {
            sb.Append(pad).Append($"Libc.SignalJumpActivation {activation} = null;\n");
            sb.Append(pad).Append($"if (Cond.B({Expr(call.Args[2])}))\n").Append(pad).Append("{\n");
            armPad = Pad(ind + 1);
        }
        var pointer = Hoist(sb, armPad, () => Coerced(buffer, new CType.Pointer(CType.ULongLong)));
        sb.Append(armPad).Append($"ulong* {activation}Buffer = {pointer};\n");
        var save = Hoist(sb, armPad, () => Coerced(saveMask, CType.Int));
        if (conditional)
        {
            sb.Append(armPad).Append($"{activation} = Libc.ArmSignalJumpBuffer({activation}Buffer, {save});\n");
            sb.Append(pad).Append("}\n");
            sb.Append(pad).Append($"using ({activation})\n");
        }
        else sb.Append(pad).Append($"using (var {activation} = Libc.ArmSignalJumpBuffer({activation}Buffer, {save}))\n");
        sb.Append(pad).Append("{\n");
        return activation;
    }

    private bool TryEmitSignalJumpGuard(StringBuilder sb, SetjmpGuard guard, int ind, string pad)
    {
        var activation = EmitSignalJumpArm(sb, guard.Env, ind, pad);
        if (activation == null) return false;
        var inner = Pad(ind + 1);
        sb.Append(inner).Append("try\n");
        GuardBody(sb, guard.TryBody, ind + 1);
        sb.Append(inner).Append($"catch (Libc.JumpBufferException __jmp) when ({activation} != null && __jmp.Identity == {activation}.Identity)\n");
        sb.Append(inner).Append("{\n");
        sb.Append(Pad(ind + 2)).Append($"{activation}.Restore(__jmp);\n");
        if (guard.CatchBody != null) Stmt(sb, guard.CatchBody, ind + 2);
        sb.Append(inner).Append("}\n");
        sb.Append(pad).Append("}\n");
        return true;
    }

    private bool TryEmitSignalJumpCapture(StringBuilder sb, SetjmpCapture capture, int ind, string pad)
    {
        var activation = EmitSignalJumpArm(sb, capture.Env, ind, pad);
        if (activation == null) return false;
        var inner = Pad(ind + 1);
        var label = $"__setjmp_{capture.Id}";
        if (capture.ResetTargetAfterArm)
            sb.Append(inner).Append($"{Expr(capture.Target)} = 0;\n");
        sb.Append(inner).Append(label).Append(":\n");
        sb.Append(inner).Append("try\n");
        GuardBody(sb, capture.Body, ind + 1);
        sb.Append(inner).Append($"catch (Libc.JumpBufferException __jmp) when ({activation} != null && __jmp.Identity == {activation}.Identity)\n");
        sb.Append(inner).Append("{\n");
        // Restoration precedes the result write and all recovery evaluation.
        sb.Append(Pad(ind + 2)).Append($"{activation}.Restore(__jmp);\n");
        sb.Append(Pad(ind + 2)).Append($"{Expr(capture.Target)} = ({Cs(capture.Target.Type)})__jmp.Value;\n");
        sb.Append(Pad(ind + 2)).Append($"goto {label};\n");
        sb.Append(inner).Append("}\n");
        sb.Append(pad).Append("}\n");
        return true;
    }
}
