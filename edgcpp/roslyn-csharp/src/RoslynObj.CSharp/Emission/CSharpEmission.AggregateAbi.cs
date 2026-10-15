using System.Globalization;
using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record AggregateLayout(RecordType Type, string NativeName, string Shape, IReadOnlyList<FieldLayout> Fields);
        private sealed record AggregateInvocation(RoutineType Signature, string Helper, string Adapter);
        private readonly List<AggregateLayout> aggregateLayouts = [];
        private readonly List<AggregateInvocation> aggregateInvocations = [];

        private static bool HasAggregateSignature(RoutineType signature) => Unwrap(signature.ReturnType) is RecordType ||
            signature.Parameters.Any(p => Unwrap(p.Type) is RecordType);

        private void ValidateNativeAggregateSignature(RoutineType signature)
        {
            ValidateCallingConvention(signature);
            ValidateOrdinaryRoutineRoles(signature);
            if (!signature.HasPrototype || signature.IsVariadic)
                throw Limit("Aggregate ABI gateways require a retained fixed current prototype.", "aggregate-abi");
            // First acceptance slice: R(R), or R(R (*)(R), R). Other
            // mechanically expressible bridge signatures remain unaccepted.
            if (Unwrap(signature.ReturnType) is not RecordType result || signature.Parameters.Count is not (1 or 2) ||
                !Unwrap(signature.Parameters[^1].Type).Equals(result))
                throw Limit("The exercised aggregate gateway requires a record result and one same-record value argument, optionally preceded by its callback pointer.", "aggregate-abi");
            if (signature.Parameters.Count == 2)
            {
                if (Unwrap(signature.Parameters[0].Type) is not PointerType { IsReference: false } pointer ||
                    Unwrap(pointer.PointeeType) is not RoutineType callback || !callback.HasPrototype || callback.IsVariadic ||
                    callback.Parameters.Count != 1 || !Unwrap(callback.ReturnType).Equals(result) || !Unwrap(callback.Parameters[0].Type).Equals(result))
                    throw Limit("The exercised aggregate gateway prefix is an ordinary pointer to the same fixed one-record callback signature.", "aggregate-abi");
                ValidateCallingConvention(callback);
                ValidateOrdinaryRoutineRoles(callback);
                ValidateAggregateBoundaryType(callback.ReturnType, false);
                ValidateAggregateBoundaryType(callback.Parameters[0].Type, false);
            }
            ValidateAggregateBoundaryType(signature.ReturnType, true);
            foreach (var parameter in signature.Parameters) ValidateAggregateBoundaryType(parameter.Type, false);
        }

        private void ValidateAggregateBoundaryType(CppType type, bool allowVoid)
        {
            ValidateAggregateQualifiers(type);
            if (Unwrap(type) is RecordType) _ = RequireAggregateLayout(type);
            else ValidateNativeScalarType(type, allowVoid);
        }

        private static void ValidateAggregateQualifiers(CppType type)
        {
            // Inspect retained wrappers AND their final raw descriptor before
            // scalar unwrapping can erase an execution-relevant qualification.
            for (var current = type; ;)
            {
                if (current.Details.IsAtomic || current.Details.SizeKind.Name == "upc_threads")
                    throw Limit("Atomic/UPC aggregate ABI storage requires a separate contract.", "aggregate-abi");
                if (current is QualifiedType qualified)
                {
                    var qualifiers = qualified.Qualification;
                    if (qualifiers.IsVolatile || qualifiers.IsAtomic || qualifiers.AddressSpace is not (null or 0) ||
                        qualifiers.IsShared || qualifiers.IsUpcStrict || qualifiers.IsUpcRelaxed ||
                        qualifiers.IsNear || qualifiers.IsFar || qualifiers.IsUnaligned)
                        throw Limit("Volatile/atomic/address-space/UPC or nonordinary aggregate ABI qualifiers require a separate contract.", "aggregate-abi");
                }
                if (current is not TypeReference reference) return;
                current = reference.UnderlyingType;
            }
        }

        private AggregateLayout RequireAggregateLayout(CppType type)
        {
            ValidateAggregateQualifiers(type);
            var record = Unwrap(type) as RecordType ?? throw Limit("The aggregate gateway needs a captured record.", "aggregate-abi");
            if (type.Size != record.Size || type.Alignment != record.Alignment)
                throw Limit("Decorated aggregate size/alignment differs from its captured ordinary representation.", "aggregate-abi");
            var existing = aggregateLayouts.FirstOrDefault(a => a.Type.Equals(record));
            if (existing is not null) return existing;
            if (record.Record.AnyVolatileMember || record.HasVirtualFunctions || record.HasVirtualBases || record.Bases.Count != 0 ||
                record.Record.ContainsFlexibleArrayMember || record.Record.IsTransparent)
                throw Limit("Aggregate ABI gateways exclude volatile members, bases, virtual dispatch and flexible/transparent storage.", "aggregate-abi");
            var layout = record.Layout;
            if (layout is null || !layout.IsComplete || record.Alignment is not { } alignment || alignment < 1 || alignment > 8)
                throw Limit("Aggregate ABI gateway requires a complete separately exercised layout with alignment at most eight.", "aggregate-abi");
            var fields = layout.Fields.ToArray();
            if (fields.Length != record.Fields.Count || fields.Any(f => f.Field.IsBitField || f.Field.IsGeneratedBase ||
                f.Field.IsGeneratedVirtualPointer == true || f.Field.HasNoUniqueAddressAttribute || f.Offset is null || f.BitOffset is not (null or { IsZero: true })))
                throw Limit("Aggregate ABI gateway does not support bitfields, overlapping struct members or generated object-model fields.", "aggregate-abi");
            foreach (var field in fields)
            {
                ValidateAggregateQualifiers(field.Field.Type);
                ValidateNativeScalarType(field.Field.Type, false);
            }
            var scalarNames = fields.Select(f => TypeName(f.Field.Type)).ToArray();
            var offsets = fields.Select(f => f.Offset!.Value).ToArray();
            string? shape = null;
            // These are representation cases with independent genuine fixtures,
            // not a general platform aggregate classifier. The native compiler
            // alone places each concrete signature in host registers/stack.
            if (record.Kind.Name == "tk_struct" && !record.IsPacked)
            {
                if (record.Size == 8 && alignment == 4 && scalarNames.SequenceEqual(["int", "int"]) && offsets.SequenceEqual([0, 4])) shape = "integer-pair";
                else if (record.Size == 16 && alignment == 8 && scalarNames.SequenceEqual(["double", "double"]) && offsets.SequenceEqual([0, 8])) shape = "sse-pair";
                else if (record.Size == 16 && alignment == 8 && scalarNames.SequenceEqual(["int", "double"]) && offsets.SequenceEqual([0, 8])) shape = "mixed-pair";
                else if (record.Size == 24 && alignment == 8 && scalarNames.SequenceEqual(["long", "long", "long"]) && offsets.SequenceEqual([0, 8, 16])) shape = "memory-triple";
            }
            // Packed/union cases remain separate from ordinary struct admission.
            else if (record.Kind.Name == "tk_struct" && record.IsPacked && record.Size == 9 && alignment == 1 &&
                scalarNames.SequenceEqual(["byte", "double"]) && offsets.SequenceEqual([0, 1])) shape = "packed-byte-double";
            else if (record.Kind.Name == "tk_union" && !record.IsPacked && record.Size == 8 && alignment == 8 &&
                scalarNames.SequenceEqual(["ulong", "double"]) && offsets.SequenceEqual([0, 0])) shape = "union-uint64-double";
            if (shape is null) throw Limit("This aggregate native representation has no separately exercised concrete gateway shape.", "aggregate-abi");
            _ = TypeName(record);
            var result = new AggregateLayout(record, InternalName("__aggregate_" + (aggregateLayouts.Count + 1)), shape, fields);
            aggregateLayouts.Add(result);
            return result;
        }

        private string AggregateCall(RoutineType signature, FunctionSymbol? function, IReadOnlyList<CppType> argumentTypes,
            IReadOnlyList<string> argumentValues, string? address = null)
        {
            if (function is not null)
            {
                function = CanonicalFunction(function);
                if (functions.Contains(function))
                {
                    // Ordinary managed calls have no host ABI boundary. Keep
                    // their selected value/parameter contract separate from
                    // the narrowly exercised native mirror whitelist.
                    ValidateOrdinaryRoutineRoles(signature);
                    ValidateOrdinaryRoutineRoles(function.Signature);
                    if (signature.IsVariadic || function.Signature.IsVariadic ||
                        signature.Parameters.Count != function.Signature.Parameters.Count ||
                        !SameAggregateBoundaryType(signature.ReturnType, function.ReturnType) ||
                        !signature.Parameters.Zip(function.Signature.Parameters).All(p => SameAggregateBoundaryType(p.First.Type, p.Second.Type)))
                        throw Limit("Selected managed aggregate call needs an explicit ordinary parameter/result view.", "call");
                    ValidateAggregateArguments(signature, argumentTypes, argumentValues);
                    return $"{OwnedName(function)}({string.Join(", ", argumentValues)})";
                }
            }
            // Imports, unknown native addresses, and address-exposed entries
            // alone require the separately proven native gateway shapes.
            ValidateNativeAggregateSignature(signature);
            ValidateAggregateArguments(signature, argumentTypes, argumentValues);
            if (function is not null)
            {
                RequireCallable(function);
                address ??= FunctionAddress(function);
            }
            if (address is null) throw Limit("An aggregate call requires its actual retained function address.", "aggregate-abi");
            var item = aggregateInvocations.FirstOrDefault(a => a.Signature.Equals(signature));
            if (item is null)
            {
                _ = RequireGeneratedAbiLibrary();
                item = new(signature, InternalName("__InvokeAggregate_" + (aggregateInvocations.Count + 1)), InternalName("__outbound_aggregate_" + (aggregateInvocations.Count + 1)));
                aggregateInvocations.Add(item);
            }
            return $"{Owner}.{item.Helper}({address}{(argumentValues.Count == 0 ? "" : ", " + string.Join(", ", argumentValues))})";
        }

        private void ValidateAggregateArguments(RoutineType signature, IReadOnlyList<CppType> argumentTypes, IReadOnlyList<string> argumentValues)
        {
            if (signature.Parameters.Count != argumentTypes.Count || argumentTypes.Count != argumentValues.Count)
                throw Limit("Aggregate call requires the exact captured fixed argument association.", "call");
            for (var i = 0; i < argumentTypes.Count; ++i)
                if (!SameAggregateBoundaryType(argumentTypes[i], signature.Parameters[i].Type))
                    throw Limit("Aggregate call argument views need an explicit accepted representation adapter.", "call");
        }

        private bool SameAggregateBoundaryType(CppType left, CppType right)
        {
            if (Unwrap(left) is RecordType || Unwrap(right) is RecordType)
                return Unwrap(left).Equals(Unwrap(right)) && left.Size == right.Size && left.Alignment == right.Alignment;
            return SameScalar(left, right);
        }

        private bool SameAggregateSignature(RoutineType left, RoutineType right) => !left.IsVariadic && !right.IsVariadic &&
            left.Parameters.Count == right.Parameters.Count && SameAggregateBoundaryType(left.ReturnType, right.ReturnType) &&
            left.Parameters.Zip(right.Parameters).All(p => SameAggregateBoundaryType(p.First.Type, p.Second.Type));

        private string AggregateBridgeType(CppType type) => Unwrap(type) is RecordType ? TypeName(type) + "*" : TypeName(type);
        private string AggregateBridgeReturn(RoutineType signature) => Unwrap(signature.ReturnType) is RecordType ? "void" : TypeName(signature.ReturnType);
        private IEnumerable<string> AggregateBridgeParameterTypes(RoutineType signature) =>
            (Unwrap(signature.ReturnType) is RecordType ? new[] { TypeName(signature.ReturnType) + "*" } : [])
            .Concat(signature.Parameters.Select(p => AggregateBridgeType(p.Type)));
        private string AggregateBridgeFunctionPointer(RoutineType signature) => "delegate* unmanaged[Cdecl]<" +
            string.Join(", ", AggregateBridgeParameterTypes(signature).Append(AggregateBridgeReturn(signature))) + ">";
        private IEnumerable<string> AggregateBridgeFormals(RoutineType signature) =>
            (Unwrap(signature.ReturnType) is RecordType ? new[] { TypeName(signature.ReturnType) + "* result" } : [])
            .Concat(signature.Parameters.Select((p, i) => $"{AggregateBridgeType(p.Type)} argument{i}"));

        private void EmitAggregateSupport(CodeWriter initialization)
        {
            foreach (var call in aggregateInvocations)
            {
                var signature = call.Signature;
                var writer = new CodeWriter();
                var aggregateResult = Unwrap(signature.ReturnType) is RecordType;
                writer.Line($"[DllImport(\"{NativeAbiLibraryName}\", EntryPoint = \"{call.Adapter}\", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");
                writer.Line($"private static extern {AggregateBridgeReturn(signature)} {call.Adapter}(nint address{(AggregateBridgeFormals(signature).Any() ? ", " + string.Join(", ", AggregateBridgeFormals(signature)) : "")});");
                writer.Line();
                writer.Line($"private static {TypeName(signature.ReturnType)} {call.Helper}(nint address{string.Concat(signature.Parameters.Select((p, i) => $", {TypeName(p.Type)} argument{i}"))})");
                writer.Open();
                foreach (var entry in functionAddresses.Where(a => a.AggregateEntry is not null && SameAggregateSignature(a.Function.Signature, signature)))
                {
                    writer.Line($"if (address == {Owner}.{entry.Field})"); writer.Open();
                    writer.Line($"{(TypeName(signature.ReturnType) == "void" ? "" : "return ")}{OwnedName(entry.Function)}({string.Join(", ", signature.Parameters.Select((_, i) => $"argument{i}"))});");
                    if (TypeName(signature.ReturnType) == "void") writer.Line("return;");
                    writer.Close();
                }
                if (aggregateResult) writer.Line($"{TypeName(signature.ReturnType)} result;");
                var args = new[] { "address" }.Concat(aggregateResult ? new[] { "&result" } : [])
                    .Concat(signature.Parameters.Select((p, i) => (Unwrap(p.Type) is RecordType ? "&" : "") + $"argument{i}"));
                writer.Line($"{(aggregateResult || TypeName(signature.ReturnType) == "void" ? "" : "return ")}{Owner}.{call.Adapter}({string.Join(", ", args)});");
                if (aggregateResult) writer.Line("return result;");
                writer.Close();
                methods.Append(writer.TextWithIndent(1)).AppendLine();
            }
            foreach (var address in functionAddresses.Where(a => a.AggregateEntry is not null))
            {
                var signature = address.Function.Signature;
                var pointerType = AggregateBridgeFunctionPointer(signature);
                var writer = new CodeWriter();
                writer.Line($"[DllImport(\"{NativeAbiLibraryName}\", EntryPoint = \"{address.AggregateRegistration}\", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");
                writer.Line($"private static extern void {address.AggregateRegistration}({pointerType} callback);");
                writer.Line();
                writer.Line("[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]");
                writer.Line($"private static {AggregateBridgeReturn(signature)} {address.AggregateBridge}({string.Join(", ", AggregateBridgeFormals(signature))})");
                writer.Open();
                var arguments = signature.Parameters.Select((p, i) => (Unwrap(p.Type) is RecordType ? "*" : "") + $"argument{i}");
                var invocation = $"{OwnedName(address.Function)}({string.Join(", ", arguments)})";
                writer.Line(Unwrap(signature.ReturnType) is RecordType ? $"*result = {invocation};" : $"{(TypeName(signature.ReturnType) == "void" ? "" : "return ")}{invocation};");
                writer.Close();
                methods.Append(writer.TextWithIndent(1)).AppendLine();
                initialization.Line($"{Owner}.{address.AggregateRegistration}(({pointerType})&{address.AggregateBridge});");
            }
        }

        private string NativeAggregateType(CppType type) => Unwrap(type) is RecordType ? RequireAggregateLayout(type).NativeName : NativeScalarType(type);
        private string NativeAggregateBridgeType(CppType type) => Unwrap(type) is RecordType ? "void *" : NativeScalarType(type);
        private string NativeAggregateBridgeReturn(RoutineType signature) => Unwrap(signature.ReturnType) is RecordType ? "void" : NativeScalarType(signature.ReturnType);
        private IEnumerable<string> NativeAggregateBridgeTypes(RoutineType signature) =>
            (Unwrap(signature.ReturnType) is RecordType ? new[] { "void *" } : []).Concat(signature.Parameters.Select(p => NativeAggregateBridgeType(p.Type)));

        private void AppendAggregateNativeSource(StringBuilder source)
        {
            foreach (var layout in aggregateLayouts)
            {
                var record = layout.Type;
                source.Append("typedef ").Append(record.Kind.Name == "tk_union" ? "union" : "struct");
                if (record.IsPacked) source.Append(" __attribute__((packed))");
                source.AppendLine(" {");
                foreach (var field in layout.Fields.Select((f, i) => (f, i)))
                    source.Append("    ").Append(NativeScalarType(field.f.Field.Type)).Append(" field").Append(field.i).AppendLine(";");
                source.Append("} ").Append(layout.NativeName).AppendLine(";");
                source.Append("_Static_assert(sizeof(").Append(layout.NativeName).Append(") == ").Append(record.Size?.ToString(CultureInfo.InvariantCulture)).AppendLine(", \"captured aggregate size\");");
                source.Append("_Static_assert(_Alignof(").Append(layout.NativeName).Append(") == ").Append(record.Alignment?.ToString(CultureInfo.InvariantCulture)).AppendLine(", \"captured aggregate alignment\");");
                foreach (var field in layout.Fields.Select((f, i) => (f, i)))
                    source.Append("_Static_assert(offsetof(").Append(layout.NativeName).Append(", field").Append(field.i).Append(") == ").Append(field.f.Offset?.ToString(CultureInfo.InvariantCulture)).AppendLine(", \"captured aggregate field offset\");");
                source.AppendLine();
            }
            foreach (var address in functionAddresses.Where(a => a.AggregateEntry is not null))
            {
                var signature = address.Function.Signature;
                var bridge = address.AggregateBridge!;
                var aggregateResult = Unwrap(signature.ReturnType) is RecordType;
                source.Append("typedef ").Append(NativeAggregateBridgeReturn(signature)).Append(" (*").Append(bridge).Append("_type)(")
                    .Append(string.Join(", ", NativeAggregateBridgeTypes(signature).DefaultIfEmpty("void"))).AppendLine(");");
                source.Append("static ").Append(bridge).Append("_type ").Append(bridge).AppendLine("_callback;");
                source.Append("ABI_EXPORT void ").Append(address.AggregateRegistration).Append('(').Append(bridge).AppendLine("_type callback) {")
                    .Append("    ").Append(bridge).AppendLine("_callback = callback;\n}");
                source.Append("ABI_EXPORT ").Append(NativeAggregateType(signature.ReturnType)).Append(' ').Append(address.AggregateEntry).Append('(')
                    .Append(string.Join(", ", signature.Parameters.Select((p, i) => $"{NativeAggregateType(p.Type)} argument{i}").DefaultIfEmpty("void"))).AppendLine(") {");
                source.Append("    if (!").Append(bridge).AppendLine("_callback) abort();");
                if (aggregateResult) source.Append("    ").Append(NativeAggregateType(signature.ReturnType)).AppendLine(" result;");
                source.Append("    ").Append(aggregateResult || NativeScalarReturnIsVoid(signature) ? "" : "return ").Append(bridge).Append("_callback(")
                    .Append(string.Join(", ", (aggregateResult ? new[] { "(void *)&result" } : []).Concat(signature.Parameters.Select((p, i) =>
                        (Unwrap(p.Type) is RecordType ? "(void *)&" : "") + $"argument{i}")))).AppendLine(");");
                if (aggregateResult) source.AppendLine("    return result;");
                source.AppendLine("}\n");
            }
            foreach (var call in aggregateInvocations)
            {
                var signature = call.Signature;
                var aggregateResult = Unwrap(signature.ReturnType) is RecordType;
                var typeName = call.Adapter + "_type";
                source.Append("typedef ").Append(NativeAggregateType(signature.ReturnType)).Append(" (*").Append(typeName).Append(")(")
                    .Append(string.Join(", ", signature.Parameters.Select(p => NativeAggregateType(p.Type)).DefaultIfEmpty("void"))).AppendLine(");");
                source.Append("ABI_EXPORT ").Append(NativeAggregateBridgeReturn(signature)).Append(' ').Append(call.Adapter).Append("(void *address");
                if (aggregateResult) source.Append(", void *result");
                foreach (var parameter in signature.Parameters.Select((p, i) => (p, i)))
                    source.Append(", ").Append(NativeAggregateBridgeType(parameter.p.Type)).Append(" argument").Append(parameter.i);
                source.AppendLine(") {");
                source.Append("    ").Append(aggregateResult ? $"*({NativeAggregateType(signature.ReturnType)} *)result = " : NativeScalarReturnIsVoid(signature) ? "" : "return ")
                    .Append("((").Append(typeName).Append(")address)(")
                    .Append(string.Join(", ", signature.Parameters.Select((p, i) => Unwrap(p.Type) is RecordType ? $"*({NativeAggregateType(p.Type)} *)argument{i}" : $"argument{i}"))).AppendLine(");\n}\n");
            }
        }

        private static bool NativeScalarReturnIsVoid(RoutineType signature) => Unwrap(signature.ReturnType).Kind.Name == "tk_void";
    }
}
