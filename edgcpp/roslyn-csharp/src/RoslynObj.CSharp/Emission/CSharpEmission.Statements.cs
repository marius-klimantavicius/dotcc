using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private HashSet<string>? statementLocalNames;
        private readonly Stack<SwitchOperation> statementSwitches = [];
        private readonly HashSet<LabelSymbol> statementLabels = [];
        private readonly HashSet<StatementOperation> enteredStatementScopes = [];
        private bool targetStatements;

        private static IEnumerable<StatementOperation> StatementChildren(StatementOperation statement) => statement switch
        {
            BlockOperation block => block.Statements,
            ConditionalOperation conditional => new[] { conditional.Then, conditional.Else }.OfType<StatementOperation>(),
            LoopOperation loop => new[] { loop.Initialization, loop.Body }.OfType<StatementOperation>(),
            SwitchOperation selection => new[] { selection.Body }.OfType<StatementOperation>(),
            _ => []
        };

        private static bool NeedsFlatStatements(StatementOperation statement) => statement is SwitchOperation or JumpOperation { JumpKind.Name: "goto" }
            || StatementChildren(statement).Any(NeedsFlatStatements);

        // C permits entry into nested compound/if/switch bodies. C# label scope
        // does not. These bodies already have method-scope local storage, so
        // retained statement roots can use flat labels without moving any init.
        // No state machine, source lookup or inferred cleanup is involved.
        private void PrepareFlatStatements(StatementOperation root, HashSet<string> localNames)
        {
            statementLocalNames = localNames;
            statementLabels.Clear();
            enteredStatementScopes.Clear();
            statementSwitches.Clear();
            var targets = new HashSet<LabelSymbol>();
            var labelScopes = new Dictionary<LabelSymbol, StatementOperation[]>();
            var labelArms = new Dictionary<LabelSymbol, KeyValuePair<ConditionalOperation, bool>[]>();
            var jumps = new List<(LabelSymbol Target, StatementOperation[] Scopes, KeyValuePair<ConditionalOperation, bool>[] Arms)>();
            var ancestry = new List<StatementOperation>();
            var arms = new Dictionary<ConditionalOperation, bool>();
            var caseOccurrences = new HashSet<SwitchCaseEntry>();
            var selections = new List<SwitchOperation>();
            void Visit(StatementOperation statement)
            {
                if (statement is LabelOperation label)
                {
                    if (!statementLabels.Add(label.Label)) throw Limit("One retained label occurs more than once in a statement body.", "jump");
                    Name(label.Label, label.Label.Name ?? "__label", localNames);
                    labelScopes[label.Label] = ancestry.ToArray();
                    labelArms[label.Label] = arms.ToArray();
                }
                if (statement is JumpOperation { Target: { } target })
                {
                    targets.Add(target);
                    Name(target, target.Name ?? "__label", localNames);
                    jumps.Add((target, ancestry.ToArray(), arms.ToArray()));
                }
                if (statement is SwitchOperation selection)
                {
                    selections.Add(selection);
                    foreach (var entry in selection.Cases)
                        Name(entry, entry.IsDefault ? "__default" : "__case", localNames);
                    if (selection.DefaultCase is { } fallback && !selection.Cases.Contains(fallback))
                        throw Limit("A retained switch default is absent from its captured case association.", "switch");
                }
                if (statement is SwitchCaseOperation selectedCase)
                {
                    if (!caseOccurrences.Add(selectedCase.Entry))
                        throw Limit("A switch case has multiple retained marker occurrences.", "switch");
                    var ownerIndex = ancestry.FindLastIndex(scope => scope.Equals(selectedCase.EnclosingSwitch));
                    if (ownerIndex < 0) throw Limit("A case marker has no retained enclosing switch scope.", "switch");
                    foreach (var scope in ancestry.Skip(ownerIndex + 1)) enteredStatementScopes.Add(scope);
                }
                var controlScope = statement is ConditionalOperation or LoopOperation or SwitchOperation;
                if (controlScope) ancestry.Add(statement);
                if (statement is ConditionalOperation conditional)
                {
                    arms[conditional] = true;
                    if (conditional.Then is not null) Visit(conditional.Then);
                    arms[conditional] = false;
                    if (conditional.Else is not null) Visit(conditional.Else);
                    arms.Remove(conditional);
                }
                else foreach (var child in StatementChildren(statement)) Visit(child);
                if (controlScope) ancestry.RemoveAt(ancestry.Count - 1);
            }
            Visit(root);
            if (targets.Any(target => !statementLabels.Contains(target)))
                throw Limit("A retained jump target has no label occurrence in this selected body.", "jump");
            foreach (var jump in jumps)
            {
                foreach (var scope in labelScopes[jump.Target].Where(scope => !jump.Scopes.Contains(scope)))
                    enteredStatementScopes.Add(scope);
                foreach (var arm in labelArms[jump.Target].Where(arm => !jump.Arms.Contains(arm)))
                    enteredStatementScopes.Add(arm.Key);
            }
            if (selections.Any(selection => selection.Cases.Any(entry => !caseOccurrences.Contains(entry))))
                throw Limit("A captured switch case has no marker occurrence in its selected body.", "switch");
        }

        private bool TryTargetStatement(StatementOperation statement, CodeWriter writer)
        {
            if (!targetStatements) return false;
            if (statement is BlockOperation or SwitchOperation or SwitchCaseOperation or LabelOperation
                or JumpOperation { JumpKind.Name: not "return" } || enteredStatementScopes.Contains(statement))
            {
                FlatStatement(statement, writer);
                return true;
            }
            return false;
        }

        private string StatementName(string stem)
        {
            string name;
            do { name = InternalName(stem + "_" + ++expressionTemporaryOrdinal); }
            while (statementLocalNames?.Contains(name) == true);
            statementLocalNames?.Add(name);
            return name;
        }

        private void FlatStatement(StatementOperation statement, CodeWriter writer)
        {
            switch (statement)
            {
                case BlockOperation block:
                    foreach (var child in block.Statements) Statement(child, writer);
                    return;
                case ConditionalOperation conditional:
                {
                    if (conditional.Initialization is not null)
                        throw Limit("A condition initialization needs its selected execution contract.", "initialization");
                    var otherwise = StatementName("__else");
                    var finished = conditional.Else is null ? otherwise : StatementName("__if_end");
                    var condition = ConditionOccurrence(Required(conditional.Condition), writer);
                    writer.Line($"if (!({condition})) goto {otherwise};");
                    if (conditional.Then is not null) Statement(conditional.Then, writer);
                    if (conditional.Else is not null)
                    {
                        writer.Line($"goto {finished};");
                        writer.Line(otherwise + ":;");
                        Statement(conditional.Else, writer);
                    }
                    writer.Line(finished + ":;");
                    return;
                }
                case LoopOperation loop:
                    FlatLoop(loop, writer);
                    return;
                case SwitchOperation selection:
                    FlatSwitch(selection, writer);
                    return;
                case SwitchCaseOperation selectedCase:
                    if (!statementSwitches.TryPeek(out var owner) || !owner.Equals(selectedCase.EnclosingSwitch)
                        || !owner.Cases.Contains(selectedCase.Entry) || !names.TryGetValue(selectedCase.Entry, out var caseName))
                        throw Limit("A case occurrence is not associated with its retained enclosing switch.", "switch");
                    writer.Line(caseName + ":;");
                    return;
                case LabelOperation label:
                    writer.Line(names[label.Label] + ":;");
                    return;
                case JumpOperation jump when jump.JumpKind.Name != "return":
                    if (jump.Jump.Initialization is not null)
                        throw Limit("A jump carries an initialization obligation without an execution adapter.", "jump");
                    if (jump.JumpKind.Name is "goto" or "break" or "continue" && jump.Target is { } target)
                    {
                        writer.Line("goto " + names[target] + ";");
                        return;
                    }
                    throw Limit($"Jump '{jump.JumpKind}' has no retained supported target.", "jump");
                default:
                    Statement(statement, writer);
                    return;
            }
        }

        private void FlatLoop(LoopOperation loop, CodeWriter writer)
        {
            if (loop.Kind.Name is not ("stmk_for" or "stmk_while" or "stmk_end_test_while") || loop.Affinity is not null)
                throw Limit($"Loop '{loop.Kind}' has no admitted flat occurrence schedule.", "loop");
            if (loop.Initialization is not null) Statement(loop.Initialization, writer);
            var test = StatementName("__loop_test");
            var continuation = StatementName("__loop_continue");
            var end = StatementName("__loop_end");
            writer.Line(test + ":;");
            if (loop.Kind.Name != "stmk_end_test_while" && loop.Condition is not null)
                writer.Line($"if (!({ConditionOccurrence(loop.Condition, writer)})) goto {end};");
            if (loop.Body is not null) Statement(loop.Body, writer);
            writer.Line(continuation + ":;");
            if (loop.Kind.Name == "stmk_end_test_while")
                writer.Line($"if ({ConditionOccurrence(Required(loop.Condition), writer)}) goto {test};");
            else
            {
                if (loop.Increment is not null) ExpressionStatementOccurrence(loop.Increment, writer);
                writer.Line($"goto {test};");
            }
            writer.Line(end + ":;");
        }

        private void FlatSwitch(SwitchOperation selection, CodeWriter writer)
        {
            var selector = Required(selection.Selector);
            if (Unwrap(selector.Type) is not ArithmeticType { Arithmetic.ArithmeticKind.Name: "signed_integer" or "unsigned_integer" or "character" or "boolean" })
                throw Limit("A switch selector needs its captured ordinary integer representation.", "switch");
            var value = ValueOccurrence(selector, writer);
            var captured = StatementName("__switch_value");
            var end = StatementName("__switch_end");
            writer.Line($"var {captured} = {value};");
            foreach (var entry in selection.Cases.Where(entry => !entry.IsDefault))
            {
                if (entry.LowConstant?.Value is not IntegerValue || entry.HighConstant is { Value: not IntegerValue })
                    throw Limit("A switch case needs its retained exact integer constant/range.", "switch");
                var low = Constant(entry.LowConstant);
                var comparison = entry.HighConstant is { } high
                    ? $"({captured} >= {low}) && ({captured} <= {Constant(high)})" : $"{captured} == {low}";
                writer.Line($"if ({comparison}) goto {names[entry]};");
            }
            writer.Line("goto " + (selection.DefaultCase is { } fallback ? names[fallback] : end) + ";");
            statementSwitches.Push(selection);
            try { if (selection.Body is not null) Statement(selection.Body, writer); }
            finally { statementSwitches.Pop(); }
            writer.Line(end + ":;");
        }
    }
}
