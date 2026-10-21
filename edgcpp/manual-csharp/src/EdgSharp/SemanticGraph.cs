// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored managed representations following EDG il.h, scope_stk.c and declaration identity contracts.
using System.Collections.ObjectModel;
using System.Numerics;

namespace EdgSharp;

public enum ScopeKind { TranslationUnit, Function, Block, Template }
public enum SymbolKind { TypeAlias, Variable, Parameter, Function, TemplateParameter, FunctionTemplate }

/// <summary>
/// One name in one lexical scope. Function entities retain their first-declaration
/// order; redeclarations add declarations to the existing entity, not this list.
/// </summary>
public sealed class NameBinding
{
    private readonly List<Symbol> symbols = [];
    public string Name { get; }
    public Scope Scope { get; }
    public ReadOnlyCollection<Symbol> Symbols { get; }
    /// <summary>Null when this name denotes more than one function entity.</summary>
    public Symbol? SingleSymbol => symbols.Count == 1 ? symbols[0] : null;
    internal NameBinding(Symbol symbol)
    {
        (Name, Scope) = (symbol.Name, symbol.Scope);
        Symbols = symbols.AsReadOnly();
        symbols.Add(symbol);
    }
    internal void Add(Symbol symbol)
    {
        if (!IsFunction(symbol) || symbols.Any(s => !IsFunction(s)) ||
            !ReferenceEquals(symbol.Scope, Scope) || symbol.Name != Name)
            throw new InvalidOperationException("Only function entities can share a name binding.");
        symbols.Add(symbol);
    }
    internal static bool IsFunction(Symbol symbol) => symbol.Kind is SymbolKind.Function or SymbolKind.FunctionTemplate;
    internal void RemoveLast(Symbol symbol)
    {
        if (!ReferenceEquals(symbols[^1], symbol)) throw new InvalidOperationException("Symbol rollback must follow insertion order.");
        symbols.RemoveAt(symbols.Count - 1);
    }
}

public sealed class Scope
{
    private readonly Dictionary<string, NameBinding> bindings = new(StringComparer.Ordinal);
    private readonly List<Declaration> declarations = [];
    private readonly List<Scope> children = [];
    public ScopeKind Kind { get; }
    public Scope? Parent { get; }
    public ReadOnlyCollection<Declaration> Declarations { get; }
    public ReadOnlyCollection<Scope> Children { get; }
    internal Scope(ScopeKind kind, Scope? parent)
    {
        (Kind, Parent) = (kind, parent);
        Declarations = declarations.AsReadOnly();
        Children = children.AsReadOnly();
    }
    /// <summary>Inspects the retained ordered binding, without activating a scope or instantiating anything.</summary>
    public NameBinding? FindLocalBinding(string name) => bindings.GetValueOrDefault(UnicodeIdentifiers.Identity(name));
    /// <summary>The nearest binding hides every outer binding with the same name.</summary>
    public NameBinding? LookupBinding(string name) => FindLocalBinding(name) ?? Parent?.LookupBinding(name);
    /// <summary>Returns only a sole local entity; null for an absent or overloaded name. Use FindLocalBinding to distinguish them.</summary>
    public Symbol? FindLocal(string name) => FindLocalBinding(name)?.SingleSymbol;
    /// <summary>Returns only a sole entity in the nearest binding; an overload never falls through to an outer scope.</summary>
    public Symbol? Lookup(string name) => LookupBinding(name)?.SingleSymbol;
    internal void Enter(Symbol symbol)
    {
        if (FindLocalBinding(symbol.Name) is NameBinding existing) existing.Add(symbol);
        else bindings.Add(symbol.Name, new NameBinding(symbol));
    }
    internal void Remove(Symbol symbol)
    {
        NameBinding binding = bindings[symbol.Name];
        binding.RemoveLast(symbol);
        if (binding.Symbols.Count == 0) bindings.Remove(symbol.Name);
    }
    internal void Add(Declaration declaration) => declarations.Add(declaration);
    internal void RemoveLastDeclaration() => declarations.RemoveAt(declarations.Count - 1);
    internal void AddChild(Scope child) => children.Add(child);
    internal void RemoveLastChild() => children.RemoveAt(children.Count - 1);
    internal IEnumerable<NameBinding> LocalBindings => bindings.Values;
}

/// <summary>One declared entity. Redeclarations share this object and retain separate declaration nodes.</summary>
public sealed class Symbol
{
    private readonly List<Declaration> declarations = [];
    /// <summary>Canonical scalar identifier identity; original spelling is retained in each declaration's NameRange.</summary>
    public string Name { get; }
    public SymbolKind Kind { get; }
    public SemanticType Type { get; internal set; }
    public Scope Scope { get; }
    public ReadOnlyCollection<Declaration> Declarations { get; }
    public FunctionDeclaration? Definition { get; internal set; }
    public FunctionTemplateDeclaration? TemplateDefinition { get; internal set; }
    public bool HasTemplateDependentValue { get; internal set; }
    internal Symbol(string name, SymbolKind kind, SemanticType type, Scope scope)
    {
        (Name, Kind, Type, Scope) = (name, kind, type, scope);
        Declarations = declarations.AsReadOnly();
    }
    internal void Add(Declaration declaration) => declarations.Add(declaration);
    internal void RemoveLastDeclaration() => declarations.RemoveAt(declarations.Count - 1);
}

public abstract class Declaration
{
    public Symbol Symbol { get; }
    public string Name => Symbol.Name;
    public SemanticType Type { get; internal set; }
    public SourceRange Range { get; internal set; }
    public SourceRange NameRange { get; }
    internal Declaration(Symbol symbol, SemanticType type, SourceRange range, SourceRange nameRange)
        => (Symbol, Type, Range, NameRange) = (symbol, type, range, nameRange);
}

public sealed class AliasDeclaration : Declaration
{
    public SemanticType AliasedType => Type;
    internal AliasDeclaration(Symbol symbol, SemanticType type, SourceRange range, SourceRange name) : base(symbol, type, range, name) { }
}
public sealed class VariableDeclaration : Declaration
{
    public Expression? Initializer { get; internal set; }
    public ReferenceBindingExpression? ReferenceBinding => Initializer as ReferenceBindingExpression;
    public bool IsParameter => Symbol.Kind == SymbolKind.Parameter;
    public SemanticType? ParameterDeclaredType { get; internal set; }
    public bool IsConstexpr { get; internal set; }
    public ConstantEvaluationResult? ConstantEvaluation { get; internal set; }
    internal VariableDeclaration(Symbol symbol, SemanticType type, SourceRange range, SourceRange name) : base(symbol, type, range, name) { }
}
public sealed class FunctionDeclaration : Declaration
{
    public new FunctionType Type => (FunctionType)base.Type;
    public ReadOnlyCollection<VariableDeclaration> Parameters { get; }
    public Scope ParameterScope { get; }
    public BlockStatement? Body { get; internal set; }
    public bool IsDefinition => Body != null;
    public bool IsConstexpr { get; internal set; }
    public FunctionTemplateInstance? TemplateInstance { get; internal set; }
    internal FunctionDeclaration(Symbol symbol, FunctionType type, SourceRange range, SourceRange name, VariableDeclaration[] parameters, Scope parameterScope)
        : base(symbol, type, range, name)
        => (Parameters, ParameterScope) = (Array.AsReadOnly((VariableDeclaration[])parameters.Clone()), parameterScope);
}

public enum ValueCategory { LValue, PRValue, XValue }
public abstract class Expression
{
    public SemanticType Type { get; }
    public ValueCategory ValueCategory { get; }
    public SourceRange Range { get; }
    public BigInteger? ConstantValue { get; }
    public bool IsTypeDependent => Type.Kind is TypeKind.TemplateParameter or TypeKind.Dependent;
    public virtual bool IsValueDependent => IsTypeDependent;
    internal Expression(SemanticType type, ValueCategory category, SourceRange range, BigInteger? constant = null)
        => (Type, ValueCategory, Range, ConstantValue) = (type, category, range, constant);
}
public sealed class ErrorExpression : Expression
{
    internal ErrorExpression(SemanticType type, SourceRange range) : base(type, ValueCategory.PRValue, range) { }
}
public class LiteralExpression : Expression
{
    internal LiteralExpression(IntegerType type, SourceRange range, BigInteger value) : base(type, ValueCategory.PRValue, range, value) { }
}
public sealed class ReferenceExpression : Expression
{
    public Symbol ReferencedSymbol { get; }
    public override bool IsValueDependent => IsTypeDependent || ReferencedSymbol.HasTemplateDependentValue;
    internal ReferenceExpression(Symbol symbol, SourceRange range)
        : base(symbol.Type is ReferenceType reference ? reference.ReferredType : symbol.Type, ValueCategory.LValue, range) => ReferencedSymbol = symbol;
}
/// <summary>
/// A snapshot of the function entities visible at this source reference. Its
/// candidate list cannot gain overloads from declarations that occur later.
/// No single function type is assigned before overload resolution.
/// </summary>
public sealed class OverloadSetExpression : Expression
{
    public ReadOnlyCollection<Symbol> Functions { get; }
    internal OverloadSetExpression(SemanticType type, Symbol[] functions, SourceRange range)
        : base(type, ValueCategory.LValue, range) => Functions = Array.AsReadOnly((Symbol[])functions.Clone());
}
public sealed class ParenthesizedExpression : Expression
{
    public Expression Operand { get; }
    public override bool IsValueDependent => Operand.IsValueDependent;
    internal ParenthesizedExpression(Expression operand, SourceRange range)
        : base(operand.Type, operand.ValueCategory, range, operand.ConstantValue) => Operand = operand;
}
public enum ConversionKind { LValueToRValue, IntegralPromotion, IntegralConversion, BooleanConversion, ArrayToPointer, QualificationConversion, NullPointerConversion }
public sealed class ConversionExpression : Expression
{
    public Expression Operand { get; }
    public ConversionKind Conversion { get; }
    public override bool IsValueDependent => Operand.IsValueDependent;
    internal ConversionExpression(Expression operand, SemanticType target, ConversionKind conversion, BigInteger? constant)
        : base(target, ValueCategory.PRValue, operand.Range, constant) => (Operand, Conversion) = (operand, conversion);
}
public sealed class UnaryExpression : Expression
{
    public TokenKind Operator { get; }
    public Expression Operand { get; }
    public override bool IsValueDependent => Operand.IsValueDependent;
    internal UnaryExpression(TokenKind op, Expression operand, SemanticType type, SourceRange range, BigInteger? constant)
        : base(type, ValueCategory.PRValue, range, constant) => (Operator, Operand) = (op, operand);
}
public sealed class BinaryExpression : Expression
{
    public TokenKind Operator { get; }
    public Expression Left { get; }
    public Expression Right { get; }
    public override bool IsValueDependent => Left.IsValueDependent || Right.IsValueDependent;
    internal BinaryExpression(TokenKind op, Expression left, Expression right, SemanticType type, SourceRange range, BigInteger? constant)
        : base(type, ValueCategory.PRValue, range, constant) => (Operator, Left, Right) = (op, left, right);
}
public sealed class CallExpression : Expression
{
    public Symbol SelectedFunction { get; }
    public SemanticType DeclaredReturnType => SelectedCandidate.FunctionType.ReturnType;
    public Expression Callee { get; }
    public ReadOnlyCollection<Expression> Arguments { get; }
    public ReadOnlyCollection<OverloadCandidate> Candidates { get; }
    public OverloadCandidate SelectedCandidate { get; }
    public FunctionTemplateInstance? TemplateInstance { get; }
    public override bool IsValueDependent => Arguments.Any(argument => argument.IsValueDependent);
    internal CallExpression(Symbol function, FunctionTemplateInstance? instance, OverloadCandidate selected, OverloadCandidate[] candidates, Expression callee, Expression[] arguments, SemanticType resultType, SourceRange range)
        : base(resultType is ReferenceType reference ? reference.ReferredType : resultType,
            resultType is ReferenceType r ? r.ReferenceKind == ReferenceKind.LValue ? ValueCategory.LValue : ValueCategory.XValue : ValueCategory.PRValue, range)
        => (SelectedFunction, TemplateInstance, SelectedCandidate, Candidates, Callee, Arguments) =
            (function, instance, selected, Array.AsReadOnly((OverloadCandidate[])candidates.Clone()), callee, Array.AsReadOnly((Expression[])arguments.Clone()));
}

public sealed class StaticAssertion
{
    public Expression Expression { get; }
    public ConstantEvaluationResult Evaluation { get; }
    public SourceRange Range { get; }
    public bool IsDeferred { get; }
    internal StaticAssertion(Expression expression, ConstantEvaluationResult evaluation, SourceRange range, bool deferred)
        => (Expression, Evaluation, Range, IsDeferred) = (expression, evaluation, range, deferred);
}

public sealed class StaticAssertStatement : Statement
{
    public StaticAssertion Assertion { get; }
    internal StaticAssertStatement(StaticAssertion assertion) : base(assertion.Range) => Assertion = assertion;
}

public abstract class Statement
{
    public SourceRange Range { get; }
    internal Statement(SourceRange range) => Range = range;
}
public sealed class BlockStatement : Statement
{
    public Scope Scope { get; }
    public ReadOnlyCollection<Statement> Statements { get; }
    internal BlockStatement(Scope scope, Statement[] statements, SourceRange range) : base(range)
        => (Scope, Statements) = (scope, Array.AsReadOnly((Statement[])statements.Clone()));
}
public sealed class DeclarationStatement : Statement
{
    public Declaration Declaration { get; }
    internal DeclarationStatement(Declaration declaration) : base(declaration.Range) => Declaration = declaration;
}
public sealed class ReturnStatement : Statement
{
    public Expression? Expression { get; }
    internal ReturnStatement(Expression? expression, SourceRange range) : base(range) => Expression = expression;
}
public sealed class ExpressionStatement : Statement
{
    public Expression Expression { get; }
    internal ExpressionStatement(Expression expression, SourceRange range) : base(range) => Expression = expression;
}
public sealed class EmptyStatement : Statement { internal EmptyStatement(SourceRange range) : base(range) { } }
public sealed class ErrorStatement : Statement { internal ErrorStatement(SourceRange range) : base(range) { } }
