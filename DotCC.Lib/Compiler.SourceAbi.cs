using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Backends;
using DotCC.Ir;

namespace DotCC;

public static partial class Compiler
{
    private sealed class SourceAbiForeignChecker
    {
        private readonly Dictionary<string, StructTypeDef> _types;
        private readonly Dictionary<string, bool> _containsStorage = new(StringComparer.Ordinal);
        private readonly HashSet<string> _visiting = new(StringComparer.Ordinal);
        private readonly bool _active;

        internal SourceAbiForeignChecker(IrBuilder unit)
        {
            _active = unit.SourceAbi.LongDouble == CLongDoubleRepresentation.X87Storage16;
            _types = _active ? unit.Types.ToDictionary(t => t.Name, StringComparer.Ordinal) : new(StringComparer.Ordinal);
        }

        private bool ContainsStorage(CType type)
        {
            switch (type.Unqualified)
            {
                case CType.LongDoubleStorage16: return true;
                case CType.Array array: return ContainsStorage(array.Element);
                case CType.Named named when _types.TryGetValue(named.Name, out var aggregate):
                    if (_containsStorage.TryGetValue(named.Name, out bool contains)) return contains;
                    if (!_visiting.Add(named.Name)) return false;
                    contains = aggregate.Fields.Any(field => ContainsStorage(field.Type));
                    _visiting.Remove(named.Name);
                    return _containsStorage[named.Name] = contains;
                // Pointers (including callback values) carry addresses, not the
                // pointee's floating representation. Stop before following them.
                default: return false;
            }
        }

        internal bool Restricted(CType.Func signature) => _active && (ContainsStorage(signature.Return)
            // C array parameters decay to pointers even if the IR retains bounds.
            || signature.Params.Any(p => p.Unqualified is not CType.Array && ContainsStorage(p)));

        internal void Validate(CType.Func signature, string name)
        {
            if (Restricted(signature)) throw ForeignAbiUnsupported(name);
        }
    }

    private static CompileException ForeignAbiUnsupported(string name) => new(
        "source ABI long double x87-storage16 supports storage/layout only; native x87 scalar or aggregate calling ABI is unsupported for '" + name + "'");

    private static IEnumerable<FuncDef> NativeExportCandidates(IrBuilder unit) => unit.Functions.Where(f =>
        f.Sym.Name != "main" && f.Sym.Storage != Storage.Static && !f.Variadic
        && f.Sym.Type.Unqualified is CType.Func signature && !VaListLifetimeValidator.HasByRefLikeSignature(signature));

    private static string SourceAbiObjectProof(IrBuilder unit)
    {
        var checker = new SourceAbiForeignChecker(unit);
        var imports = unit.ProtoOnlyReferenced.Values.Where(s => s.Type.Unqualified is CType.Func { Variadic: false } f && checker.Restricted(f))
            .Select(s => new SourceAbiForeignRestriction("import", s.Name, s.TargetName));
        var definitions = unit.Functions.Where(f => f.Sym.Storage != Storage.Static && f.Sym.Type.Unqualified is CType.Func type && checker.Restricted(type))
            .Select(f => new SourceAbiForeignRestriction("definition", f.Sym.Name, f.Sym.TargetName));
        return SourceAbiForeignMetadata.Serialize(unit.SourceAbi, imports.Concat(definitions));
    }

    private static void ValidateSourceAbiBindings(IrBuilder unit, EmitMode emit)
    {
        if (unit.SourceAbi.LongDouble != CLongDoubleRepresentation.X87Storage16) return;
        var checker = new SourceAbiForeignChecker(unit);
        if (emit == EmitMode.SharedLib)
            foreach (var function in NativeExportCandidates(unit))
                checker.Validate((CType.Func)function.Sym.Type.Unqualified, function.Sym.Name);
        void Expression(CExpr expression)
        {
            var type = expression switch
            {
                IndirectCall call => call.Callee.Type.Unqualified,
                Call { CalleeSym: { } symbol } => symbol.Type.Unqualified,
                _ => null,
            };
            if (type is CType.Pointer pointer) type = pointer.Pointee.Unqualified;
            if (type is CType.Func { IsNativeCallConv: true } signature) checker.Validate(signature, "native function pointer");
        }
        void Expr(CExpr? expression) { if (expression is not null) VaListLifetimeValidator.Walk(expression, Expression); }
        void Statement(CStmt? statement)
        {
            switch (statement)
            {
                case Block block: foreach (var child in block.Stmts) Statement(child); break;
                case Seq sequence: foreach (var child in sequence.Stmts) Statement(child); break;
                case DeclStmt declaration: foreach (var local in declaration.Decls) Expr(local.Init); break;
                case ArrayDecl array:
                    Expr(array.CountExpr); if (array.Inits is { } values) foreach (var value in values) Expr(value); break;
                case ExprStmt expression: Expr(expression.Expr); break;
                case Return result: Expr(result.Value); break;
                case Labeled label: Statement(label.Body); break;
                case If branch: Expr(branch.Cond); Statement(branch.Then); Statement(branch.Else); break;
                case While loop: Expr(loop.Cond); Statement(loop.Body); break;
                case DoWhile loop: Statement(loop.Body); Expr(loop.Cond); break;
                case For loop: Statement(loop.Init); Expr(loop.Cond); Expr(loop.Post); Statement(loop.Body); break;
                case Switch choice:
                    Expr(choice.Subject); foreach (var section in choice.Sections) foreach (var child in section.Body) Statement(child); break;
                case CaseLabelStmt label: Expr(label.CaseExpr); Statement(label.Body); break;
                case SetjmpGuard guard: Expr(guard.Env); Statement(guard.TryBody); Statement(guard.CatchBody); break;
                case SetjmpCapture capture: Expr(capture.Env); Expr(capture.Target); Statement(capture.Body); break;
                case DeferGuard guard: Statement(guard.Body); Statement(guard.Cleanup); break;
            }
        }
        foreach (var global in unit.Globals) Expr(global.Init);
        foreach (var function in unit.Functions) Statement(function.Body);
    }

    private static void ValidateSourceAbiObjectImports(IEnumerable<string> nativeSymbols,
        IReadOnlyList<string> objectTexts, IReadOnlyList<string> objectPaths, IReadOnlyList<SourceAbiForeignProof> proofs)
    {
        var active = nativeSymbols.ToHashSet(StringComparer.Ordinal);
        if (active.Count == 0) return;
        for (int index = 0; index < proofs.Count; index++)
        {
            var proof = proofs[index];
            foreach (var restriction in proof.Restrictions)
                if (restriction.Role == "import" && active.Contains(restriction.Name)) throw ForeignAbiUnsupported(restriction.Name);
            if (!proof.X87Storage16 || proof.Complete) continue;
            foreach (var line in objectTexts[index].Split('\n').Where(l => l.StartsWith(FragImport, StringComparison.Ordinal)))
            {
                var entry = line[FragImport.Length..];
                var space = entry.IndexOf(' ');
                if (space > 0 && active.Contains(entry[..space]))
                    throw new CompileException("storage-only source ABI object lacks foreign ABI proof: '" + objectPaths[index] + "'; regenerate objects");
            }
        }
    }
}
