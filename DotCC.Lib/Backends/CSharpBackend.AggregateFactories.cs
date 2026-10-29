using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private string BoundedStructArrayInitText(StructInit initializer)
    {
        var fields = new StringBuilder();
        var arguments = new List<string>();
        var stores = new StringBuilder();
        void Store(string destination, CType type, CExpr value)
        {
            var field = "p" + arguments.Count;
            fields.Append("        public ").Append(Cs(type)).Append(' ').Append(field).Append(";\n");
            string argument = Coerced(value, type);
            if (_canHoist)
            {
                // Consume the WHOLE value before rendering the next operand.
                // Its comma/statement-expression prefixes are already pending;
                // delaying its final value until pack construction would let the
                // next operand's prefixes interleave with this evaluation.
                string local = "__aggregateValue" + _clCounter++;
                _pending.Add(Cs(type) + " " + local + " = " + argument);
                argument = local;
            }
            // No-hoist/lazy/loop contexts retain inline evaluation at the
            // original site. Pointer, CBool, callback and struct values remain
            // typed locals/fields, without delegates or generic pointer carriers.
            arguments.Add(field + " = " + argument);
            stores.Append("        ").Append(destination).Append(" = args.").Append(field).Append(";\n");
        }
        foreach (var member in initializer.Members)
        {
            var destination = "destination->" + EmitHelpers.Id(member.Name);
            if (member.Value is not InlineArrayInit array)
            {
                Store(destination, member.FieldType, member.Value);
                continue;
            }
            var element = Cs(array.Element);
            var pointer = IsFixedBufferType(element) ? destination : "((" + element + "*)&" + destination + ")";
            // These paths prove the complete array before registering any data.
            // A mixed/effectful array is captured entirely in the argument pack;
            // it never becomes a partially fabricated constant byte payload.
            if (EmitPrimitiveCopies(stores, pointer, array.Element, array.Elems, zeroed: true)) continue;
            // Small constant record tables still benefit here: the factory was
            // already required for inline storage. Ordinary global tables keep
            // their existing 1024-byte aggregate-data threshold.
            if (TryAggregateBytes(array.Element, array.Elems, out var bytes, minimumBytes: 1))
            {
                AppendAggregateDataCopies(stores, pointer, array.Element, bytes);
                continue;
            }
            for (int index = 0; index < array.Elems.Count; index++)
                Store(pointer + "[" + index + "]", array.Element, array.Elems[index]);
        }
        bool packedArguments = arguments.Count != 0;
        var type = Cs(initializer.Type);
        var helpers = new StringBuilder();
        string calls = AggregateFactoryFillHelpers(helpers, stores.ToString(), type, packedArguments);
        var body = new StringBuilder();
        if (packedArguments)
            body.Append("    public struct Arguments\n    {\n").Append(fields).Append("    }\n");
        body.Append("    public static ").Append(type).Append(" Create(")
            .Append(packedArguments ? "in Arguments args" : "").Append(")\n    {\n        ")
            .Append(type).Append(" result = default;\n        ").Append(type).Append("* destination = &result;\n")
            .Append(calls).Append("        return result;\n    }\n").Append(helpers);
        string name = "__DotccAggregateInit_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body.ToString())));
        _aggregateFactories.TryAdd(name, "internal static unsafe class " + name + "\n{\n" + body + "}\n\n");
        return name + ".Create(" + (packedArguments
            ? "new " + name + ".Arguments { " + string.Join(", ", arguments) + " }" : "") + ")";
    }

    private static string AggregateFactoryFillHelpers(StringBuilder helpers, string stores, string type, bool arguments)
    {
        // Bound helper bodies and their call tree without making array length
        // part of a method signature. Create has <=1 parameter; every fill has
        // a destination pointer and, only when needed, one readonly pack borrow.
        int level = 0;
        string parameters = type + "* destination" + (arguments ? ", in Arguments args" : "");
        string values = "destination" + (arguments ? ", in args" : "");
        do
        {
            var part = new StringBuilder();
            var calls = new StringBuilder();
            int index = 0;
            void Flush()
            {
                if (part.Length == 0) return;
                string name = "Fill_" + level + "_" + index++;
                helpers.Append("    private static void ").Append(name).Append('(').Append(parameters)
                    .Append(")\n    {\n").Append(part).Append("\n    }\n");
                calls.Append("        ").Append(name).Append('(').Append(values).Append(");\n");
                part.Clear();
            }
            foreach (var statement in Compiler.InitializerStatements(stores))
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
}
