// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten scalar reference semantics following types.c reference relation,
// il.c reference canonicalization/collapsing, declarator.c pointer_declarator,
// overload.c direct_reference_binding_possible/prep_reference_initializer_operand
// and decl_inits.c check_for_missing_initializer_full. No class/lifetime engine
// or template-reference deduction is implied by these bounded scalar operations.
namespace EdgSharp;

public enum ReferenceKind { LValue, RValue }

/// <summary>Declared reference identity; expressions have the referred-to type instead.</summary>
public sealed class ReferenceType : SemanticType
{
    public SemanticType ReferredType { get; }
    public ReferenceKind ReferenceKind { get; }
    internal ReferenceType(SemanticType referredType, ReferenceKind kind)
        : base(TypeKind.Reference, referredType.DisplayName + (kind == ReferenceKind.LValue ? " &" : " &&"))
        => (ReferredType, ReferenceKind) = (referredType, kind);
}

internal sealed partial class TypeTable
{
    private readonly Dictionary<(SemanticType, ReferenceKind), ReferenceType> references = [];

    // Compound types may hide a TU-owned template parameter arbitrarily deep.
    // Neither successful nor failed/unsupported analyses may add those graphs
    // to session-lived intern tables. Extend this walk with every compound type.
    internal static bool ContainsScopedOrDependentType(SemanticType type) => type switch
    {
        TemplateParameterType => true,
        ReferenceType reference => ContainsScopedOrDependentType(reference.ReferredType),
        FunctionType function => ContainsScopedOrDependentType(function.ReturnType) ||
            function.ParameterTypes.Any(ContainsScopedOrDependentType),
        _ => type.Kind == TypeKind.Dependent
    };

    internal ReferenceType Reference(SemanticType referredType, ReferenceKind kind)
    {
        // The parser separately rejects explicit repeated reference operators.
        // A reference obtained through an alias is composed by collapsing.
        if (referredType is ReferenceType previous)
        {
            if (kind == ReferenceKind.RValue || previous.ReferenceKind == ReferenceKind.LValue) return previous;
            referredType = previous.ReferredType;
        }
        if (referredType.Kind is TypeKind.Void or TypeKind.Error)
            throw new ArgumentException("A reference requires a non-void referred-to type.", nameof(referredType));
        if (ContainsScopedOrDependentType(referredType)) return new ReferenceType(referredType, kind);
        var key = (referredType, kind);
        if (!references.TryGetValue(key, out ReferenceType? result))
            references.Add(key, result = new ReferenceType(referredType, kind));
        return result;
    }
    internal int InternedReferenceTypeCount => references.Count;
}

public enum ReferenceBindingKind { Direct, MaterializedTemporary, ConvertedTemporary }
public enum ReferenceBindingContext { Variable, Argument, Return }
public enum ReferenceTemporaryLifetime { ReferenceVariable, FullExpression }

/// <summary>A bounded storage-origin fact, not a proof that a reference remains alive.</summary>
public enum ReferenceReferentOrigin { Unknown, StaticObject, AutomaticObject, CallerReference, Temporary }

/// <summary>
/// Viable scalar reference conversion before committing initialization. Direct
/// binding never reads the referred object. Temporary conversion records the
/// ordinary integral rank, independently from reference-kind/cv tie breakers.
/// </summary>
public sealed class ReferenceBinding
{
    public ReferenceType ReferenceType { get; }
    public IntegerType SourceType { get; }
    public IntegerType TargetType => (IntegerType)ReferenceType.ReferredType;
    public ValueCategory SourceCategory { get; }
    public ReferenceBindingKind Kind { get; }
    public StandardConversionRank Rank { get; }
    public bool AddsQualification { get; }
    public bool RequiresTemporary => Kind != ReferenceBindingKind.Direct;
    public bool IsDirectBinding => Kind != ReferenceBindingKind.ConvertedTemporary;
    public bool RequiresLValueToRValue => Kind == ReferenceBindingKind.ConvertedTemporary && SourceCategory != ValueCategory.PRValue;
    internal ReferenceBinding(ReferenceType type, IntegerType source, ValueCategory category, ReferenceBindingKind kind,
        StandardConversionRank rank, bool addsQualification)
        => (ReferenceType, SourceType, SourceCategory, Kind, Rank, AddsQualification) =
            (type, source, category, kind, rank, addsQualification);
}

/// <summary>
/// Identity of a materialized scalar temporary and its initializer. Lifetime
/// describes only this binding context; cleanup/destruction execution is future
/// object-model work. Binding another reference does not extend this lifetime.
/// </summary>
public sealed class MaterializedTemporaryExpression : Expression
{
    public Expression Initializer { get; }
    public ReferenceTemporaryLifetime Lifetime { get; }
    public Symbol? LifetimeOwner { get; }
    public override bool IsValueDependent => Initializer.IsValueDependent;
    internal MaterializedTemporaryExpression(Expression initializer, SemanticType type,
        ReferenceTemporaryLifetime lifetime, Symbol? owner)
        : base(type, ValueCategory.XValue, initializer.Range)
        => (Initializer, Lifetime, LifetimeOwner) = (initializer, lifetime, owner);
}

/// <summary>
/// An implicit binding operation. Type/category describe the bound referent;
/// Binding.ReferenceType retains the separate declared reference type. A
/// binding supplies no folded integer value, including when its source does.
/// </summary>
public sealed class ReferenceBindingExpression : Expression
{
    public Expression Source { get; }
    public Expression Referent { get; }
    public ReferenceBinding Binding { get; }
    public ReferenceBindingContext Context { get; }
    public MaterializedTemporaryExpression? Temporary => Referent as MaterializedTemporaryExpression;
    /// <summary>A known temporary reached through named references, without re-extending its lifetime.</summary>
    public MaterializedTemporaryExpression? KnownTemporary { get; }
    public ReferenceReferentOrigin ReferentOrigin { get; }
    public override bool IsValueDependent => Source.IsValueDependent;
    internal ReferenceBindingExpression(Expression source, Expression referent, ReferenceBinding binding,
        ReferenceBindingContext context)
        : base(binding.TargetType, binding.ReferenceType.ReferenceKind == ReferenceKind.LValue ? ValueCategory.LValue : ValueCategory.XValue, source.Range)
    {
        (Source, Referent, Binding, Context) = (source, referent, binding, context);
        (ReferentOrigin, KnownTemporary) = FindOrigin(referent);
    }
    private static (ReferenceReferentOrigin Origin, MaterializedTemporaryExpression? Temporary) FindOrigin(Expression expression)
    {
        while (expression is ParenthesizedExpression parenthesized) expression = parenthesized.Operand;
        if (expression is MaterializedTemporaryExpression temporary) return (ReferenceReferentOrigin.Temporary, temporary);
        if (expression is not ReferenceExpression reference) return (ReferenceReferentOrigin.Unknown, null);
        Symbol symbol = reference.ReferencedSymbol;
        if (symbol.Type is ReferenceType)
        {
            if (symbol.Kind == SymbolKind.Parameter) return (ReferenceReferentOrigin.CallerReference, null);
            // Earlier completed bindings already contain immutable origin and
            // temporary identity facts. Reuse them without recursively walking
            // an ever-growing alias chain. A self-reference has no completed
            // initializer yet and remains Unknown.
            return symbol.Declarations.OfType<VariableDeclaration>().FirstOrDefault()?.ReferenceBinding is { } binding
                ? (binding.ReferentOrigin, binding.KnownTemporary) : (ReferenceReferentOrigin.Unknown, null);
        }
        return (symbol.Scope.Kind == ScopeKind.TranslationUnit ? ReferenceReferentOrigin.StaticObject : ReferenceReferentOrigin.AutomaticObject, null);
    }
}

internal sealed partial class Parser
{
    private bool InTemplateScope()
    {
        for (Scope? scope = state.CurrentScope; scope != null; scope = scope.Parent)
            if (scope.Kind == ScopeKind.Template) return true;
        return false;
    }

    // Part of declarator.c's pointer-declarator walk; pointers, arrays and
    // parenthesized declarators still have their existing Unsupported boundary.
    private SemanticType ScanReferenceDeclarator(SemanticType type)
    {
        if (!Is(TokenKind.Ampersand) && !Is(TokenKind.AndAnd))
        {
            if (type is ReferenceType && InTemplateScope())
                UnsupportedAndThrow("Reference declarators in templates are not implemented.");
            return type;
        }
        Token reference = Take();
        if (InTemplateScope() || TypeTable.ContainsScopedOrDependentType(type))
            UnsupportedAndThrow("Dependent and template reference declarators are not implemented.");
        if (Is("const"))
        {
            Fail("REF001", "A reference declarator cannot have cv-qualifiers after '&' or '&&'.", Current.Range);
            while (Eat("const")) { }
        }
        if (Is(TokenKind.Ampersand) || Is(TokenKind.AndAnd))
        {
            Fail("REF002", "Explicit reference-to-reference declarator operators are invalid; alias composition uses reference collapsing.", Current.Range);
            while (Is(TokenKind.Ampersand) || Is(TokenKind.AndAnd)) Take();
            return types.Error;
        }
        if (type.Kind == TypeKind.Void)
        {
            Fail("REF003", "A reference cannot refer to void.", reference.Range);
            return types.Error;
        }
        if (type.Kind == TypeKind.Error) return type;
        return types.Reference(type, reference.Kind == TokenKind.Ampersand ? ReferenceKind.LValue : ReferenceKind.RValue);
    }

    private ReferenceBinding? MatchReferenceBinding(Expression source, ReferenceType target)
    {
        if (source.Type is not IntegerType integer || target.ReferredType is not IntegerType referred) return null;
        bool related = integer.IntegerKind == referred.IntegerKind;
        bool isConst = referred.Qualifiers.HasFlag(TypeQualifiers.Const);
        if (related)
        {
            // Cv cannot be discarded by falling back to a copied temporary
            // when the source and target are reference-related.
            if ((integer.Qualifiers & ~referred.Qualifiers) != 0) return null;
            if (target.ReferenceKind == ReferenceKind.RValue && source.ValueCategory == ValueCategory.LValue) return null;
            if (target.ReferenceKind == ReferenceKind.LValue && !isConst && source.ValueCategory != ValueCategory.LValue) return null;
            return new ReferenceBinding(target, integer, source.ValueCategory,
                source.ValueCategory == ValueCategory.PRValue ? ReferenceBindingKind.MaterializedTemporary : ReferenceBindingKind.Direct,
                StandardConversionRank.ExactMatch, (referred.Qualifiers & ~integer.Qualifiers) != 0);
        }
        // Unrelated integer types may copy-convert into a temporary for const
        // lvalue or rvalue references, including an lvalue source for T&&.
        if (target.ReferenceKind == ReferenceKind.LValue && !isConst) return null;
        return new ReferenceBinding(target, integer, source.ValueCategory, ReferenceBindingKind.ConvertedTemporary,
            IntegerConversionRank(integer, referred), false);
    }

    private Expression BindReference(Expression source, ReferenceType target, ReferenceBindingContext context,
        Symbol? owner = null, ReferenceBinding? selectedBinding = null)
    {
        if (source.Type.Kind == TypeKind.Error) return source;
        if (source.IsTypeDependent || TypeTable.ContainsScopedOrDependentType(target))
        {
            Unsupported("Dependent reference initialization is not implemented.", source.Range);
            return new ErrorExpression(types.Error, source.Range);
        }
        if (source.Type.Kind == TypeKind.Void)
        {
            Fail("REF005", "A reference cannot bind to a void expression.", source.Range);
            return new ErrorExpression(types.Error, source.Range);
        }
        if (source.Type is not IntegerType || target.ReferredType is not IntegerType)
        {
            Unsupported("Only scalar integer reference initialization is implemented.", source.Range);
            return new ErrorExpression(types.Error, source.Range);
        }
        ReferenceBinding? binding = selectedBinding ?? MatchReferenceBinding(source, target);
        if (binding == null)
        {
            Fail("REF005", $"Cannot bind '{target}' to an expression of type '{source.Type}' and category '{source.ValueCategory}'.", source.Range);
            return new ErrorExpression(types.Error, source.Range);
        }
        Expression referent = source;
        if (binding.RequiresTemporary)
        {
            Expression initializer = binding.Kind == ReferenceBindingKind.ConvertedTemporary ? Convert(source, target.ReferredType) : source;
            referent = new MaterializedTemporaryExpression(initializer, target.ReferredType,
                context == ReferenceBindingContext.Variable ? ReferenceTemporaryLifetime.ReferenceVariable : ReferenceTemporaryLifetime.FullExpression, owner);
        }
        // Origin is informational. Returning a local/temporary reference is a
        // lifetime hazard, not an invented invalid-reference-binding error.
        var result = new ReferenceBindingExpression(source, referent, binding, context);
        bool returningLocalTemporary = result.KnownTemporary is { Lifetime: ReferenceTemporaryLifetime.ReferenceVariable, LifetimeOwner: { } lifetimeOwner } &&
            lifetimeOwner.Scope.Kind != ScopeKind.TranslationUnit;
        if (context == ReferenceBindingContext.Return &&
            (result.Temporary != null || returningLocalTemporary || result.ReferentOrigin == ReferenceReferentOrigin.AutomaticObject))
            state.Diagnostics.Add("REF006", result.Temporary != null
                ? "The returned reference binds to a temporary whose lifetime ends with the return expression."
                : returningLocalTemporary ? "The returned reference refers to a temporary whose local lifetime owner leaves scope on return."
                : "The returned reference refers to an automatic object.", source.Range,
                severity: DiagnosticSeverity.Warning);
        return result;
    }
}
