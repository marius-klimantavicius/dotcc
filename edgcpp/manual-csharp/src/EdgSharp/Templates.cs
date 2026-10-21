// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten direct-type-parameter subset of templates.c body caching,
// prototype instantiation, deduction wrapup, function partial ordering and
// concrete body rescanning; overload.c call argument deduction; lookup.c
// declaration-sequence filtering. No native or generated frontend is used.
using System.Collections.ObjectModel;

namespace EdgSharp;

public sealed class TemplateParameterDeclaration : Declaration
{
    public new TemplateParameterType Type => (TemplateParameterType)base.Type;
    internal TemplateParameterDeclaration(Symbol symbol, TemplateParameterType type, SourceRange range, SourceRange name)
        : base(symbol, type, range, name) { }
}

/// <summary>Copied ordinary lookup entities at template definition, including each overload's source-visible membership.</summary>
public sealed class DefinitionContextSnapshot
{
    public ReadOnlyDictionary<string, ReadOnlyCollection<Symbol>> Bindings { get; }
    private DefinitionContextSnapshot(Dictionary<string, ReadOnlyCollection<Symbol>> bindings) => Bindings = new(bindings);
    internal static DefinitionContextSnapshot Capture(Scope scope)
    {
        var bindings = new Dictionary<string, ReadOnlyCollection<Symbol>>(StringComparer.Ordinal);
        for (Scope? current = scope; current != null; current = current.Parent)
            foreach (NameBinding binding in current.LocalBindings)
                bindings.TryAdd(binding.Name, Array.AsReadOnly(binding.Symbols.ToArray()));
        return new DefinitionContextSnapshot(bindings);
    }
}

public enum TemplateInstanceState { NotInstantiated, Instantiating, Complete, Failed }

public sealed class FunctionTemplateInstance
{
    public FunctionTemplateDeclaration Template { get; }
    public ReadOnlyCollection<SemanticType> TypeArguments { get; }
    public Symbol Symbol { get; }
    public FunctionDeclaration? Declaration { get; internal set; }
    public TemplateInstanceState State { get; internal set; }
    public int BodyReplayCount { get; internal set; }
    internal FunctionTemplateInstance(FunctionTemplateDeclaration template, SemanticType[] arguments, FunctionType type)
    {
        (Template, TypeArguments) = (template, Array.AsReadOnly((SemanticType[])arguments.Clone()));
        Symbol = new Symbol(template.Name, SymbolKind.Function, type, template.Symbol.Scope);
    }
}

public sealed class FunctionTemplateDeclaration : Declaration
{
    private readonly List<FunctionTemplateInstance> instances = [];
    public new FunctionType Type => (FunctionType)base.Type;
    public ReadOnlyCollection<TemplateParameterDeclaration> TypeParameters { get; }
    public FunctionDeclaration Pattern { get; }
    public Scope TemplateScope { get; }
    public TokenBuffer BodyTokens { get; }
    public DefinitionContextSnapshot DefinitionContext { get; }
    public ReadOnlyCollection<FunctionTemplateInstance> Instances { get; }
    public bool IsValid { get; internal set; } = true;
    internal FunctionTemplateDeclaration(Symbol symbol, FunctionType type, SourceRange range, SourceRange name,
        TemplateParameterDeclaration[] typeParameters, Scope templateScope, FunctionDeclaration pattern,
        TokenBuffer body, DefinitionContextSnapshot context) : base(symbol, type, range, name)
    {
        (TypeParameters, TemplateScope, Pattern, BodyTokens, DefinitionContext) =
            (Array.AsReadOnly((TemplateParameterDeclaration[])typeParameters.Clone()), templateScope, pattern, body, context);
        Instances = instances.AsReadOnly();
    }
    internal void AddInstance(FunctionTemplateInstance instance) => instances.Add(instance);
    internal void RemoveLastInstance(FunctionTemplateInstance instance)
    {
        if (!ReferenceEquals(instances[^1], instance)) throw new InvalidOperationException("Instance rollback must follow creation order.");
        instances.RemoveAt(instances.Count - 1);
    }
}

/// <summary>A real operator with deferred type-dependent semantics; concrete rescanning replaces it after substitution.</summary>
public sealed class DependentUnaryExpression : Expression
{
    public TokenKind Operator { get; }
    public Expression Operand { get; }
    public override bool IsValueDependent => true;
    internal DependentUnaryExpression(TokenKind op, Expression operand, SemanticType type, SourceRange range)
        : base(type, ValueCategory.PRValue, range) => (Operator, Operand) = (op, operand);
}
public sealed class DependentBinaryExpression : Expression
{
    public TokenKind Operator { get; }
    public Expression Left { get; }
    public Expression Right { get; }
    public override bool IsValueDependent => true;
    internal DependentBinaryExpression(TokenKind op, Expression left, Expression right, SemanticType type, SourceRange range)
        : base(type, ValueCategory.PRValue, range) => (Operator, Left, Right) = (op, left, right);
}
public sealed class DependentConversionExpression : Expression
{
    public Expression Operand { get; }
    public override bool IsValueDependent => true;
    internal DependentConversionExpression(Expression operand, SemanticType target)
        : base(target, ValueCategory.PRValue, operand.Range) => Operand = operand;
}
public sealed class DependentCallExpression : Expression
{
    public Expression Callee { get; }
    public ReadOnlyCollection<Symbol> OrdinaryCandidates { get; }
    public ReadOnlyCollection<Expression> Arguments { get; }
    public override bool IsValueDependent => true;
    internal DependentCallExpression(Expression callee, Symbol[] candidates, Expression[] arguments, SemanticType type, SourceRange range)
        : base(type, ValueCategory.PRValue, range)
        => (Callee, OrdinaryCandidates, Arguments) = (callee, Array.AsReadOnly((Symbol[])candidates.Clone()), Array.AsReadOnly((Expression[])arguments.Clone()));
}

internal sealed class UnresolvedFunctionExpression(string name, SemanticType type, SourceRange range)
    : Expression(type, ValueCategory.LValue, range)
{
    internal string Name { get; } = name;
}

internal sealed partial class Parser
{
    private Symbol[]? VisibleSymbols(string name)
    {
        for (Scope? scope = state.CurrentScope; scope != null; scope = scope.Parent)
        {
            if (activeTemplate != null && ReferenceEquals(scope, activeTemplate.TemplateScope.Parent))
                return activeTemplate.DefinitionContext.Bindings.TryGetValue(name, out var captured) ? captured.ToArray() : null;
            if (scope.FindLocalBinding(name) is NameBinding binding) return binding.Symbols.ToArray();
        }
        return null;
    }
    private Symbol? SoleVisibleSymbol(string name) => VisibleSymbols(name) is { Length: 1 } symbols ? symbols[0] : null;
    private SemanticType SubstituteActiveType(SemanticType type)
        => activeTemplate != null && activeTemplateArguments != null ? SubstituteType(type, activeTemplateArguments) : type;
    private SemanticType SubstituteType(SemanticType type, IReadOnlyList<SemanticType> arguments)
    {
        if (type is not TemplateParameterType parameter) return type;
        SemanticType result = arguments[parameter.Index];
        return parameter.Qualifiers.HasFlag(TypeQualifiers.Const) ? types.Const(result) : result;
    }
    private static bool TypeDependent(Expression expression) => expression.Type.Kind is TypeKind.TemplateParameter or TypeKind.Dependent;

    private FunctionTemplateDeclaration TemplateDeclaration()
    {
        int diagnosticStart = state.Diagnostics.ErrorCount;
        Token start = Take();
        Expect(TokenKind.Less, "'<' after template");
        Scope enclosing = state.CurrentScope;
        Scope templateScope = state.PushScope(ScopeKind.Template);
        var typeParameters = new List<TemplateParameterDeclaration>();
        FunctionType functionType;
        VariableDeclaration[] parameters;
        Scope parameterScope;
        Token identifier;
        TokenBuffer body;
        bool isConstexpr;
        try
        {
            do
            {
                Token parameterStart = Current;
                if (!Eat("class") && !Eat("typename")) UnsupportedAndThrow("Only named template type parameters are implemented.");
                if (Is(TokenKind.Ellipsis)) UnsupportedAndThrow("Template parameter packs are not implemented.");
                if (!Is(TokenKind.Identifier)) UnsupportedAndThrow("Unnamed template type parameters are not implemented.");
                Token name = Expect(TokenKind.Identifier, "a template type parameter name");
                var type = new TemplateParameterType(name.IdentifierText, typeParameters.Count, templateScope);
                Symbol symbol = NewLocalSymbol(name, SymbolKind.TemplateParameter, type);
                var parameter = new TemplateParameterDeclaration(symbol, type, SourceRange.Cover(parameterStart.Range, name.Range), name.Range);
                state.AddDeclaration(parameter);
                typeParameters.Add(parameter);
                if (Is(TokenKind.Equal) || Is(TokenKind.Ellipsis)) UnsupportedAndThrow("Template defaults and parameter packs are not implemented.");
            } while (Eat(TokenKind.Comma));
            Expect(TokenKind.Greater, "'>' after template parameters");
            isConstexpr = Eat("constexpr");
            SemanticType result = ScanType();
            if (isConstexpr && result.Kind == TypeKind.Void)
                Unsupported("Constexpr void functions are outside the implemented integer evaluator.", start.Range);
            if (Is(TokenKind.Star) || Is(TokenKind.Ampersand) || Is(TokenKind.AndAnd))
                UnsupportedAndThrow("Template pointer and reference declarators are not implemented.");
            identifier = Expect(TokenKind.Identifier, "a function template name");
            if (!Is(TokenKind.LeftParen)) UnsupportedAndThrow("Only free function templates are implemented.");
            (parameterScope, parameters) = FunctionParameters();
            functionType = types.Function(result, parameters.Select(p => p.Type));
            if (!Is(TokenKind.LeftBrace)) UnsupportedAndThrow("Function template declarations without a body and alternate definition forms are not implemented.");
            int bodyStart = state.Tokens.Index;
            int braces = 0;
            do
            {
                Token token = Take();
                if (token.Kind == TokenKind.LeftBrace) ++braces;
                else if (token.Kind == TokenKind.RightBrace) --braces;
                if (Is(TokenKind.EndOfSource) && braces != 0)
                {
                    Fail("TMP002", "Missing closing brace in function template body.", start.Range);
                    throw new ParseFailure();
                }
            } while (braces != 0);
            body = state.Tokens.Retain(bodyStart, state.Tokens.Index - bodyStart);
        }
        finally { state.PopScope(); }
        if (typeParameters.Any(parameter => parameter.Name == identifier.IdentifierText))
            Fail("TMP001", "A function template name cannot redeclare its own template parameter.", identifier.Range);
        if (identifier.IdentifierText == "main") Unsupported("A global function template named main is outside the implemented main rules.", identifier.Range);
        Symbol entity = NewLocalSymbol(identifier, SymbolKind.FunctionTemplate, functionType);
        var pattern = new FunctionDeclaration(entity, functionType, SourceRange.Cover(start.Range, body.Tokens[^2].Range), identifier.Range, parameters, parameterScope)
            { IsConstexpr = isConstexpr };
        var declaration = new FunctionTemplateDeclaration(entity, functionType, pattern.Range, identifier.Range,
            typeParameters.ToArray(), templateScope, pattern, body, DefinitionContextSnapshot.Capture(enclosing));
        if (enclosing.FindLocalBinding(entity.Name)?.Symbols.Any(s => !ReferenceEquals(s, entity) && s.TemplateDefinition is FunctionTemplateDeclaration previous && EquivalentTemplate(previous, declaration)) == true)
            Unsupported("Function template redeclarations with equivalent direct signatures are not implemented.", identifier.Range);
        state.SetTemplateDefinition(entity, declaration);
        state.AddDeclaration(declaration);
        var replayState = new ParseState(body.CreateCursor(), state.Diagnostics, state.GlobalScope, state);
        replayState.ActivateRetainedScope(templateScope);
        replayState.CurrentFunction = pattern;
        replayState.PendingReplays.Add(body);
        var parser = new Parser(replayState, types, options) { activeTemplate = declaration, prototypeAnalysis = true, instantiationDepth = instantiationDepth };
        try { pattern.Body = parser.CompoundStatement(parameterScope); }
        finally { replayState.PendingReplays.Clear(); }
        declaration.IsValid = state.Diagnostics.ErrorCount == diagnosticStart;
        return declaration;
    }
    private static bool EquivalentTemplate(FunctionTemplateDeclaration left, FunctionTemplateDeclaration right)
    {
        if (left.TypeParameters.Count != right.TypeParameters.Count || left.Type.ParameterTypes.Count != right.Type.ParameterTypes.Count) return false;
        static bool Same(SemanticType a, SemanticType b) => a is TemplateParameterType x && b is TemplateParameterType y
            ? x.Index == y.Index && x.Qualifiers == y.Qualifiers : ReferenceEquals(a, b);
        return Same(left.Type.ReturnType, right.Type.ReturnType) && left.Type.ParameterTypes.Zip(right.Type.ParameterTypes).All(pair => Same(pair.First, pair.Second));
    }
    private FunctionTemplateInstance? InstantiateCandidate(OverloadCandidate candidate)
    {
        FunctionTemplateDeclaration template = candidate.Template!;
        FunctionTemplateInstance? instance = template.Instances.FirstOrDefault(i => i.TypeArguments.SequenceEqual(candidate.TemplateArguments));
        if (instance == null)
        {
            instance = new FunctionTemplateInstance(template, candidate.TemplateArguments.ToArray(), candidate.FunctionType);
            state.RegisterInstance(template, instance);
        }
        if (unevaluatedParsing || prototypeAnalysis && (!constantRequiredParsing || !template.Pattern.IsConstexpr) ||
            instance.State is TemplateInstanceState.Complete or TemplateInstanceState.Instantiating) return instance;
        if (instance.State == TemplateInstanceState.Failed) return null;
        if (instantiationDepth >= options.TemplateInstantiationDepthLimit)
        {
            Fail("TMP003", "Template instantiation depth limit exceeded.", template.NameRange);
            state.UpdateInstance(instance, TemplateInstanceState.Failed);
            return null;
        }
        state.UpdateInstance(instance, TemplateInstanceState.Instantiating);
        var replay = new ParseState(template.BodyTokens.CreateCursor(), state.Diagnostics, state.GlobalScope, state);
        replay.ActivateRetainedScope(template.TemplateScope);
        Scope scope = replay.PushScope(ScopeKind.Function);
        var parameters = new List<VariableDeclaration>();
        for (int i = 0; i < template.Pattern.Parameters.Count; ++i)
        {
            VariableDeclaration pattern = template.Pattern.Parameters[i];
            SemanticType type = SubstituteType(pattern.Type, candidate.TemplateArguments);
            var symbol = new Symbol(pattern.Name, SymbolKind.Parameter, type, scope);
            if (pattern.Name.Length != 0) replay.Enter(symbol);
            var parameter = new VariableDeclaration(symbol, type, pattern.Range, pattern.NameRange);
            replay.AddDeclaration(parameter);
            parameters.Add(parameter);
        }
        replay.PopScope();
        var definition = new FunctionDeclaration(instance.Symbol, candidate.FunctionType, template.Range, template.NameRange, parameters.ToArray(), scope)
            { IsConstexpr = template.Pattern.IsConstexpr, TemplateInstance = instance };
        replay.UpdateInstance(instance, TemplateInstanceState.Instantiating, definition, replay: true);
        replay.AddInstanceDeclaration(definition);
        replay.SetDefinition(instance.Symbol, definition);
        replay.CurrentFunction = definition;
        replay.PendingReplays.Add(template.BodyTokens);
        int before = state.Diagnostics.ErrorCount;
        var parser = new Parser(replay, types, options) { activeTemplate = template, activeTemplateArguments = candidate.TemplateArguments.ToArray(), instantiationDepth = instantiationDepth + 1 };
        try
        {
            definition.Body = parser.CompoundStatement(scope);
            replay.UpdateInstance(instance, state.Diagnostics.ErrorCount == before ? TemplateInstanceState.Complete : TemplateInstanceState.Failed);
        }
        catch (ParseFailure) { replay.UpdateInstance(instance, TemplateInstanceState.Failed); }
        finally { replay.PendingReplays.Clear(); }
        return instance.State == TemplateInstanceState.Complete ? instance : null;
    }
}
