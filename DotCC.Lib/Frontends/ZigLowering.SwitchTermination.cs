#nullable enable
using System.Linq;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // A default plus terminating arms proves that execution cannot reach the
    // next statement. An arm's break completes this switch; it is not proof
    // that the enclosing block terminates. Goto targets stay conservative.
    private static bool SwitchCannotComplete(Switch statement) =>
        statement.Sections.Any(section => section.Labels.Any(label => label.CaseExpr is null))
        && statement.Sections.All(section => section.Body.Count > 0 && SwitchArmExits(section.Body[^1]));

    private static bool SwitchArmExits(CStmt statement) => statement switch
    {
        Break or Goto => false,
        Block block => block.Stmts.Count > 0 && SwitchArmExits(block.Stmts[^1]),
        Seq sequence => sequence.Stmts.Count > 0 && SwitchArmExits(sequence.Stmts[^1]),
        If conditional => conditional.Else is { } otherwise
            && SwitchArmExits(conditional.Then) && SwitchArmExits(otherwise),
        DeferGuard guard => SwitchArmExits(guard.Body),
        Switch nested => SwitchCannotComplete(nested),
        _ => Terminates(statement),
    };
}
