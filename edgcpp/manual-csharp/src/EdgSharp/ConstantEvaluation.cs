// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten bounded interpreter following interpret.c's explicit work stack,
// per-invocation call/local storage and invalid/cost-limit outcomes, with target
// integer arithmetic. It evaluates the typed graph; optional parser folds are
// not a constant-expression classification or a substitute for interpretation.
using System.Numerics;

namespace EdgSharp;

public enum ConstantEvaluationStatus { Success, Dependent, NotConstant, InvalidExpression, UninitializedRead, UndefinedBehavior, ResourceLimit, ExpiredObject }

public sealed class ConstantEvaluationResult
{
    public ConstantEvaluationStatus Status { get; }
    public BigInteger? Value { get; }
    public ConstantReferenceValue? ReferenceValue { get; }
    public ConstantArrayObject? ArrayValue { get; }
    public ConstantPointerValue? PointerValue { get; }
    public ConstantPointerValue? NonObjectReferenceValue { get; }
    public System.Collections.ObjectModel.ReadOnlyCollection<ConstantNonObjectReferenceBinding> NonObjectReferenceBindings { get; }
    public System.Collections.ObjectModel.ReadOnlyCollection<ConstantArrayObject> Arrays { get; }
    public System.Collections.ObjectModel.ReadOnlyCollection<ConstantStorageObject> StorageObjects { get; }
    public System.Collections.ObjectModel.ReadOnlyCollection<ConstantReferenceBinding> ReferenceBindings { get; }
    public int Steps { get; }
    public int MaximumCallDepth { get; }
    public string Message { get; }
    internal ConstantEvaluationResult(ConstantEvaluationStatus status, BigInteger? value, int steps, int depth, string message,
        ConstantReferenceValue? reference = null, ConstantStorageObject[]? storage = null, ConstantReferenceBinding[]? bindings = null,
        ConstantArrayObject[]? arrays = null, ConstantArrayObject? array = null, ConstantPointerValue? pointer = null,
        ConstantPointerValue? nonObjectReference = null, ConstantNonObjectReferenceBinding[]? nonObjectBindings = null)
        => (Status, Value, Steps, MaximumCallDepth, Message, ReferenceValue, StorageObjects, ReferenceBindings, Arrays, ArrayValue, PointerValue, NonObjectReferenceValue, NonObjectReferenceBindings) =
            (status, value, steps, depth, message, reference, Array.AsReadOnly(storage ?? []), Array.AsReadOnly(bindings ?? []), Array.AsReadOnly(arrays ?? []), array, pointer, nonObjectReference, Array.AsReadOnly(nonObjectBindings ?? []));
}

internal sealed partial class ConstantInterpreter(AnalysisOptions options)
{
    private abstract record Work;
    private sealed record EvaluateExpression(Expression Expression) : Work;
    private sealed record ConvertValue(ConversionExpression Expression) : Work;
    private sealed record UnaryValue(UnaryExpression Expression) : Work;
    private sealed record BinaryValue(BinaryExpression Expression) : Work;
    private sealed record LogicalLeft(BinaryExpression Expression) : Work;
    private sealed record LogicalRight : Work;
    private sealed record EnterCall(CallExpression Expression) : Work;
    private sealed record LeaveCall(CallFrame Frame) : Work;
    private sealed record ExecuteBlock(BlockStatement Block, CallFrame Frame) : Work;
    private sealed record LeaveBlock(BlockStatement Block, CallFrame Frame) : Work;
    private sealed record ExecuteStatement(Statement Statement, CallFrame Frame) : Work;
    private sealed record StoreLocal(Symbol Symbol, CallFrame Frame) : Work;
    private sealed record CompleteMaterialization(MaterializedTemporaryExpression Expression) : Work;
    private sealed record CompleteBinding(ReferenceBindingExpression Expression) : Work;
    private sealed record CompleteGlobalReference(ConstantSlot Slot) : Work;
    private sealed record CompleteGlobalScalar(ConstantCell Cell) : Work;
    private sealed record ResumeLoad(ConversionExpression Expression, ConstantCell Cell) : Work;
    private sealed record BeginFullExpression : Work;
    private sealed record EndFullExpression : Work;
    private sealed record ReturnValue(CallFrame Frame) : Work;
    private sealed record DiscardValue : Work;
    private sealed class CallFrame(FunctionDeclaration function, int identity)
    {
        internal FunctionDeclaration Function { get; } = function;
        internal int Identity { get; } = identity;
        internal Dictionary<Symbol, ConstantSlot> Locals { get; } = [];
        internal List<ConstantCell> Owned { get; } = [];
        internal Dictionary<Symbol, List<ConstantCell>> LifetimeOwners { get; } = [];
        internal bool Returned { get; set; }
        internal ConstantRuntimeValue Result { get; set; }
    }
    private sealed class EvaluationStopped(ConstantEvaluationStatus status, string message) : Exception(message)
    {
        internal ConstantEvaluationStatus Status { get; } = status;
    }
    private readonly Stack<Work> work = [];
    private readonly Stack<ConstantRuntimeValue> values = [];
    private readonly Stack<CallFrame> calls = [];
    private int steps, maximumDepth;
    private static void Stop(ConstantEvaluationStatus status, string message) => throw new EvaluationStopped(status, message);

    internal ConstantEvaluationResult Evaluate(Expression expression)
    {
        steps = maximumDepth = invocationSequence = 0;
        work.Push(new EndFullExpression());
        work.Push(new EvaluateExpression(expression));
        work.Push(new BeginFullExpression());
        try
        {
            while (work.Count != 0)
            {
                if (++steps > options.ConstantEvaluationStepLimit) Stop(ConstantEvaluationStatus.ResourceLimit, "Constant evaluation step limit exceeded.");
                switch (work.Pop())
                {
                    case EvaluateExpression item: EvaluateNode(item.Expression); break;
                    case ConvertValue item:
                        if (EvaluateAggregateConversion(item.Expression)) break;
                        ConstantRuntimeValue converted = values.Pop();
                        if (item.Expression.Conversion == ConversionKind.LValueToRValue && converted.Location is { } pending && !pending.Initialized && pending.DeferredInitializer is { } declaration)
                        {
                            EnsureAlive(pending);
                            if (pending.Initializing) Stop(ConstantEvaluationStatus.UninitializedRead, "The initializer attempts to read the object being initialized.");
                            pending.Initializing = true;
                            work.Push(new ResumeLoad(item.Expression, pending));
                            ScheduleFullExpression(declaration.Initializer!, new CompleteGlobalScalar(pending));
                            break;
                        }
                        BigInteger integer = item.Expression.Conversion == ConversionKind.LValueToRValue ? Load(converted) : Scalar(converted);
                        values.Push(ConstantRuntimeValue.Scalar(TypeTable.ConvertInteger(integer, (IntegerType)item.Expression.Type)));
                        break;
                    case UnaryValue item: EvaluateUnary(item.Expression); break;
                    case BinaryValue item: EvaluateBinary(item.Expression); break;
                    case LogicalLeft item:
                        bool truth = !Scalar(values.Pop()).IsZero;
                        if (item.Expression.Operator == TokenKind.AndAnd ? !truth : truth)
                            values.Push(ConstantRuntimeValue.Scalar(truth ? BigInteger.One : BigInteger.Zero));
                        else { work.Push(new LogicalRight()); work.Push(new EvaluateExpression(item.Expression.Right)); }
                        break;
                    case LogicalRight:
                        values.Push(ConstantRuntimeValue.Scalar(Scalar(values.Pop()).IsZero ? BigInteger.Zero : BigInteger.One));
                        break;
                    case EnterCall item: StartCall(item.Expression); break;
                    case LeaveCall item:
                        if (!item.Frame.Returned) Stop(ConstantEvaluationStatus.NotConstant, "A constexpr call reaches the end without returning a value.");
                        if (!ReferenceEquals(calls.Pop(), item.Frame)) throw new InvalidOperationException("Interpreter call stack is inconsistent.");
                        foreach (ConstantCell cell in item.Frame.Owned) cell.Alive = false;
                        aggregateLocals.Remove(item.Frame);
                        if (item.Frame.Function.Type.ReturnType is ReferenceType && item.Frame.Result.NonObjectLocation != null)
                            Stop(ConstantEvaluationStatus.UndefinedBehavior, "Using this reference-returning call requires an object at its nonobject array location.");
                        values.Push(item.Frame.Result);
                        break;
                    case ExecuteBlock item:
                        if (item.Frame.Returned) break;
                        work.Push(new LeaveBlock(item.Block, item.Frame));
                        for (int i = item.Block.Statements.Count - 1; i >= 0; --i)
                            work.Push(new ExecuteStatement(item.Block.Statements[i], item.Frame));
                        break;
                    case LeaveBlock item:
                        LeaveAggregateScope(item.Block.Scope, item.Frame);
                        foreach (VariableDeclaration local in item.Block.Scope.Declarations.OfType<VariableDeclaration>())
                            if (!local.IsParameter)
                            {
                                if (item.Frame.Locals.Remove(local.Symbol, out ConstantSlot? slot) && slot.Object != null) slot.Object.Alive = false;
                                if (item.Frame.LifetimeOwners.Remove(local.Symbol, out List<ConstantCell>? owned))
                                    foreach (ConstantCell cell in owned) cell.Alive = false;
                            }
                        break;
                    case ExecuteStatement item:
                        if (!item.Frame.Returned) EvaluateStatement(item.Statement, item.Frame);
                        break;
                    case StoreLocal item: Store(item.Frame.Locals[item.Symbol], values.Pop()); break;
                    case CompleteMaterialization item: Materialize(item.Expression); break;
                    case CompleteBinding item:
                        ConstantRuntimeValue bound = values.Pop();
                        if (bound.NonObjectLocation is { } nonobject)
                        {
                            nonObjectBindings.Add((item.Expression, nonobject, calls.TryPeek(out CallFrame? markerFrame) ? markerFrame.Identity : 0));
                            values.Push(bound); break;
                        }
                        ConstantCell target = Location(bound);
                        EnsureAlive(target);
                        bindingObservations.Add((item.Expression, target, calls.TryPeek(out CallFrame? current) ? current.Identity : 0));
                        values.Push(ConstantRuntimeValue.Address(target));
                        break;
                    case CompleteGlobalReference item:
                        if (values.Peek().NonObjectLocation != null)
                            Stop(ConstantEvaluationStatus.UndefinedBehavior, "Using this reference requires an object at its nonobject array location.");
                        ConstantCell global = Location(values.Pop());
                        item.Slot.Referent = global; item.Slot.Initializing = false;
                        values.Push(ConstantRuntimeValue.Address(global));
                        break;
                    case CompleteGlobalScalar item:
                        item.Cell.Value = Scalar(values.Pop()); item.Cell.Initialized = true; item.Cell.Initializing = false;
                        break;
                    case ResumeLoad item:
                        values.Push(ConstantRuntimeValue.Scalar(TypeTable.ConvertInteger(Load(ConstantRuntimeValue.Address(item.Cell)), (IntegerType)item.Expression.Type)));
                        break;
                    case BeginFullExpression: fullExpressions.Push([]); break;
                    case EndFullExpression:
                        foreach (ConstantCell temporary in fullExpressions.Pop()) temporary.Alive = false;
                        break;
                    case ReturnValue item:
                        item.Frame.Result = values.Pop();
                        item.Frame.Returned = true;
                        break;
                    case DiscardValue: values.Pop(); break;
                    case FinishSubscript item: Subscript(item.Expression); break;
                    case FinishPointer item: FinishPointerValue(item); break;
                    case FinishPointerArithmetic item: PointerArithmetic(item.Expression); break;
                }
            }
            if (values.Count != 1 || calls.Count != 0) throw new InvalidOperationException("Interpreter did not finish with one value.");
            ConstantRuntimeValue result = values.Pop();
            if (result.NonObjectLocation?.Array is { } nonobjectArray && nonobjectArray.Elements.Any(cell => !cell.Alive))
                Stop(ConstantEvaluationStatus.ExpiredObject, "The designated array's lifetime has ended.");
            if (result.Array is { } array && array.Elements.Any(cell => !cell.Alive) ||
                result.Pointer?.Array is { } pointed && pointed.Elements.Any(cell => !cell.Alive))
                Stop(ConstantEvaluationStatus.ExpiredObject, "The designated array's lifetime has ended.");
            if (result.Location is { } location)
            {
                EnsureAlive(location);
                if (!location.Known) Stop(ConstantEvaluationStatus.NotConstant, "An unknown runtime referent is not a persistent reference constant.");
                if (location.Lifetime != ConstantStorageLifetime.Static)
                    Stop(ConstantEvaluationStatus.NotConstant, "A reference constant cannot designate automatic storage outside this evaluation.");
            }
            return Result(ConstantEvaluationStatus.Success, result, "");
        }
        catch (EvaluationStopped stopped)
        {
            foreach (ConstantCell cell in storage) if (cell.Lifetime != ConstantStorageLifetime.Static) cell.Alive = false;
            return Result(stopped.Status, default, stopped.Message);
        }
        finally { work.Clear(); values.Clear(); calls.Clear(); globals.Clear(); storage.Clear(); bindingObservations.Clear(); fullExpressions.Clear(); ClearAggregateState(); }
    }
    private void EvaluateNode(Expression expression)
    {
        switch (expression)
        {
            case StringLiteralExpression stringLiteral: EvaluateString(stringLiteral); break;
            case ArrayInitializationExpression initialization:
                values.Push(ConstantRuntimeValue.ArrayAddress(MakeArray((ArrayType)initialization.Type, null, null, initialization.Literal.CodeUnits, null, true))); break;
            case SizeOfExpression size: values.Push(ConstantRuntimeValue.Scalar(size.ByteSize)); break;
            case DependentSizeOfExpression: Stop(ConstantEvaluationStatus.Dependent, "Dependent sizeof requires substitution."); break;
            case SubscriptExpression subscript:
                work.Push(new FinishSubscript(subscript)); work.Push(new EvaluateExpression(subscript.SourceRight)); work.Push(new EvaluateExpression(subscript.SourceLeft)); break;
            case PointerArithmeticExpression pointer:
                work.Push(new FinishPointerArithmetic(pointer)); work.Push(new EvaluateExpression(pointer.Offset)); work.Push(new EvaluateExpression(pointer.Pointer)); break;
            case LiteralExpression literal: values.Push(ConstantRuntimeValue.Scalar(literal.ConstantValue!.Value)); break;
            case ParenthesizedExpression parens: work.Push(new EvaluateExpression(parens.Operand)); break;
            case ReferenceExpression reference:
                if (reference.IsValueDependent)
                    Stop(ConstantEvaluationStatus.Dependent, "A template-dependent value requires substitution.");
                ResolveReference(reference);
                break;
            case ReferenceBindingExpression binding:
                work.Push(new CompleteBinding(binding)); work.Push(new EvaluateExpression(binding.Referent)); break;
            case MaterializedTemporaryExpression temporary:
                work.Push(new CompleteMaterialization(temporary)); work.Push(new EvaluateExpression(temporary.Initializer)); break;
            case ConversionExpression conversion:
                if (conversion.Type is not (IntegerType or PointerType)) Stop(ConstantEvaluationStatus.InvalidExpression, "This conversion is not interpreted.");
                work.Push(new ConvertValue(conversion));
                work.Push(new EvaluateExpression(conversion.Operand));
                break;
            case UnaryExpression unary:
                work.Push(new UnaryValue(unary)); work.Push(new EvaluateExpression(unary.Operand)); break;
            case BinaryExpression binary:
                if (binary.Operator is TokenKind.AndAnd or TokenKind.OrOr)
                { work.Push(new LogicalLeft(binary)); work.Push(new EvaluateExpression(binary.Left)); break; }
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
        if (function == null || function.Body == null || !function.IsConstexpr || function.Type.ReturnType is not (IntegerType or ReferenceType or PointerType))
            Stop(ConstantEvaluationStatus.NotConstant, "Constant evaluation requires a visible constexpr scalar function definition.");
        if (calls.Count >= options.ConstantEvaluationCallLimit) Stop(ConstantEvaluationStatus.ResourceLimit, "Constant evaluation call depth limit exceeded.");
        var frame = new CallFrame(function!, ++invocationSequence);
        for (int i = function!.Parameters.Count - 1; i >= 0; --i)
        {
            VariableDeclaration parameter = function.Parameters[i];
            ConstantRuntimeValue argument = values.Pop();
            if (BindAggregateParameter(parameter, argument, frame)) continue;
            var slot = new ConstantSlot { Object = parameter.Type is IntegerType type ? Allocate(type, parameter.Symbol, parameter.Symbol, ConstantStorageLifetime.Automatic, true, frame) : null };
            Store(slot, argument); frame.Locals.Add(parameter.Symbol, slot);
        }
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
                if (DeclareAggregate(variable, frame)) break;
                frame.Locals[variable.Symbol] = new ConstantSlot { Object = variable.Type is IntegerType type ? Allocate(type, variable.Symbol, variable.Symbol, ConstantStorageLifetime.Automatic, true, frame) : null };
                if (variable.Initializer != null)
                {
                    ScheduleFullExpression(variable.Initializer, new StoreLocal(variable.Symbol, frame));
                }
                break;
            case DeclarationStatement or EmptyStatement or StaticAssertStatement: break;
            case ReturnStatement { Expression: Expression expression }:
                ScheduleFullExpression(expression, new ReturnValue(frame)); break;
            case ExpressionStatement expression:
                Expression discarded = expression.Expression;
                while (discarded is ParenthesizedExpression parenthesized) discarded = parenthesized.Operand;
                if (discarded is ReferenceExpression reference && reference.ReferencedSymbol.Type is not ReferenceType) break;
                ScheduleFullExpression(expression.Expression, new DiscardValue()); break;
            default: Stop(ConstantEvaluationStatus.InvalidExpression, "Unsupported or erroneous statement in constant evaluation."); break;
        }
    }
    private void PushArithmetic(BigInteger value, IntegerType type)
    {
        if (type.IsSigned && (value < type.Minimum || value > type.Maximum))
            Stop(ConstantEvaluationStatus.UndefinedBehavior, "Signed integer overflow in constant evaluation.");
        values.Push(ConstantRuntimeValue.Scalar(TypeTable.ConvertInteger(value, type)));
    }
    private void EvaluateUnary(UnaryExpression expression)
    {
        BigInteger operand = Scalar(values.Pop());
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
        BigInteger right = Scalar(values.Pop()), left = Scalar(values.Pop());
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
        if ((!declaration.Type.Qualifiers.HasFlag(TypeQualifiers.Const) && !declaration.IsConstexpr && declaration.Type is not ReferenceType) || declaration.Initializer == null) return;
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
