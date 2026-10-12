namespace RoslynObj;

public abstract partial class SemanticVisitor
{
    public virtual void VisitDeclaration(Declaration declaration) => VisitNode(declaration);
    public virtual void VisitOperator(OperatorOperation operation) => VisitOperation(operation);
    public virtual void VisitConversion(ConversionOperation operation) => VisitOperator(operation);
    public virtual void VisitConstant(ConstantOperation operation) => VisitOperation(operation);
    public virtual void VisitReference(ReferenceOperation operation) => VisitOperation(operation);
    public virtual void VisitLoweredEh(LoweredEhOperation operation) => VisitOperation(operation);
    public virtual void VisitCleanupState(CleanupStateOperation operation) => VisitLoweredEh(operation);
    public virtual void VisitBlock(BlockOperation operation) => VisitStatement(operation);
    public virtual void VisitConditional(ConditionalOperation operation) => VisitStatement(operation);
    public virtual void VisitLoop(LoopOperation operation) => VisitStatement(operation);
    public virtual void VisitJump(JumpOperation operation) => VisitStatement(operation);
    public virtual void VisitSwitch(SwitchOperation operation) => VisitStatement(operation);
    public virtual void VisitTry(TryOperation operation) => VisitStatement(operation);
    public virtual void VisitAssembly(AssemblyOperation operation) => VisitStatement(operation);
    public virtual void VisitThrow(ThrowOperation operation) => VisitOperation(operation);
    public virtual void VisitAwait(AwaitOperation operation) => VisitOperation(operation);
    public virtual void VisitAllocation(AllocationOperation operation) => VisitOperation(operation);
    public virtual void VisitLambda(LambdaOperation operation) => VisitOperation(operation);
    public virtual void VisitDeclarationStatement(DeclarationOperation operation) => VisitStatement(operation);
    public virtual void VisitRangeFor(RangeForOperation operation) => VisitStatement(operation);
    public virtual void VisitForEach(ForEachOperation operation) => VisitStatement(operation);
    public virtual void VisitMicrosoftTry(MicrosoftTryOperation operation) => VisitStatement(operation);
    public virtual void VisitLabel(LabelOperation operation) => VisitStatement(operation);
    public virtual void VisitCoroutine(CoroutineOperation operation) => VisitStatement(operation);
    public virtual void VisitVlaStatement(VlaStatementOperation operation) => VisitStatement(operation);
    public virtual void VisitTemporary(TemporaryOperation operation) => VisitOperation(operation);
    public virtual void VisitCondition(ConditionOperation operation) => VisitOperation(operation);
    public virtual void VisitLifetime(LifetimeOperation operation) => VisitOperation(operation);
    public virtual void VisitTypeQuery(TypeQueryOperation operation) => VisitOperation(operation);
    public virtual void VisitTypeOperand(TypeOperandOperation operation) => VisitOperation(operation);
    public virtual void VisitStatementExpression(StatementExpressionOperation operation) => VisitOperation(operation);
    public virtual void VisitReuse(ReuseOperation operation) => VisitOperation(operation);
    public virtual void VisitVlaExpression(VlaExpressionOperation operation) => VisitOperation(operation);
    public virtual void VisitBuiltin(BuiltinOperation operation) => VisitOperation(operation);
    public virtual void VisitParameterReference(ParameterReferenceOperation operation) => VisitOperation(operation);
    public virtual void VisitDependent(DependentOperation operation) => VisitOperation(operation);
}
