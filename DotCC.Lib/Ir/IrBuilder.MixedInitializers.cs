#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private StructInit BuildStructMixed(CType type, IReadOnlyList<Init> items)
    {
        var members = new List<FieldInit>();
        var fields = StructFieldsOf(type).Where(field => !field.IsAnonBitField).ToList();
        string[]? cursor = fields.Count == 0 ? null : new[] { fields[0].Name };
        foreach (var item in items)
        {
            if (item is InitMember designated)
            {
                SetDesignatedMember(type, members, designated.Path, designated.Value);
                cursor = Next(type, designated.Path.Split('.'));
            }
            else
            {
                if (cursor is null) throw new IrUnsupportedException("too many initializers for struct/union");
                SetValue(type, members, cursor, item);
                cursor = Next(type, cursor);
            }
        }
        return new StructInit(members) { Type = type };

        string[]? Next(CType owner, string[] path)
        {
            var ownerFields = StructFieldsOf(owner).Where(field => !field.IsAnonBitField).ToList();
            int index = ownerFields.FindIndex(field => field.Name == path[0]);
            if (index < 0) throw new IrUnsupportedException("mixed initializer continuation through an anonymous promoted member");
            if (path.Length > 1 && Next(ownerFields[index].Type, path[1..]) is { } nested)
                return new[] { path[0] }.Concat(nested).ToArray();
            if (_structIsUnion.GetValueOrDefault(((CType.Named)owner.Unqualified).Name) || index + 1 == ownerFields.Count)
                return null;
            return new[] { ownerFields[index + 1].Name };
        }

        void SetValue(CType owner, List<FieldInit> values, string[] path, Init initializer)
        {
            var field = StructFieldsOf(owner).First(field => field.Name == path[0]);
            if (_structIsUnion.GetValueOrDefault(((CType.Named)owner.Unqualified).Name)
                && values.Count > 0 && values[0].Name != field.Name) values.Clear();
            int index = values.FindIndex(value => value.Name == field.Name);
            CExpr result;
            if (path.Length > 1)
            {
                var nested = index >= 0 && values[index].Value is StructInit previous
                    ? previous.Members.ToList() : new List<FieldInit>();
                SetValue(field.Type, nested, path[1..], initializer);
                result = new StructInit(nested) { Type = field.Type };
            }
            else result = BuildFieldInitializer(field.Type, initializer);
            var replacement = new FieldInit(field.Name, field.Type, result);
            if (index < 0) values.Add(replacement);
            else values[index] = replacement;
        }
    }
}
