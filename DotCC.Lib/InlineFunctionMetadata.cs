using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotCC.Ir;

namespace DotCC;

/// <summary>A typed-IR proof candidate, independent of C# spelling. A missing shape
/// means that equivalence has not been proved; it never means an empty body.</summary>
internal sealed record InlineFunctionMetadata(string OriginalName, bool IsStatic, string Shape,
    bool AddressUsed, string[] Dependencies)
{
    internal const string Version = "//!!dotcc-obj inline-metadata:3";
    internal const string Prefix = "//!!dotcc-obj inline:";
    internal string Serialize(string name) => Prefix + string.Join(" ", name, OriginalName,
        IsStatic ? "1" : "0", Shape, AddressUsed ? "1" : "0", string.Join(",", Dependencies));

    internal static (string Name, InlineFunctionMetadata Metadata) Parse(string line)
    {
        var parts = line[Prefix.Length..].Split(' ');
        static bool Identifier(string s) => s.Length > 0 && s.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '@');
        if (parts.Length != 6 || !Identifier(parts[0]) || !Identifier(parts[1])
            || parts[2] is not ("0" or "1") || parts[4] is not ("0" or "1")
            || !(parts[3] == "-" || parts[3].Length == 64 && parts[3].All(Uri.IsHexDigit))
            || parts[5].Split(',', StringSplitOptions.RemoveEmptyEntries).Any(s => !Identifier(s)))
            throw new CompileException("invalid inline metadata; regenerate objects");
        return (parts[0], new(parts[1], parts[2] == "1", parts[3], parts[4] == "1",
            parts[5].Split(',', StringSplitOptions.RemoveEmptyEntries)));
    }

    internal static InlineFunctionMetadata From(FuncDef function, IrBuilder unit, bool addressUsed)
    {
        var proof = new ShapeWriter(unit);
        proof.Atom(function.Sym.Name);
        proof.Type(function.Sym.Type);
        proof.Atom(function.Sym.IsNoReturn);
        proof.Atom(function.Sym.Deprecated);
        proof.Atom(function.Variadic);
        foreach (var parameter in function.Params) proof.Local(parameter);
        proof.Statement(function.Body);
        return new(function.Sym.Name, function.Sym.Storage == Storage.Static,
            proof.Supported ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(proof.Text.ToString()))) : "-",
            addressUsed, proof.Dependencies.ToArray());
    }

    // Explicit, conservative traversal. New IR nodes opt out until their complete
    // semantics are described here. No reflection and no generated-text matching.
    private sealed class ShapeWriter(IrBuilder unit)
    {
        internal readonly StringBuilder Text = new();
        internal readonly List<string> Dependencies = new();
        internal bool Supported = true;
        private readonly Dictionary<Symbol, int> _locals = new();
        private readonly HashSet<string> _types = new(StringComparer.Ordinal);
        private bool _observesIdentity;

        internal void Atom(object? value)
        {
            var s = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "<null>";
            Text.Append(s.Length).Append(':').Append(s).Append(';');
        }
        internal void Type(CType type)
        {
            Atom(ObjectAggregateMetadata.Describe(type));
            switch (type)
            {
                case CType.Pointer p: Type(p.Pointee); break;
                case CType.Array a: Type(a.Element); break;
                case CType.Func f: Type(f.Return); foreach (var p in f.Params) Type(p); break;
                // Named aggregate identity is shared by the final module. The
                // object linker separately validates every complete layout and
                // resolves forward declarations. Walking the entire pointer graph
                // here would confuse an unrelated opaque type's completion with
                // a change in this function (e.g. reading Wrapper.value).
                case CType.Enum e when _types.Add(e.Name):
                    var enumeration = unit.Enums.FirstOrDefault(t => t.Name == e.Name);
                    if (enumeration == null) { Supported = false; break; }
                    foreach (var member in enumeration.Members) { Atom(member.Name); Atom(member.Value); }
                    break;
                case CType.Prim or CType.VoidType or CType.Named or CType.Enum or CType.ComplexType or CType.Float128Type: break;
                default: Supported = false; break;
            }
        }
        internal void Local(Symbol symbol)
        {
            if (!_locals.TryAdd(symbol, _locals.Count)) { Supported = false; return; }
            Type(symbol.Type); Atom(symbol.Alignment); Atom(symbol.AddressTaken);
            Atom(symbol.Storage); Atom(symbol.IsThreadLocal); Atom(symbol.IsConstexpr); Atom(symbol.ConstValue);
            if (symbol.IsGlobal || symbol.Storage == Storage.Static) Supported = false;
        }
        private void Reference(Symbol symbol)
        {
            Type(symbol.Type);
            if (_locals.TryGetValue(symbol, out var local)) { Atom("local"); Atom(local); }
            else if (symbol.Kind == SymKind.Func)
            {
                Atom("function"); Atom(symbol.Name);
                if (symbol.Storage == Storage.Static)
                {
                    Atom(Dependencies.Count); Dependencies.Add(symbol.TargetName);
                }
                else Atom(symbol.TargetName);
            }
            else if (symbol.Kind == SymKind.EnumConst) { Atom("enum"); Atom(symbol.Name); Atom(symbol.ConstValue); }
            else if (symbol.IsGlobal && symbol.Storage != Storage.Static)
            { Atom("global"); Atom(symbol.TargetName); Atom(symbol.IsThreadLocal); }
            else if (symbol.IsGlobal && symbol.Storage == Storage.Static && symbol.Type.IsConst
                && !symbol.IsThreadLocal && !symbol.AddressTaken && !_observesIdentity
                && IsConstantValueType(symbol.Type, new HashSet<string>(StringComparer.Ordinal))
                && unit.Globals.FirstOrDefault(g => ReferenceEquals(g.Sym, symbol)) is { } global
                && IsConstantInitializer(global.Init))
            {
                // These bodies read equal immutable values. Include the bound
                // declaration and initializer, not just the source spelling: two
                // object files may initialize the same name differently.
                Atom("static-constant-value"); Atom(symbol.TargetName);
                Expression(global.Init);
            }
            else Supported = false; // Mutable/identity-sensitive TU storage or unbound references.
        }
        private bool IsConstantValueType(CType type, HashSet<string> visiting)
        {
            if (type.IsVolatile || type.IsAtomic) return false;
            switch (type)
            {
                case CType.Prim or CType.Enum or CType.ComplexType or CType.Float128Type: return true;
                case CType.Array a: return a.Count != null && IsConstantValueType(a.Element, visiting);
                case CType.Named n:
                    if (!visiting.Add(n.Name)) return false;
                    var definition = unit.Types.FirstOrDefault(t => t.Name == n.Name && !t.IsIncomplete);
                    var result = definition != null && definition.Fields.All(f => IsConstantValueType(f.Type, visiting));
                    visiting.Remove(n.Name);
                    return result;
                default: return false; // Pointers can depend on distinct objects, even in const aggregates.
            }
        }
        private static bool IsConstantInitializer(CExpr? e) => e switch
        {
            null or LitInt or LitFloat or LitBool or DefaultLit or EnumConstRef or SizeOfExpr or OffsetOf => true,
            Unary u when u.Op is UnOp.Plus or UnOp.Neg or UnOp.BitNot or UnOp.LogNot => IsConstantInitializer(u.Operand),
            Binary b => IsConstantInitializer(b.Left) && IsConstantInitializer(b.Right),
            Cast c => IsConstantInitializer(c.Operand),
            Paren p => IsConstantInitializer(p.Inner),
            CondExpr c => IsConstantInitializer(c.Cond) && IsConstantInitializer(c.Then) && IsConstantInitializer(c.Else),
            StructInit s => s.Members.All(m => IsConstantInitializer(m.Value)),
            InlineArrayInit a => a.Elems.All(IsConstantInitializer),
            _ => false,
        };
        private void Expressions(IReadOnlyList<CExpr>? expressions)
        {
            Atom(expressions?.Count ?? -1);
            if (expressions != null) foreach (var e in expressions) Expression(e);
        }
        private void Expression(CExpr? expression)
        {
            // Pointer/array results can expose storage identity (including &g.member
            // and implicit array decay, which Symbol.AddressTaken does not record).
            var previous = _observesIdentity;
            _observesIdentity |= expression?.Type.Unqualified is CType.Pointer or CType.Array or CType.Func;
            try { ExpressionCore(expression); }
            finally { _observesIdentity = previous; }
        }
        private void ExpressionCore(CExpr? expression)
        {
            if (expression == null) { Atom("null-expression"); return; }
            Type(expression.Type); Atom(expression.IsLValue);
            switch (expression)
            {
                case LitInt e: Atom("int"); Atom(e.Digits); Atom(e.Value); break;
                case LitFloat e: Atom("float"); Atom(e.Text); break;
                case LitBool e: Atom("bool"); Atom(e.Value); break;
                case LitStr e: Atom("str"); Atom(e.Segments.Count); foreach (var s in e.Segments) Atom(s); break;
                case LitU16Str e: Atom("str16"); Atom(e.Segments.Count); foreach (var s in e.Segments) Atom(s); break;
                case LitU32Str e: Atom("str32"); Atom(e.Segments.Count); foreach (var s in e.Segments) Atom(s); break;
                case NullPtr: Atom("null"); break;
                case DefaultLit: Atom("default"); break;
                case RuntimeIntrinsic e: Atom("runtime"); Atom(e.Kind); break;
                case VarRef e: Atom("ref"); Reference(e.Sym); break;
                case EnumConstRef e: Atom("enum-ref"); Reference(e.Sym); break;
                case Unary e: Atom("unary"); Atom(e.Op); Expression(e.Operand); break;
                case Binary e: Atom("binary"); Atom(e.Op); Expression(e.Left); Expression(e.Right); break;
                case Assign e: Atom("assign"); Atom(e.CompoundOp); Expression(e.Target); Expression(e.Value); break;
                case Call e:
                    Atom("call"); Atom(e.Callee);
                    if (e.CalleeSym != null) Reference(e.CalleeSym); else Atom("unresolved");
                    Atom(e.ParamTypes?.Count ?? -1);
                    if (e.ParamTypes != null) foreach (var p in e.ParamTypes) Type(p);
                    Expressions(e.Args); break;
                case IndirectCall e: Atom("indirect"); Expression(e.Callee); Expressions(e.Args); break;
                case Cast e: Atom("cast"); Type(e.Target); Expression(e.Operand); break;
                case CondExpr e: Atom("conditional"); Expression(e.Cond); Expression(e.Then); Expression(e.Else); break;
                case Ir.Index e: Atom("index"); Expression(e.Base); Expression(e.Idx); break;
                case Member e: Atom("member"); Atom(e.Field); Atom(e.Arrow); Expression(e.Base); break;
                case Paren e: Atom("paren"); Expression(e.Inner); break;
                case CommaOp e: Atom("comma"); Expressions(e.Items); break;
                case CommaSeq e: Atom("comma-seq"); Expressions(e.Items); break;
                case StatementExpression e: Atom("statement-expression"); Statement(e.Body); Expression(e.Value); break;
                case SizeOfExpr e: Atom("sizeof"); Type(e.Of); break;
                case OffsetOf e: Atom("offsetof"); Type(e.StructType); Atom(e.Path.Count); foreach (var p in e.Path) Atom(p); if (e.MemberType != null) Type(e.MemberType); break;
                case StructInit e:
                    Atom("struct-init"); Atom(e.Members.Count);
                    foreach (var m in e.Members) { Atom(m.Name); Type(m.FieldType); Expression(m.Value); } break;
                case StackArray e: Atom("stack-array"); Type(e.Element); Expressions(e.Elems); break;
                case InlineArrayInit e: Atom("inline-array"); Type(e.Element); Expressions(e.Elems); break;
                case StackNew e: Atom("stack-new"); Type(e.StructType); break;
                default: Supported = false; break;
            }
        }
        private void Statements(IReadOnlyList<CStmt> statements)
        { Atom(statements.Count); foreach (var s in statements) Statement(s); }
        internal void Statement(CStmt? statement)
        {
            switch (statement)
            {
                case null: Atom("null-statement"); break;
                case Block s: Atom("block"); Statements(s.Stmts); break;
                case Seq s: Atom("seq"); Statements(s.Stmts); break;
                case DeclStmt s:
                    Atom("decl"); Atom(s.Decls.Count);
                    foreach (var d in s.Decls) { Local(d.Sym); Expression(d.Init); } break;
                case ArrayDecl s: Atom("array"); Local(s.Sym); Type(s.Element); Expression(s.CountExpr); Expressions(s.Inits); break;
                case ExprStmt s: Atom("expr"); Expression(s.Expr); break;
                case If s: Atom("if"); Expression(s.Cond); Statement(s.Then); Statement(s.Else); break;
                case While s: Atom("while"); Expression(s.Cond); Statement(s.Body); break;
                case DoWhile s: Atom("do"); Statement(s.Body); Expression(s.Cond); break;
                case For s: Atom("for"); Statement(s.Init); Expression(s.Cond); Expression(s.Post); Statement(s.Body); break;
                case Return s: Atom("return"); Expression(s.Value); break;
                case Break: Atom("break"); break;
                case Continue: Atom("continue"); break;
                case Goto s: Atom("goto"); Atom(s.Label); break;
                case Labeled s: Atom("label"); Atom(s.Name); Statement(s.Body); break;
                case CaseLabelStmt s: Atom("case"); Expression(s.CaseExpr); Statement(s.Body); break;
                case FallthroughMarker: Atom("fallthrough"); break;
                case Switch s:
                    Atom("switch"); Expression(s.Subject); Atom(s.Sections.Count);
                    foreach (var section in s.Sections)
                    {
                        Atom(section.Labels.Count);
                        foreach (var label in section.Labels) { Expression(label.CaseExpr); Expression(label.HiExpr); }
                        Statements(section.Body);
                    } break;
                default: Supported = false; break;
            }
        }
    }
}
