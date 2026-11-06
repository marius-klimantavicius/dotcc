#nullable enable
using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private CExpr BuildSourceArrayFieldInitializer(IReadOnlyList<Item> fields, CType.Named type)
    {
        // Spill enclosing operands before the initializer starts. Each field is
        // then stored before the next field can evaluate or hoist any effects.
        var buffer = RequireHoistable("struct literal with an array field");
        var statements = new List<CStmt>();
        var result = _symbols.Declare(new Symbol
        {
            Name = "__anf" + _anfTempCounter++, Kind = SymKind.Var, Type = type,
        });
        statements.Add(new DeclStmt([new(result, new DefaultLit { Type = type })]));
        var reference = new VarRef(result) { Type = type, IsLValue = true };
        var written = new HashSet<string>(System.StringComparer.Ordinal);

        void Store(string name, CType fieldType, CExpr value)
        {
            var target = new Member(reference, name, false) { Type = fieldType, IsLValue = true };
            if (fieldType.Unqualified is CType.Array array)
            {
                if (array.Count is not { } count)
                    throw new IrUnsupportedException($"struct '{type.Name}': field '{name}' has no comptime length");
                if (count != 0)
                    statements.Add(new ExprStmt(new Call("memcpy", [target, value, ElementBytes(array.Element, count)])
                    { Type = new CType.Pointer(CType.Void) }));
            }
            else if (fieldType.Unqualified is CType.VoidType)
            {
                if (value is not DefaultLit and not ZigUndefinedValue) statements.Add(new ExprStmt(value));
            }
            else statements.Add(new ExprStmt(new Assign(null, target, value) { Type = fieldType }));
        }

        foreach (var item in fields)
        {
            var field = (Zig.FieldInit)item.Content!;
            var name = SourceMemberName(field.Arg1);
            var fieldType = _ir.StructFieldType(type, name)
                ?? throw new IrUnsupportedException($"struct '{type.Name}' has no field '{name}'");
            written.Add(name);
            if (fieldType.Unqualified is CType.Array && IsZeroArrayValue(field.Arg3)) continue;
            CExpr value;
            using (EnterFreshHoist())
            {
                value = LowerExprSink(field.Arg3, BitcastFieldLocation(type.Name, name, fieldType));
                statements.AddRange(_hoist!);
            }
            Store(name, fieldType, value);
        }

        var owner = _moduleGraph?.OwnerOfContainer(type.Name) ?? this;
        foreach (var field in _ir.StructFieldsOf(type.Name) ?? [])
        {
            if (written.Contains(field.Name)) continue;
            if (owner._structFieldDefaults.TryGetValue((type.Name, field.Name), out var declaration))
            {
                if (field.Type.Unqualified is CType.Array && IsZeroArrayValue(declaration)) continue;
                CExpr value;
                using (owner.EnterFreshHoist())
                {
                    value = owner.LowerFieldDefault(type.Name, field.Type, declaration, field.Name);
                    statements.AddRange(owner._hoist!);
                }
                Store(field.Name, field.Type, value);
            }
            else if (owner._reifiedFieldDefaults.TryGetValue((type.Name, field.Name), out var value))
                Store(field.Name, field.Type, value);
        }
        buffer.AddRange(statements);
        return reference;
    }
}
