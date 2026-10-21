// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten bounded interpreter following interpret.c's explicit work stack,
// per-invocation call/local storage and invalid/cost-limit outcomes, with target
// integer arithmetic. It evaluates the typed graph; optional parser folds are
// not a constant-expression classification or a substitute for interpretation.
using System.Numerics;

namespace EdgSharp;

public enum ConstantEvaluationStatus { Success, Dependent, NotConstant, InvalidExpression, UninitializedRead, UndefinedBehavior, ResourceLimit }

public sealed class ConstantEvaluationResult
{
    public ConstantEvaluationStatus Status { get; }
    public BigInteger? Value { get; }
    public int Steps { get; }
    public int MaximumCallDepth { get; }
    public string Message { get; }
    internal ConstantEvaluationResult(ConstantEvaluationStatus status, BigInteger? value, int steps, int depth, string message)
        => (Status, Value, Steps, MaximumCallDepth, Message) = (status, value, steps, depth, message);
}

internal sealed class ConstantInterpreter(AnalysisOptions options)
{
    private abstract record Work;
    private sealed record EvaluateExpression(Expression Expression) : Work;
    private sealed record ConvertValue(ConversionExpression Expression) : Work;
    private sealed record UnaryValue(UnaryExpression Expression) : Work;
    private sealed record BinaryValue(BinaryExpression Expression) : Work;
    private sealed record EnterCall(CallExpression Expression) : Work;
    private sealed record LeaveCall(CallFrame Frame) : Work;
    private sealed record ExecuteBlock(BlockStatement Block, CallFrame Frame) : Work;
    private sealed record LeaveBlock(BlockStatement Block, CallFrame Frame) : Work;
    private sealed record ExecuteStatement(Statement Statement, CallFrame Frame) : Work;
    private sealed record StoreLocal(Symbol Symbol, CallFrame Frame) : Work;
    private sealed record ReturnValue(CallFrame Frame) : Work;
    private sealed record DiscardValue : Work;
    private sealed class CallFrame(FunctionDeclaration function)
    {
        internal FunctionDeclaration Function { get; } = function;
        internal Dictionary<Symbol, BigInteger?> Locals { get; } = [];
        internal bool Returned { get; set; }
        internal BigInteger Result { get; set; }
    }
    private sealed class EvaluationStopped(ConstantEvaluationStatus status, string message) : Exception(message)
    {
        internal ConstantEvaluationStatus Status { get; } = status;
    }
    private readonly Stack<Work> work = [];
    private readonly Stack<BigInteger> values = [];
    private readonly Stack<CallFrame> calls = [];
    private int steps, maximumDepth;
    private static void Stop(ConstantEvaluationStatus status, string message) => throw new EvaluationStopped(status, message);

    internal ConstantEvaluationResult Evaluate(Expression expression)
    {
        steps = maximumDepth = 0;
        work.Push(new EvaluateExpression(expression));
        try
        {
            while (work.Count != 0)
            {
                if (++steps > options.ConstantEvaluationStepLimit) Stop(ConstantEvaluationStatus.ResourceLimit, "Constant evaluation step limit exceeded.");
                switch (work.Pop())
                {
                    case EvaluateExpression item: EvaluateNode(item.Expression); break;
                    case ConvertValue item:
                        values.Push(TypeTable.ConvertInteger(values.Pop(), (IntegerType)item.Expression.Type));
                        break;
                    case UnaryValue item: EvaluateUnary(item.Expression); break;
                    case BinaryValue item: EvaluateBinary(item.Expression); break;
                    case EnterCall item: StartCall(item.Expression); break;
                    case LeaveCall item:
                        if (!item.Frame.Returned) Stop(ConstantEvaluationStatus.NotConstant, "A constexpr call reaches the end without returning a value.");
                        if (!ReferenceEquals(calls.Pop(), item.Frame)) throw new InvalidOperationException("Interpreter call stack is inconsistent.");
                        values.Push(item.Frame.Result);
                        break;
                    case ExecuteBlock item:
                        if (item.Frame.Returned) break;
                        work.Push(new LeaveBlock(item.Block, item.Frame));
                        for (int i = item.Block.Statements.Count - 1; i >= 0; --i)
                            work.Push(new ExecuteStatement(item.Block.Statements[i], item.Frame));
                        break;
                    case LeaveBlock item:
                        foreach (VariableDeclaration local in item.Block.Scope.Declarations.OfType<VariableDeclaration>())
                            if (!local.IsParameter) item.Frame.Locals.Remove(local.Symbol);
                        break;
                    case ExecuteStatement item:
                        if (!item.Frame.Returned) EvaluateStatement(item.Statement, item.Frame);
                        break;
                    case StoreLocal item: item.Frame.Locals[item.Symbol] = values.Pop(); break;
                    case ReturnValue item:
                        item.Frame.Result = values.Pop();
                        item.Frame.Returned = true;
                        break;
                    case DiscardValue: values.Pop(); break;
                }
            }
            if (values.Count != 1 || calls.Count != 0) throw new InvalidOperationException("Interpreter did not finish with one value.");
            return new(ConstantEvaluationStatus.Success, values.Pop(), steps, maximumDepth, "");
        }
        catch (EvaluationStopped stopped)
        {
            return new(stopped.Status, null, steps, maximumDepth, stopped.Message);
        }
        finally { work.Clear(); values.Clear(); calls.Clear(); }
    }
    private void EvaluateNode(Expression expression)
    {
        switch (expression)
        {
            case LiteralExpression literal: values.Push(literal.ConstantValue!.Value); break;
            case ParenthesizedExpression parens: work.Push(new EvaluateExpression(parens.Operand)); break;
            case ReferenceExpression reference:
                if (reference.IsValueDependent)
                    Stop(ConstantEvaluationStatus.Dependent, "A template-dependent value requires substitution.");
                if (calls.TryPeek(out CallFrame? frame) && frame.Locals.TryGetValue(reference.ReferencedSymbol, out BigInteger? local))
                {
                    if (local == null) Stop(ConstantEvaluationStatus.UninitializedRead, "Read of an uninitialized automatic object.");
                    values.Push(local!.Value);
                }
                else if (reference.ReferencedSymbol.Declarations.FirstOrDefault() is VariableDeclaration variable &&
                    variable.Type.Qualifiers.HasFlag(TypeQualifiers.Const) &&
                    variable.ConstantEvaluation is { Status: ConstantEvaluationStatus.Success, Value: BigInteger constant })
                    values.Push(constant);
                else Stop(ConstantEvaluationStatus.NotConstant, "The referenced object has no usable constant value in this evaluation.");
                break;
            case ConversionExpression conversion:
                if (conversion.Type is not IntegerType) Stop(ConstantEvaluationStatus.InvalidExpression, "Only integer value conversions are interpreted.");
                work.Push(new ConvertValue(conversion));
                work.Push(new EvaluateExpression(conversion.Operand));
                break;
            case UnaryExpression unary:
                work.Push(new UnaryValue(unary)); work.Push(new EvaluateExpression(unary.Operand)); break;
            case BinaryExpression binary:
                work.Push(new BinaryValue(binary));
                work.Push(new EvaluateExpression(binary.Right)); work.Push(new EvaluateExpression(binary.Left));
                break;
            case CallExpression call:
                work.Push(new EnterCall(call));
                for (int i = call.Arguments.Count - 1; i >= 0; --i) work.Push(new EvaluateExpression(call.Arguments[i]));
                break;
            case DependentUnaryExpression or DependentBinaryExpression or DependentConversionExpression or DependentCallExpression:
                Stop(ConstantEvaluationStatus.Dependent, "A template-dependent expression requires substitution."); break;
            case ErrorExpression: Stop(ConstantEvaluationStatus.InvalidExpression, "Cannot interpret an erroneous expression."); break;
            default: Stop(ConstantEvaluationStatus.NotConstant, "This expression has no implemented constant value."); break;
        }
    }
    private void StartCall(CallExpression call)
    {
        FunctionDeclaration? function = call.SelectedFunction.Definition;
        if (function == null || function.Body == null || !function.IsConstexpr || function.Type.ReturnType is not IntegerType)
            Stop(ConstantEvaluationStatus.NotConstant, "Constant evaluation requires a visible constexpr integer function definition.");
        if (calls.Count >= options.ConstantEvaluationCallLimit) Stop(ConstantEvaluationStatus.ResourceLimit, "Constant evaluation call depth limit exceeded.");
        var frame = new CallFrame(function!);
        for (int i = function!.Parameters.Count - 1; i >= 0; --i) frame.Locals.Add(function.Parameters[i].Symbol, values.Pop());
        calls.Push(frame);
        maximumDepth = Math.Max(maximumDepth, calls.Count);
        work.Push(new LeaveCall(frame));
        work.Push(new ExecuteBlock(function.Body!, frame));
    }
    private void EvaluateStatement(Statement statement, CallFrame frame)
    {
        switch (statement)
        {
            case BlockStatement block: work.Push(new ExecuteBlock(block, frame)); break;
            case DeclarationStatement { Declaration: VariableDeclaration variable }:
                frame.Locals[variable.Symbol] = null;
                if (variable.Initializer != null)
                {
                    work.Push(new StoreLocal(variable.Symbol, frame));
                    work.Push(new EvaluateExpression(variable.Initializer));
                }
                break;
            case DeclarationStatement or EmptyStatement or StaticAssertStatement: break;
            case ReturnStatement { Expression: Expression expression }:
                work.Push(new ReturnValue(frame)); work.Push(new EvaluateExpression(expression)); break;
            case ExpressionStatement expression:
                // In this subset the only glvalues are object/function
                // references and parentheses around them. Discarding such a
                // nonvolatile glvalue performs no lvalue-to-rvalue conversion.
                if (expression.Expression.ValueCategory == ValueCategory.LValue) break;
                work.Push(new DiscardValue()); work.Push(new EvaluateExpression(expression.Expression)); break;
            default: Stop(ConstantEvaluationStatus.InvalidExpression, "Unsupported or erroneous statement in constant evaluation."); break;
        }
    }
    private void PushArithmetic(BigInteger value, IntegerType type)
    {
        if (type.IsSigned && (value < type.Minimum || value > type.Maximum))
            Stop(ConstantEvaluationStatus.UndefinedBehavior, "Signed integer overflow in constant evaluation.");
        values.Push(TypeTable.ConvertInteger(value, type));
    }
    private void EvaluateUnary(UnaryExpression expression)
    {
        BigInteger operand = values.Pop();
        BigInteger result = expression.Operator switch
        {
            TokenKind.Plus => operand, TokenKind.Minus => -operand,
            TokenKind.Tilde => ~operand, TokenKind.Bang => operand.IsZero ? BigInteger.One : BigInteger.Zero,
            _ => throw new InvalidOperationException("Unexpected unary operator in the interpreter.")
        };
        PushArithmetic(result, (IntegerType)expression.Type);
    }
    private void EvaluateBinary(BinaryExpression expression)
    {
        BigInteger right = values.Pop(), left = values.Pop();
        var operationType = (IntegerType)expression.Left.Type;
        if ((expression.Operator is TokenKind.Slash or TokenKind.Percent) &&
            (right.IsZero || operationType.IsSigned && left == operationType.Minimum && right == -1))
            Stop(ConstantEvaluationStatus.UndefinedBehavior, "Invalid integer division in constant evaluation.");
        BigInteger result = expression.Operator switch
        {
            TokenKind.Plus => left + right, TokenKind.Minus => left - right, TokenKind.Star => left * right,
            TokenKind.Slash => left / right, TokenKind.Percent => left % right,
            TokenKind.Ampersand => left & right, TokenKind.Pipe => left | right, TokenKind.Caret => left ^ right,
            TokenKind.EqualEqual => left == right ? BigInteger.One : BigInteger.Zero,
            TokenKind.NotEqual => left != right ? BigInteger.One : BigInteger.Zero,
            TokenKind.Less => left < right ? BigInteger.One : BigInteger.Zero,
            TokenKind.Greater => left > right ? BigInteger.One : BigInteger.Zero,
            TokenKind.LessEqual => left <= right ? BigInteger.One : BigInteger.Zero,
            TokenKind.GreaterEqual => left >= right ? BigInteger.One : BigInteger.Zero,
            _ => throw new InvalidOperationException("Unexpected binary operator in the interpreter.")
        };
        PushArithmetic(result, (IntegerType)expression.Type);
    }
}

internal sealed partial class Parser
{
    private bool constantRequiredParsing;
    private void EvaluateVariableInitializer(VariableDeclaration declaration)
    {
        if (!declaration.Type.Qualifiers.HasFlag(TypeQualifiers.Const) || declaration.Initializer == null) return;
        state.SetDependentValue(declaration.Symbol, prototypeAnalysis && declaration.Initializer.IsValueDependent);
        ConstantEvaluationResult evaluation = new ConstantInterpreter(options).Evaluate(declaration.Initializer);
        declaration.ConstantEvaluation = evaluation;
        if (declaration.IsConstexpr && evaluation.Status != ConstantEvaluationStatus.Success &&
            !(prototypeAnalysis && evaluation.Status == ConstantEvaluationStatus.Dependent))
            Fail("CEX001", "A constexpr initializer is not a constant expression: " + evaluation.Message, declaration.NameRange);
    }
    private StaticAssertion StaticAssertion()
    {
        Token start = Take();
        Expect(TokenKind.LeftParen, "'(' after static_assert");
        bool saved = constantRequiredParsing;
        Expression expression;
        constantRequiredParsing = true;
        try { expression = Convert(ScanExprFull(), types.Integer(IntegerKind.Bool)); }
        finally { constantRequiredParsing = saved; }
        if (Is(TokenKind.Comma)) UnsupportedAndThrow("Static assertion message literals are not implemented.");
        Expect(TokenKind.RightParen, "')' after static assertion");
        Token end = Expect(TokenKind.Semicolon, "';' after static assertion");
        ConstantEvaluationResult evaluation = new ConstantInterpreter(options).Evaluate(expression);
        bool deferred = prototypeAnalysis && (evaluation.Status == ConstantEvaluationStatus.Dependent ||
            evaluation.Status == ConstantEvaluationStatus.Success && evaluation.Value == 0);
        if (!deferred)
        {
            if (evaluation.Status != ConstantEvaluationStatus.Success)
                Fail("CEX001", "Static assertion requires a constant expression: " + evaluation.Message, expression.Range);
            else if (evaluation.Value == 0) Fail("CEX002", "Static assertion failed.", expression.Range);
        }
        var assertion = new StaticAssertion(expression, evaluation, SourceRange.Cover(start.Range, end.Range), deferred);
        state.AddAssertion(assertion);
        return assertion;
    }
}
