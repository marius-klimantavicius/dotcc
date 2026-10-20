// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored managed representations following EDG il.h, scope_stk.c and declaration identity contracts.
using System.Collections.ObjectModel;
using System.Numerics;

namespace EdgSharp;

public enum ScopeKind { TranslationUnit, Function, Block }
public enum SymbolKind { TypeAlias, Variable, Parameter, Function }

public sealed class Scope
{
    private readonly Dictionary<string, Symbol> symbols = new(StringComparer.Ordinal);
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
    /// <summary>Inspects retained lexical scope facts without activating a scope or instantiating anything.</summary>
    public Symbol? FindLocal(string name) => symbols.GetValueOrDefault(name);
    public Symbol? Lookup(string name) => FindLocal(name) ?? Parent?.Lookup(name);
    internal void Enter(Symbol symbol) => symbols.Add(symbol.Name, symbol);
    internal void Remove(string name) => symbols.Remove(name);
    internal void Add(Declaration declaration) => declarations.Add(declaration);
    internal void RemoveLastDeclaration() => declarations.RemoveAt(declarations.Count - 1);
    internal void AddChild(Scope child) => children.Add(child);
    internal void RemoveLastChild() => children.RemoveAt(children.Count - 1);
}

/// <summary>One declared entity. Redeclarations share this object and retain separate declaration nodes.</summary>
public sealed class Symbol
{
    private readonly List<Declaration> declarations = [];
    public string Name { get; }
    public SymbolKind Kind { get; }
    public SemanticType Type { get; }
    public Scope Scope { get; }
    public ReadOnlyCollection<Declaration> Declarations { get; }
    public FunctionDeclaration? Definition { get; internal set; }
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
    public SemanticType Type { get; }
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
    public bool IsParameter => Symbol.Kind == SymbolKind.Parameter;
    internal VariableDeclaration(Symbol symbol, SemanticType type, SourceRange range, SourceRange name) : base(symbol, type, range, name) { }
}
public sealed class FunctionDeclaration : Declaration
{
    public new FunctionType Type => (FunctionType)base.Type;
    public ReadOnlyCollection<VariableDeclaration> Parameters { get; }
    public Scope ParameterScope { get; }
    public BlockStatement? Body { get; internal set; }
    public bool IsDefinition => Body != null;
    internal FunctionDeclaration(Symbol symbol, FunctionType type, SourceRange range, SourceRange name, VariableDeclaration[] parameters, Scope parameterScope)
        : base(symbol, type, range, name)
        => (Parameters, ParameterScope) = (Array.AsReadOnly((VariableDeclaration[])parameters.Clone()), parameterScope);
}

public enum ValueCategory { LValue, PRValue }
public abstract class Expression
{
    public SemanticType Type { get; }
    public ValueCategory ValueCategory { get; }
    public SourceRange Range { get; }
    public BigInteger? ConstantValue { get; }
    internal Expression(SemanticType type, ValueCategory category, SourceRange range, BigInteger? constant = null)
        => (Type, ValueCategory, Range, ConstantValue) = (type, category, range, constant);
}
public sealed class ErrorExpression : Expression
{
    internal ErrorExpression(SemanticType type, SourceRange range) : base(type, ValueCategory.PRValue, range) { }
}
public sealed class LiteralExpression : Expression
{
    internal LiteralExpression(IntegerType type, SourceRange range, BigInteger value) : base(type, ValueCategory.PRValue, range, value) { }
}
public sealed class ReferenceExpression : Expression
{
    public Symbol ReferencedSymbol { get; }
    internal ReferenceExpression(Symbol symbol, SourceRange range)
        : base(symbol.Type, ValueCategory.LValue, range) => ReferencedSymbol = symbol;
}
public sealed class ParenthesizedExpression : Expression
{
    public Expression Operand { get; }
    internal ParenthesizedExpression(Expression operand, SourceRange range)
        : base(operand.Type, operand.ValueCategory, range, operand.ConstantValue) => Operand = operand;
}
public enum ConversionKind { LValueToRValue, IntegralPromotion, IntegralConversion, BooleanConversion }
public sealed class ConversionExpression : Expression
{
    public Expression Operand { get; }
    public ConversionKind Conversion { get; }
    internal ConversionExpression(Expression operand, SemanticType target, ConversionKind conversion, BigInteger? constant)
        : base(target, ValueCategory.PRValue, operand.Range, constant) => (Operand, Conversion) = (operand, conversion);
}
public sealed class UnaryExpression : Expression
{
    public TokenKind Operator { get; }
    public Expression Operand { get; }
    internal UnaryExpression(TokenKind op, Expression operand, SemanticType type, SourceRange range, BigInteger? constant)
        : base(type, ValueCategory.PRValue, range, constant) => (Operator, Operand) = (op, operand);
}
public sealed class BinaryExpression : Expression
{
    public TokenKind Operator { get; }
    public Expression Left { get; }
    public Expression Right { get; }
    internal BinaryExpression(TokenKind op, Expression left, Expression right, SemanticType type, SourceRange range, BigInteger? constant)
        : base(type, ValueCategory.PRValue, range, constant) => (Operator, Left, Right) = (op, left, right);
}
public sealed class CallExpression : Expression
{
    public Symbol SelectedFunction { get; }
    public Expression Callee { get; }
    public ReadOnlyCollection<Expression> Arguments { get; }
    internal CallExpression(Symbol function, Expression callee, Expression[] arguments, SemanticType resultType, SourceRange range)
        : base(resultType, ValueCategory.PRValue, range)
        => (SelectedFunction, Callee, Arguments) = (function, callee, Array.AsReadOnly((Expression[])arguments.Clone()));
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
