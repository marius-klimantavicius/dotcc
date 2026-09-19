using System.Linq;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    // C profiles commonly leave declarations/references in `if (0 && ...)`
    // branches. C# still binds unreachable source, so omit only branches whose
    // condition is provably effect-free and whose body has no possible entry.
    // This deliberately does not fold general arithmetic, casts or variables.
    private static bool? ConstantTruth(CExpr expression) => expression switch
    {
        LitInt { Value: { } value } => value != 0,
        LitBool literal => literal.Value,
        EnumConstRef enumeration => enumeration.Sym.ConstValue != 0,
        Paren parenthesized => ConstantTruth(parenthesized.Inner),
        ComptimeFold { Resolved: { } resolved } => ConstantTruth(resolved),
        Cast cast when cast.Target.Unqualified == CType.Bool => ConstantTruth(cast.Operand),
        Unary { Op: UnOp.LogNot } unary => ConstantTruth(unary.Operand) is { } truth ? !truth : null,
        Binary { Op: BinOp.LogAnd } binary => ConstantTruth(binary.Left) switch
        {
            false => false,
            true => ConstantTruth(binary.Right),
            null => null,
        },
        Binary { Op: BinOp.LogOr } binary => ConstantTruth(binary.Left) switch
        {
            true => true,
            false => ConstantTruth(binary.Right),
            null => null,
        },
        CondExpr conditional => ConstantTruth(conditional.Cond) is { } chooseThen
            ? ConstantTruth(chooseThen ? conditional.Then : conditional.Else) : null,
        _ => null,
    };

    // A C label (including a case label nested in another statement) can enter
    // a branch without evaluating its condition. Unknown control nodes are
    // retained conservatively. A nested switch's own section labels cannot be
    // entered from outside that switch, but ordinary labels in its bodies can.
    private static bool CanDiscardBranch(CStmt? statement) => statement switch
    {
        null => true,
        Labeled or CaseLabelStmt => false,
        Block block => block.Stmts.All(CanDiscardBranch),
        Seq sequence => sequence.Stmts.All(CanDiscardBranch),
        If conditional => CanDiscardBranch(conditional.Then) && CanDiscardBranch(conditional.Else),
        While loop => CanDiscardBranch(loop.Body),
        DoWhile loop => CanDiscardBranch(loop.Body),
        For loop => CanDiscardBranch(loop.Init) && CanDiscardBranch(loop.Body),
        Switch selection => selection.Sections.All(section => section.Body.All(CanDiscardBranch)),
        DeclStmt or ArrayDecl or ExprStmt or Return or Break or Continue or Goto => true,
        _ => false,
    };

    private bool TryEmitConstantIf(StringBuilder output, If conditional, int indent)
    {
        if (ConstantTruth(conditional.Cond) is not { } chooseThen
            || !CanDiscardBranch(chooseThen ? conditional.Else : conditional.Then)) return false;
        if ((chooseThen ? conditional.Then : conditional.Else) is { } selected)
            Stmt(output, selected, indent);
        return true;
    }
}
