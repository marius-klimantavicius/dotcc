// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten bounded counterparts of statements.c scan_condition,
// dependent_statement/if_statement/while_statement/do_statement/for_statement,
// structured break/continue resolution and interpret.c process_stmt_work.
// Scalar assignment follows expr.c and interpret.c eok_assign/*_assign.
using System.Numerics;

namespace EdgSharp;

internal static class ScalarExpressionEffects
{
    internal static bool HasSideEffects(Expression expression)
    {
        // A flat source sum produces a deep left-associated graph. Queries must
        // not consume the CLR stack before interpreter budgets can be applied.
        var pending = new Stack<Expression>(); pending.Push(expression);
        while (pending.TryPop(out Expression? current))
        {
            switch (current)
            {
                case AssignmentExpression or CallExpression or DependentCallExpression: return true;
                case ParenthesizedExpression p: pending.Push(p.Operand); break;
                case ConversionExpression c: pending.Push(c.Operand); break;
                case UnaryExpression u: pending.Push(u.Operand); break;
                case BinaryExpression b: pending.Push(b.Right); pending.Push(b.Left); break;
                case ReferenceBindingExpression b: pending.Push(b.Referent); break;
                case MaterializedTemporaryExpression t: pending.Push(t.Initializer); break;
                case DependentUnaryExpression u: pending.Push(u.Operand); break;
                case DependentBinaryExpression b: pending.Push(b.Right); pending.Push(b.Left); break;
                case DependentConversionExpression c: pending.Push(c.Operand); break;
                case SubscriptExpression s: pending.Push(s.SourceRight); pending.Push(s.SourceLeft); break;
                case PointerArithmeticExpression p: pending.Push(p.Offset); pending.Push(p.Pointer); break;
                case StringLiteralExpression or ArrayInitializationExpression or SizeOfExpression or DependentSizeOfExpression: break;
                case FloatingLiteralExpression or LiteralExpression or ReferenceExpression or OverloadSetExpression or UnresolvedFunctionExpression or ErrorExpression: break;
                default:
                    // Dispatch a new child's override (e.g. unevaluated sizeof).
                    // The base implementation for an unknown root returns true,
                    // so this also terminates conservatively without reflection.
                    if (ReferenceEquals(current, expression) || current.HasSideEffects) return true;
                    break;
            }
        }
        return false;
    }
}

internal static class ScalarExpressionDependencies
{
    internal static bool IsValueDependent(Expression expression)
    {
        var pending = new Stack<Expression>(); pending.Push(expression);
        while (pending.TryPop(out Expression? current))
        {
            if (current.IsTypeDependent) return true;
            switch (current)
            {
                case DependentUnaryExpression or DependentBinaryExpression or DependentConversionExpression or DependentCallExpression or DependentSizeOfExpression: return true;
                case ReferenceExpression reference:
                    if (reference.ReferencedSymbol.HasTemplateDependentValue) return true;
                    break;
                case ParenthesizedExpression p: pending.Push(p.Operand); break;
                case ConversionExpression c: pending.Push(c.Operand); break;
                case UnaryExpression u: pending.Push(u.Operand); break;
                case BinaryExpression b: pending.Push(b.Right); pending.Push(b.Left); break;
                case AssignmentExpression a: pending.Push(a.Target); pending.Push(a.Value); break;
                case CallExpression call:
                    foreach (Expression argument in call.Arguments) pending.Push(argument);
                    break;
                case ReferenceBindingExpression b: pending.Push(b.Source); break;
                case MaterializedTemporaryExpression t: pending.Push(t.Initializer); break;
                case SubscriptExpression s: pending.Push(s.SourceRight); pending.Push(s.SourceLeft); break;
                case PointerArithmeticExpression p: pending.Push(p.Offset); pending.Push(p.Pointer); break;
                case StringLiteralExpression or ArrayInitializationExpression or SizeOfExpression: break;
                case FloatingLiteralExpression or LiteralExpression or OverloadSetExpression or UnresolvedFunctionExpression or ErrorExpression: break;
                default:
                    if (!ReferenceEquals(current, expression) && current.IsValueDependent) return true;
                    break;
            }
        }
        return false;
    }
}

internal sealed partial class Parser
{
    private readonly Stack<LoopTarget> loopTargets = [];
    private Scope? forBodyParent;

    private Expression ControlCondition(bool selection = false)
    {
        if (selection && Is(TokenKind.Semicolon)) UnsupportedAndThrow("Selection initializer statements are not implemented.");
        if (StartsDeclaration()) UnsupportedAndThrow("Condition declarations and selection initializers are not implemented.");
        Expression condition = Convert(ScanExprFull(), types.Integer(IntegerKind.Bool));
        if (selection && Is(TokenKind.Semicolon)) UnsupportedAndThrow("Selection initializer statements are not implemented.");
        return condition;
    }

    private BlockStatement Substatement(Scope? forScope = null)
    {
        Scope? saved = forBodyParent;
        forBodyParent = forScope;
        try
        {
            if (Is(TokenKind.LeftBrace)) return CompoundStatement();
            // statements.c dependent_statement: even an unbraced declaration
            // is scoped to its substatement, never to the surrounding block.
            Scope scope = state.PushScope(ScopeKind.Block);
            try
            {
                Statement statement = Statement();
                return new BlockStatement(scope, [statement], statement.Range);
            }
            finally { state.PopScope(); }
        }
        finally { forBodyParent = saved; }
    }

    private IfStatement IfStatement()
    {
        Token start = Take();
        if (Is("constexpr") || Is("consteval")) UnsupportedAndThrow("constexpr/consteval selection statements are not implemented.");
        Expect(TokenKind.LeftParen, "'(' after if");
        Expression condition = ControlCondition(selection: true);
        Expect(TokenKind.RightParen, "')' after condition");
        BlockStatement thenStatement = Substatement();
        BlockStatement? elseStatement = Eat("else") ? Substatement() : null;
        return new IfStatement(condition, thenStatement, elseStatement, SourceRange.Cover(start.Range, (elseStatement ?? thenStatement).Range));
    }

    private LoopStatement IterationStatement()
    {
        Token start = Take();
        var target = new LoopTarget();
        if (start.Spelling == "for") return ForStatement(start, target);
        if (start.Spelling == "while")
        {
            Expect(TokenKind.LeftParen, "'(' after while");
            Expression condition = ControlCondition();
            Expect(TokenKind.RightParen, "')' after condition");
            BlockStatement body = LoopBody(target);
            return new WhileStatement(target, condition, body, SourceRange.Cover(start.Range, body.Range));
        }
        BlockStatement doBody = LoopBody(target);
        if (!Eat("while")) { Fail("PAR001", "Expected while after do body.", Current.Range); throw new ParseFailure(); }
        Expect(TokenKind.LeftParen, "'(' after while");
        if (StartsDeclaration()) { Fail("CTL002", "A do-while condition cannot be a declaration.", Current.Range); throw new ParseFailure(); }
        Expression doCondition = Convert(ScanExprFull(), types.Integer(IntegerKind.Bool));
        Expect(TokenKind.RightParen, "')' after condition");
        Token end = Expect(TokenKind.Semicolon, "';' after do-while");
        return new DoStatement(target, doCondition, doBody, SourceRange.Cover(start.Range, end.Range));
    }

    private BlockStatement LoopBody(LoopTarget target, Scope? forScope = null)
    {
        loopTargets.Push(target);
        try { return Substatement(forScope); }
        finally { loopTargets.Pop(); }
    }

    private ForStatement ForStatement(Token start, LoopTarget target)
    {
        Expect(TokenKind.LeftParen, "'(' after for");
        // Detect the unimplemented range separator before a declaration scan
        // can misdiagnose a valid range-for as a missing semicolon.
        int nesting = 0;
        for (int lookahead = 0; ; ++lookahead)
        {
            TokenKind kind = state.Tokens.Peek(lookahead).Kind;
            if (kind == TokenKind.EndOfSource || nesting == 0 && kind is TokenKind.Semicolon or TokenKind.RightParen) break;
            if (nesting == 0 && kind == TokenKind.Colon) UnsupportedAndThrow("Range-based for statements are not implemented.");
            if (kind is TokenKind.LeftParen or TokenKind.LeftBrace or TokenKind.LeftBracket) ++nesting;
            if (kind is TokenKind.RightParen or TokenKind.RightBrace or TokenKind.RightBracket) --nesting;
        }
        Scope scope = state.PushScope(ScopeKind.Block);
        try
        {
            Statement initializer;
            if (Is(TokenKind.Semicolon)) initializer = new EmptyStatement(Take().Range);
            else if (StartsDeclaration())
            {
                if (Is("using")) UnsupportedAndThrow("Alias declarations in for initializers are not implemented in this profile.");
                initializer = new DeclarationStatement(Declaration(allowFunction: false));
            }
            else
            {
                SourceRange initStart = Current.Range;
                Expression value = ScanExprFull();
                Token initEnd = ExpectExpressionSemicolon();
                initializer = new ExpressionStatement(value, SourceRange.Cover(initStart, initEnd.Range));
            }
            Expression? condition = Is(TokenKind.Semicolon) ? null : ControlCondition();
            Expect(TokenKind.Semicolon, "';' after for condition");
            Expression? increment = Is(TokenKind.RightParen) ? null : ScanExprFull();
            Expect(TokenKind.RightParen, "')' after for increment");
            BlockStatement body = LoopBody(target, scope);
            return new ForStatement(target, scope, initializer, condition, increment, body, SourceRange.Cover(start.Range, body.Range));
        }
        finally { state.PopScope(); }
    }

    private Statement LoopTransferStatement()
    {
        Token start = Take();
        Token end = Expect(TokenKind.Semicolon, "';' after loop transfer");
        SourceRange range = SourceRange.Cover(start.Range, end.Range);
        if (!loopTargets.TryPeek(out LoopTarget? target))
        {
            Fail("CTL001", $"{start.Spelling} requires an enclosing loop.", start.Range);
            return new ErrorStatement(range);
        }
        return start.Spelling == "break" ? new BreakStatement(target, range) : new ContinueStatement(target, range);
    }
}

public enum ConstantControlFlowKind { Then, Else, LoopBody, LoopExit, Break, Continue, Return }
public sealed class ConstantControlFlowObservation
{
    public Statement Statement { get; }
    public ConstantControlFlowKind Kind { get; }
    public int Invocation { get; }
    internal ConstantControlFlowObservation(Statement statement, ConstantControlFlowKind kind, int invocation)
        => (Statement, Kind, Invocation) = (statement, kind, invocation);
}
public sealed class ConstantAssignmentObservation
{
    public AssignmentExpression Expression { get; }
    public ConstantStorageObject Object { get; }
    /// <summary>The value stored at this operation, independent of subsequent writes to the cell.</summary>
    public BigInteger? IntegerValue { get; }
    public FloatingConstantValue? FloatingValue { get; }
    public BigInteger Value => IntegerValue ?? throw new InvalidOperationException("This assignment stores a floating value; inspect FloatingValue.");
    public int Invocation { get; }
    internal ConstantAssignmentObservation(AssignmentExpression expression, ConstantStorageObject storage, ConstantRuntimeValue value, int invocation)
        => (Expression, Object, IntegerValue, FloatingValue, Invocation) = (expression, storage, value.Integer,
            value.Floating == null ? null : new FloatingConstantValue(value.Floating), invocation);
}

internal sealed partial class ConstantInterpreter
{
    private sealed record AssignmentValue(AssignmentExpression Expression) : Work;
    private sealed record SelectBranch(IfStatement Statement, CallFrame Frame) : Work;
    private sealed record TestLoop(LoopStatement Statement, CallFrame Frame) : Work;
    private sealed record SelectLoopBody(LoopStatement Statement, CallFrame Frame) : Work;
    private sealed record AfterLoopBody(LoopStatement Statement, CallFrame Frame) : Work;
    private sealed record NextUnsequencedOperand(UnsequencedRegion Region) : Work;
    private sealed record FinishUnsequenced(UnsequencedRegion Region, BinaryExpression Expression) : Work;
    private sealed class UnsequencedRegion(int invocation)
    {
        internal int Invocation { get; } = invocation;
        internal bool Right { get; set; }
        internal HashSet<ConstantCell> Reads { get; } = [];
        internal HashSet<ConstantCell> Writes { get; } = [];
    }
    private readonly List<UnsequencedRegion> unsequenced = [];
    private readonly List<ConstantControlFlowObservation> controlFlow = [];
    private readonly List<(AssignmentExpression Expression, ConstantCell Cell, ConstantRuntimeValue Value, int Invocation)> assignments = [];
    private int CurrentInvocation => calls.TryPeek(out CallFrame? frame) ? frame.Identity : 0;
    private void Observe(Statement statement, ConstantControlFlowKind kind, CallFrame frame)
        => controlFlow.Add(new(statement, kind, frame.Identity));
    private void ClearScalarControlState() { unsequenced.Clear(); controlFlow.Clear(); assignments.Clear(); }

    private void NoteAccess(ConstantCell cell, bool write)
    {
        // exprutil.c merge_seq_pt_info_lists: conflicts between unsequenced
        // operand evaluations are UB. Nested function executions are separately
        // sequenced; caller argument computations still have caller identity.
        foreach (UnsequencedRegion region in unsequenced)
        {
            if (region.Invocation != CurrentInvocation) continue;
            if (region.Right)
            {
                if (region.Writes.Contains(cell) || write && region.Reads.Contains(cell))
                    Stop(ConstantEvaluationStatus.UndefinedBehavior, "Unsequenced scalar modification and access in binary operands.");
            }
            else if (write) region.Writes.Add(cell);
            else region.Reads.Add(cell);
        }
    }

    private void ScheduleBinary(BinaryExpression expression)
    {
        var region = new UnsequencedRegion(CurrentInvocation);
        unsequenced.Add(region);
        work.Push(new FinishUnsequenced(region, expression));
        work.Push(new EvaluateExpression(expression.Right));
        work.Push(new NextUnsequencedOperand(region));
        work.Push(new EvaluateExpression(expression.Left));
    }
    private void ScheduleAssignment(AssignmentExpression expression)
    {
        // Dependency failures are reached through normal operand work items;
        // never walk an arbitrarily deep graph ahead of the work budget.
        // C++17+ RHS is sequenced before LHS, including address-producing
        // calls. A compound assignment evaluates its destination only once.
        work.Push(new AssignmentValue(expression));
        work.Push(new EvaluateExpression(expression.Target));
        work.Push(new EvaluateExpression(expression.Value));
    }
    private void Assign(AssignmentExpression expression)
    {
        ConstantCell cell = Location(values.Pop());
        ConstantRuntimeValue input = values.Pop();
        EnsureAlive(cell);
        if (!cell.Known) Stop(ConstantEvaluationStatus.NotConstant, "Cannot modify an unknown runtime object.");
        if (cell.ReadOnly)
            Stop(ConstantEvaluationStatus.UndefinedBehavior, "Cannot modify const storage.");
        if (!cell.BeganInEvaluation)
            Stop(ConstantEvaluationStatus.NotConstant, "Cannot modify an object whose lifetime began outside this constant evaluation.");
        if (expression.OperationType is FloatingType floatingType)
        {
            ConstantRuntimeValue leftValue = ConvertScalar(LoadScalar(ConstantRuntimeValue.Address(cell)), floatingType);
            TokenKind op = expression.Operator switch { TokenKind.PlusEqual => TokenKind.Plus, TokenKind.MinusEqual => TokenKind.Minus,
                TokenKind.StarEqual => TokenKind.Star, TokenKind.SlashEqual => TokenKind.Slash, _ => throw new InvalidOperationException() };
            input = FloatingOperation(op, FloatingScalar(leftValue), FloatingScalar(input));
        }
        else if (expression.Operator != TokenKind.Equal)
        {
            BigInteger value = Scalar(input);
            IntegerType operationType = (IntegerType)expression.OperationType!;
            BigInteger left = TypeTable.ConvertInteger(Load(ConstantRuntimeValue.Address(cell)), operationType);
            if ((expression.Operator is TokenKind.SlashEqual or TokenKind.PercentEqual) &&
                (value.IsZero || operationType.IsSigned && left == operationType.Minimum && value == -1))
                Stop(ConstantEvaluationStatus.UndefinedBehavior, "Invalid integer division in compound assignment.");
            value = expression.Operator switch
            {
                TokenKind.PlusEqual => left + value, TokenKind.MinusEqual => left - value,
                TokenKind.StarEqual => left * value, TokenKind.SlashEqual => left / value,
                TokenKind.PercentEqual => left % value, TokenKind.AmpersandEqual => left & value,
                TokenKind.PipeEqual => left | value, TokenKind.CaretEqual => left ^ value,
                _ => throw new InvalidOperationException("Unexpected scalar assignment operator.")
            };
            if (operationType.IsSigned && (value < operationType.Minimum || value > operationType.Maximum))
                Stop(ConstantEvaluationStatus.UndefinedBehavior, "Signed integer overflow in compound assignment.");
            input = ConstantRuntimeValue.Scalar(TypeTable.ConvertInteger(value, operationType));
        }
        input = ConvertScalar(input, cell.Type);
        NoteAccess(cell, write: true);
        StoreScalar(cell, input);
        assignments.Add((expression, cell, input, CurrentInvocation));
        values.Push(ConstantRuntimeValue.Address(cell));
    }

    private void ScheduleCondition(Expression condition, Work continuation)
    {
        // A condition's temporaries expire before its dependent statement.
        // Preserve only its scalar bool across the full-expression boundary.
        work.Push(continuation); work.Push(new EndFullExpression());
        work.Push(new EvaluateExpression(condition)); work.Push(new BeginFullExpression());
    }
    private void ScheduleLoopBody(LoopStatement statement, CallFrame frame)
    {
        Observe(statement, ConstantControlFlowKind.LoopBody, frame);
        work.Push(new AfterLoopBody(statement, frame));
        work.Push(new ExecuteBlock(statement.Body, frame));
    }
    private bool ProcessScalarControlWork(Work next)
    {
        switch (next)
        {
            case AssignmentValue item: Assign(item.Expression); break;
            case NextUnsequencedOperand item: item.Region.Right = true; break;
            case FinishUnsequenced item:
                if (!ReferenceEquals(unsequenced[^1], item.Region)) throw new InvalidOperationException("Unsequenced work scopes are inconsistent.");
                unsequenced.RemoveAt(unsequenced.Count - 1); EvaluateBinary(item.Expression); break;
            case SelectBranch item:
                bool truth = !Scalar(values.Pop()).IsZero;
                Observe(item.Statement, truth ? ConstantControlFlowKind.Then : ConstantControlFlowKind.Else, item.Frame);
                BlockStatement? selected = truth ? item.Statement.Then : item.Statement.Else;
                if (selected != null) work.Push(new ExecuteBlock(selected, item.Frame));
                break;
            case TestLoop item:
                if (item.Frame.HasTransfer) break;
                if (item.Statement.Condition == null) ScheduleLoopBody(item.Statement, item.Frame);
                else ScheduleCondition(item.Statement.Condition, new SelectLoopBody(item.Statement, item.Frame));
                break;
            case SelectLoopBody item:
                if (!Scalar(values.Pop()).IsZero) ScheduleLoopBody(item.Statement, item.Frame);
                else Observe(item.Statement, ConstantControlFlowKind.LoopExit, item.Frame);
                break;
            case AfterLoopBody item:
                if (item.Frame.Returned) break;
                if (item.Frame.TransferTarget != null)
                {
                    // Only this resolved owner may consume the transfer. All
                    // intervening LeaveScope items have already run.
                    if (!ReferenceEquals(item.Frame.TransferTarget, item.Statement.Target)) break;
                    item.Frame.TransferTarget = null;
                    if (!item.Frame.Continue) { Observe(item.Statement, ConstantControlFlowKind.LoopExit, item.Frame); break; }
                    item.Frame.Continue = false;
                }
                work.Push(new TestLoop(item.Statement, item.Frame));
                if (item.Statement is ForStatement { Increment: { } increment })
                    ScheduleFullExpression(increment, new DiscardValue());
                break;
            default: return false;
        }
        return true;
    }
}
