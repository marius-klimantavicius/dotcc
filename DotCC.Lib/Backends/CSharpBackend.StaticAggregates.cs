using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private static int StaticAggregateStores(CExpr expression) => expression switch
    {
        StructInit aggregate => SumMembers(aggregate),
        InlineArrayInit array => SumElements(array),
        _ => 1
    };

    private static int SumMembers(StructInit aggregate)
    {
        int count = 0;
        foreach (var member in aggregate.Members) count += StaticAggregateStores(member.Value);
        return count;
    }

    private static int SumElements(InlineArrayInit array)
    {
        int count = 0;
        foreach (var element in array.Elems) count += StaticAggregateStores(element);
        return count;
    }

    private bool EmitLargeStaticAggregateInitializer(StringBuilder output, GlobalVar global)
    {
        if (global.Init is not StructInit aggregate) return false;
        // Static C storage is already zeroed and its final address is established
        // before these constant stores. Write through that address instead of
        // building enormous by-value object/factory argument temporaries.
        var stores = new StringBuilder();
        string alias = InitializerAlias(global);
        string destination = "(*" + alias + ")";
        var previous = _globalAddressBases;
        _globalAddressBases = new(previous, System.Collections.Generic.ReferenceEqualityComparer.Instance)
        {
            [global.Sym] = alias
        };
        try { EmitStaticAggregateStores(stores, destination, aggregate.Type, aggregate); }
        finally { _globalAddressBases = previous; }
        // Reuse the address within each bounded initialization helper. An
        // explicit pin also remains valid when the same object is linked into
        // instance/context storage instead of the ordinary unmanaged statics.
        // Do not let one fixed block undo C025's whole-statement partitioning.
        AppendInitializerChunks(output, stores.ToString(), Cs(aggregate.Type) + "* " + alias + " = &"
            + GlobalStorageReferences.Emit(global.Sym.TargetName), pin: true);
        return true;
    }

    private readonly HashSet<string> _initializerNames = new(StringComparer.Ordinal);

    private string InitializerAlias(GlobalVar global)
    {
        string name = global.Sym.TargetName.TrimStart('@') + "_destination";
        while (!_initializerNames.Add(name)) name += "_";
        return EmitHelpers.Id(name);
    }

    private static void AppendInitializerChunks(StringBuilder output, string stores, string declaration, bool pin)
    {
        var part = new StringBuilder();
        void Flush()
        {
            if (part.Length == 0) return;
            if (pin) output.Append("        fixed (").Append(declaration).Append(")\n        {\n");
            else output.Append("        {\n            ").Append(declaration).Append('\n');
            output.Append(part).Append("\n        };\n");
            part.Clear();
        }
        foreach (var statement in Compiler.InitializerStatements(stores))
        {
            if (part.Length + statement.Length > 16384) Flush();
            part.Append(statement);
        }
        Flush();
    }

    private static string InitializerHelpers(StringBuilder members, string stores, string pointer, string alias, string stem,
        string methodPrefix = "private static unsafe void ", Action<string, string>? addMember = null,
        Func<string, string>? callName = null)
    {
        // Complete statements (including rows/copies) stay together. Build a
        // bounded call tree too, rather than moving a giant list into Create.
        int level = 0;
        do
        {
            int index = 0;
            var part = new StringBuilder();
            var calls = new StringBuilder();
            void Flush()
            {
                if (part.Length == 0) return;
                string name = stem + "_" + level + "_" + index++;
                var member = "    " + methodPrefix + name + "(" + pointer + " " + alias
                    + ")\n    {\n" + part + "\n    }\n";
                if (addMember is null) members.Append(member);
                else addMember(name, member);
                calls.Append("        ").Append(callName is null ? name : callName(name)).Append('(').Append(alias).Append(");\n");
                part.Clear();
            }
            foreach (string statement in Compiler.InitializerStatements(stores))
            {
                if (part.Length + statement.Length > 16384) Flush();
                part.Append(statement);
            }
            Flush();
            stores = calls.ToString();
            level++;
        } while (stores.Length > 16384);
        return stores;
    }

    private void EmitStaticAggregateStores(StringBuilder output, string destination, CType type, CExpr value,
        bool readableRow = false, bool zeroed = true)
    {
        if (value is StructInit aggregate)
        {
            if (readableRow && TryReadableRecord(aggregate, out string row))
            {
                output.Append("        ").Append(destination).Append(" = ").Append(row).Append(";\n");
                return;
            }
            var assigned = new HashSet<string>(StringComparer.Ordinal);
            bool union = aggregate.Type.Unqualified is CType.Named named
                && _aggregateDefinitions.TryGetValue(named.Name, out var definition) && definition.IsUnion;
            foreach (var member in aggregate.Members)
            {
                bool untouched = zeroed && (!union || assigned.Count == 0) && !assigned.Contains(member.Name);
                assigned.Add(member.Name);
                EmitStaticAggregateStores(output, destination + "." + DotCC.EmitHelpers.Id(member.Name),
                    member.FieldType, member.Value, zeroed: untouched);
            }
            return;
        }
        if (value is InlineArrayInit array)
        {
            var elementType = Cs(array.Element);
            if (EmitPrimitiveCopies(output, IsFixedBufferType(elementType) ? destination : "&" + destination,
                array.Element, array.Elems, zeroed)) return;
            for (int index = 0; index < array.Elems.Count; ++index)
            {
                string element = IsFixedBufferType(elementType) ? destination + "[" + index + "]"
                    : "((" + elementType + "*)&" + destination + ")[" + index + "]";
                EmitStaticAggregateStores(output, element, array.Element, array.Elems[index], readableRow: true, zeroed: zeroed);
            }
            return;
        }
        output.Append("        ").Append(destination).Append(" = ").Append(Coerced(value, type)).Append(";\n");
    }

    // Bounds are deliberately far below C026's 32 KiB aggregate cutoff: at
    // most 512 value bytes, 32 leaf stores and 4 KiB source per complete row.
    // _PyRuntime and records containing inline arrays always stay in place.
    private bool TryReadableRecord(StructInit value, out string text, int depth = 0)
    {
        text = "";
        if (depth > 8 || value.Type.Unqualified is not CType.Named named
            || !_aggregateDefinitions.TryGetValue(named.Name, out var definition)
            || definition.IsIncomplete || !ReadableRecordLayout(definition, depth)
            || _offsetModel.Aggregate(named.Name).Size > 512 || StaticAggregateStores(value) > 32
            || definition.IsUnion && value.Members.Count > 1) return false;
        var assigned = new HashSet<string>(StringComparer.Ordinal);
        string pad = new(' ', 8 + depth * 4);
        var result = new StringBuilder("new()\n").Append(pad).Append("{\n");
        foreach (var member in value.Members)
        {
            if (!assigned.Add(member.Name) || !definition.Fields.Any(field => field.Name == member.Name)) return false;
            string expression;
            if (member.Value is StructInit nested)
            {
                if (!TryReadableRecord(nested, out expression, depth + 1)) return false;
            }
            else
            {
                if (!ReadableRecordScalar(member.Value)) return false;
                expression = ReadableRecordConversion(member.Value, member.FieldType);
            }
            result.Append(pad).Append("    ").Append(EmitHelpers.Id(member.Name)).Append(" = ").Append(expression).Append(",\n");
        }
        text = result.Append(pad).Append('}').ToString();
        return text.Length <= 4096;
    }

    private bool ReadableRecordLayout(StructTypeDef definition, int depth)
    {
        if (depth > 8 || definition.IsIncomplete) return false;
        var layout = _offsetModel.Aggregate(definition.Name);
        int end = 0;
        foreach (var field in definition.Fields)
        {
            if (field.IsBitField || field.IsAnonymousAggregate || field.IsFlexibleArray
                || field.Type.Unqualified is CType.Array) return false;
            if (field.Type.Unqualified is CType.Named nested)
            {
                if (!_aggregateDefinitions.TryGetValue(nested.Name, out var child) || !ReadableRecordLayout(child, depth + 1)) return false;
            }
            else if (field.Type.Unqualified is not (CType.Prim or CType.Enum or CType.Pointer or CType.Func)) return false;
            var fieldLayout = _offsetModel.Type(IrBuilder.LayoutType(field.Type));
            if (fieldLayout.Size == 0 || !layout.Offsets.TryGetValue(field.Name, out int offset)
                || offset < 0 || offset + fieldLayout.Size > layout.Size || !definition.IsUnion && offset < end) return false;
            end = offset + fieldLayout.Size;
        }
        return true;
    }

    private bool ReadableRecordScalar(CExpr value) => value switch
    {
        LitInt or LitFloat or LitBool or LitStr or LitU16Str or LitU32Str or NullPtr or DefaultLit or EnumConstRef or SizeOfExpr or OffsetOf => true,
        Paren p => ReadableRecordScalar(p.Inner),
        Cast c => ReadableRecordScalar(c.Operand),
        Unary { Op: UnOp.AddrOf } address => ReadableRelocation(address.Operand),
        Unary { Op: UnOp.Plus or UnOp.Neg or UnOp.BitNot or UnOp.LogNot } unary => ReadableRecordScalar(unary.Operand),
        Binary binary => ReadableRecordScalar(binary.Left) && ReadableRecordScalar(binary.Right),
        CondExpr conditional => ReadableRecordScalar(conditional.Cond) && ReadableRecordScalar(conditional.Then)
            && ReadableRecordScalar(conditional.Else),
        VarRef reference => reference.Sym.Kind == SymKind.Func || reference.Sym.Type.Unqualified is CType.Array,
        _ => false
    };

    private bool ReadableRelocation(CExpr value) => value switch
    {
        VarRef reference => reference.Sym.IsGlobal,
        Paren p => ReadableRelocation(p.Inner),
        Member { Arrow: false } member => ReadableRelocation(member.Base),
        DotCC.Ir.Index index => ReadableRelocation(index.Base) && _foldConstant(index.Idx) is not null,
        _ => false
    };

    private string ReadableRecordConversion(CExpr value, CType target)
    {
        // Only an original non-enum integer constant in range may omit its
        // narrow conversion. Do not fold an out-of-range signedness boundary
        // into a deceptively representable literal or erase an enum cast.
        bool boundary = false;
        VaListLifetimeValidator.Walk(value, node => boundary |= node is Cast || node.Type.Unqualified is CType.Enum);
        if (!boundary && target.Unqualified is CType.Prim { Integer: true, Bytes: <= 2 } primitive
            && value.Type.Unqualified is CType.Prim { Integer: true }
            && PayloadInteger(_foldConstant(value), out var number))
        {
            long minimum = primitive.Signed ? -(1L << (primitive.Bytes * 8 - 1)) : 0;
            long maximum = primitive.Signed ? (1L << (primitive.Bytes * 8 - 1)) - 1 : (1L << (primitive.Bytes * 8)) - 1;
            if (number >= minimum && number <= maximum && Cs(target) != "CBool")
                return number.ToString(CultureInfo.InvariantCulture);
        }
        return Coerced(value, target);
    }
}
