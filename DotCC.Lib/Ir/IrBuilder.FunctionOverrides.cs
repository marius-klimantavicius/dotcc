using System;
using System.Collections.Generic;
using System.Linq;
using LALR.CC.LexicalGrammar;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private CPreprocessingOptions? _functionOverrideOptions;
    private readonly Dictionary<Symbol, (FunctionOverride Rule, string Signature, string TranslationUnit, string? DeclarationFile)> _functionReplacements = new();
    private readonly Dictionary<FunctionOverride, HashSet<Symbol>> _functionOverrideMatches = new();
    internal string TranslationUnitPath { get; set; } = "";
    private string FunctionUnitIdentity => TranslationUnitPath.Length == 0 ? _file : TranslationUnitPath;

    internal void ConfigureFunctionOverrides(CPreprocessingOptions? options) => _functionOverrideOptions = options;

    private void BindFunctionOverride(Symbol symbol, FnSig signature, Item declaration)
    {
        if (_functionOverrideOptions is not { FunctionOverrides.Count: > 0 } options) return;
        var actual = new CType.Func(signature.Return, signature.Params.Select(p => p.Type).ToArray(), signature.Variadic);
        var key = OverrideTypeKey(actual);
        if (_functionReplacements.TryGetValue(symbol, out var previous) && previous.Signature != key)
            throw CPreprocessingOptions.FunctionError(previous.Rule, "incompatible redeclaration signature: " + key);
        if (previous.Rule is not null) ValidateFunctionAttributes(previous.Rule, symbol);
        foreach (var rule in options.FunctionOverrides)
        {
            if (rule.Name != symbol.Name
                || (rule.Linkage is not null && rule.Linkage != (symbol.Storage == Storage.Static ? "internal" : "external"))
                || (rule.TranslationUnit is not null && rule.TranslationUnit != TranslationUnitPath)
                || (rule.DeclarationFile is not null && rule.DeclarationFile != SourceFileOrigin.Of(declaration)?.Identity)) continue;
            var expected = BindOverrideSignature(rule);
            if (OverrideTypeKey(expected) != key)
                throw CPreprocessingOptions.FunctionError(rule, "signature mismatch: expected " + OverrideTypeKey(expected) + ", found " + key);
            ValidateOverrideTarget(rule, actual);
            ValidateFunctionAttributes(rule, symbol);
            if (_functionReplacements.TryGetValue(symbol, out previous) && previous.Rule != rule)
                throw CPreprocessingOptions.FunctionError(rule, "multiple rules select the same function");
            if (!_functionOverrideMatches.TryGetValue(rule, out var matches))
                _functionOverrideMatches[rule] = matches = new();
            matches.Add(symbol);
            if (matches.Count > 1)
                throw CPreprocessingOptions.FunctionError(rule, "ambiguous function selector; specify translationUnit and/or linkage");
            _functionReplacements[symbol] = (rule, key, TranslationUnitPath, SourceFileOrigin.Of(declaration)?.Identity);
            // A later declaration can select a previously bound definition.
            var index = Functions.FindIndex(f => ReferenceEquals(f.Sym, symbol));
            if (index >= 0) Functions[index] = BuildFunctionReplacement(symbol);
        }
    }

    private CType.Func BindOverrideSignature(FunctionOverride rule)
    {
        // Parse type spellings with the ordinary C grammar, then resolve typedefs
        // against a private snapshot of the declaration's binding environment.
        var signature = rule.Signature;
        var source = signature.ReturnType + " __dotcc_override_signature(" +
            (signature.ParameterTypes.Count == 0 ? "void" : string.Join(",", signature.ParameterTypes)) + ");";
        try
        {
            using var lexer = BytesLexer.FromString(source, C.BuildLexer());
            using var validator = new CTokenValidator(lexer);
            using var keywords = new DialectKeywordRewriter(validator, CDialect.Default);
            using var types = new TypeNameRewriter(keywords, _typedefs.Keys.Concat(Compiler.PredefinedTypeNames));
            using var tokens = new SyncLATokenIterator(types);
            var root = C.BuildSourceLocatedParser().ParseInput(tokens, debugger: null, trimReductions: true);
            if (root.Content is C.FnsOne one) root = one.Arg0;
            if (root.IsError || root.Content is not C.FuncProto proto)
                throw new CompileException("expected one function signature");
            var bound = CreateMacroEvaluator().ExtractFnSig(proto.Arg0);
            if (bound.Name != "__dotcc_override_signature") throw new CompileException("invalid signature declarator");
            return new(bound.Return, bound.Params.Select(p => p.Type).ToArray(), bound.Variadic);
        }
        catch (Exception ex) when (ex is CompileException or LALR.CC.ParseErrorException or LexerException)
        { throw CPreprocessingOptions.FunctionError(rule, "invalid signature: " + ex.Message); }
    }

    private static void ValidateOverrideTarget(FunctionOverride rule, CType.Func type)
    {
        static bool Supported(CType t) => !t.IsVolatile && !t.IsAtomic && t.Unqualified switch
        {
            CType.Prim or CType.VoidType or CType.Named or CType.Enum => true,
            CType.Pointer p => Supported(p.Pointee),
            _ => false,
        };
        if (type.Variadic || !Supported(type.Return) || type.Params.Any(p => !Supported(p)))
            throw CPreprocessingOptions.FunctionError(rule, "unsupported signature: variadic, volatile/atomic, and callback signatures are not supported");
        if (rule.Target.Kind == "intrinsic")
        {
            var intrinsic = FunctionOverrideIntrinsic.Find(rule.Target.Value)
                ?? throw CPreprocessingOptions.FunctionError(rule, "unknown intrinsic: " + rule.Target.Value);
            if (!intrinsic.MatchesSignature(type))
                throw CPreprocessingOptions.FunctionError(rule,
                    rule.Target.Value + " requires " + intrinsic.SignatureDescription);
        }
    }

    private static void ValidateFunctionAttributes(FunctionOverride rule, Symbol symbol)
    {
        if (symbol is { FromSystemHeader: true, Name: "abort" or "exit" or "_Exit" })
            throw CPreprocessingOptions.FunctionError(rule, "special system termination functions are not supported");
        if (symbol.IsNoReturn != rule.Target.DoesNotReturn)
            throw CPreprocessingOptions.FunctionError(rule, "noreturn declaration and target doesNotReturn contract must agree");
    }

    // Top-level parameter qualifiers do not participate in C function type
    // compatibility; pointee qualifiers and named identities do.
    internal static string OverrideTypeKey(CType type, bool parameter = false)
    {
        var quals = parameter ? TypeQual.None : type.Quals;
        var prefix = quals == TypeQual.None ? "" : "[" + quals + "]";
        return prefix + (type.Unqualified switch
        {
            CType.Pointer p => OverrideTypeKey(p.Pointee) + "*",
            CType.Func f => OverrideTypeKey(f.Return) + "(" + string.Join(",", f.Params.Select(p => OverrideTypeKey(p, true))) + (f.Variadic ? ",..." : "") + ")",
            CType.Named n => "named:" + n.Name,
            CType.Enum e => "enum:" + e.Name,
            _ => type.Describe(),
        });
    }

    private FuncDef BuildFunctionReplacement(Symbol symbol)
    {
        var replacement = _functionReplacements[symbol];
        var type = (CType.Func)symbol.Type;
        var parameters = type.Params.Select((t, i) => new Symbol
        {
            Name = "__arg" + i, TargetName = "__arg" + i, Kind = SymKind.Param, Type = t,
        }).ToArray();
        var call = new Call(symbol.Name, parameters.Select(p => (CExpr)new VarRef(p) { Type = p.Type }).ToArray(), type.Params)
        { Type = type.Return, SemanticTarget = replacement.Rule.Target };
        CStmt statement = type.Return is CType.VoidType || replacement.Rule.Target.DoesNotReturn
            ? new ExprStmt(call) : new Return(call);
        return new(symbol, parameters, new Block(new[] { statement }), false);
    }

    internal void FinishFunctionOverrides()
    {
        if (_functionOverrideOptions is not { } options) return;
        foreach (var (symbol, replacement) in _functionReplacements)
        {
            if (!Functions.Any(f => ReferenceEquals(f.Sym, symbol))) Functions.Add(BuildFunctionReplacement(symbol));
            _protoOnlyFuncs.Remove(symbol.Name);
            var fields = new List<(string Name, string Value)> { ("name", symbol.Name),
                ("signature", replacement.Signature), ("target", replacement.Rule.Target.Kind + ":" + replacement.Rule.Target.Value),
                ("matches", "1"),
                ("origin", replacement.Rule.Origin), ("translationUnit", replacement.TranslationUnit),
                ("declarationFile", replacement.DeclarationFile ?? "unknown") };
            if (replacement.Rule.Target.DoesNotReturn) fields.Add(("doesNotReturn", "true"));
            CPreprocessingOptions.WriteEvent(options.Report, "function-override", fields.ToArray());
        }
        foreach (var rule in options.FunctionOverrides)
            if (!_functionOverrideMatches.ContainsKey(rule))
            {
                if (rule.RequireMatch) throw CPreprocessingOptions.FunctionError(rule, "requireMatch was not satisfied");
                CPreprocessingOptions.WriteEvent(options.Report, "function-override-unmatched", ("name", rule.Name), ("origin", rule.Origin));
            }
    }

    internal IEnumerable<FunctionOverrideMetadata> FunctionOverrideMetadata => Functions.Select(f =>
    {
        _functionReplacements.TryGetValue(f.Sym, out var replacement);
        return new FunctionOverrideMetadata(f.Sym.TargetName, OverrideTypeKey(f.Sym.Type), replacement.Rule?.Target,
            replacement.Rule?.TranslationUnit, replacement.Rule?.DeclarationFile, replacement.Rule?.Linkage);
    });
}
