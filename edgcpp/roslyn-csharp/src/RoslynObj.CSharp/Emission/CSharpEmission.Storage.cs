using System.Globalization;
using System.Numerics;
using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record StorageItem(SemanticEntity Entity, CppType Type, Constant? Initializer, string OwnerField, bool ThreadLocal);
        private readonly Dictionary<SemanticEntity, StorageItem> storage = [];
        private readonly List<StorageItem> storageOrder = [];
        private readonly List<ArrayType> arrays = [];
        private readonly List<(bool ThreadLocal, string Assignment)> addressFixups = [];
        private string? threadInitializationOwner;

        private bool IsSelectedConstantStorageCompanion(VariableSymbol variable)
        {
            if (options.ManagedProgramSelection is not { } program || !program.ConstantStorageCompanions.Contains(variable)) return false;
            if (program.PromotedConstantStoragePolicy != "captured-external-comdat-constant-manifest-order")
                throw Limit("A constant-storage companion requires its explicit selected storage policy.", "storage-selection");
            return true;
        }

        private bool TryAdmitSelectedConstantStorageCompanion(VariableSymbol variable)
        {
            if (!IsSelectedConstantStorageCompanion(variable)) return false;
            var representative = CanonicalVariable(variable);
            if (!IsSelectedConstantStorageCompanion(representative) ||
                representative.Details.ConstantInitializer is not { Value: StringValue } initializer ||
                representative.Initializer is not null || representative.LocalStaticInitializations.Count != 0)
                throw Limit("A selected constant-storage companion lost its canonical static constant definition.", "storage-selection");
            if (storage.TryGetValue(representative, out var existing) &&
                (!existing.Type.Equals(representative.Type) || existing.Initializer?.Equals(initializer) != true))
                throw Limit("A selected constant-storage companion already has a different allocation or initializer.", "storage-selection");
            // The immutable selection admitted these exact members. Only the
            // representative supplies an allocation and static initializer;
            // raw local declarations and occurrence types keep their identity.
            AddStorage(representative, representative.Type, initializer, representative.Name ?? "constant_storage");
            staticLocals.Add(variable);
            staticLocals.Add(representative);
            return true;
        }

        private void AdmitGlobal(VariableSymbol variable)
        {
            if (variable.Details.IsNonreal || variable.Details.IsPrototypeInstantiation || variable.Details.IsTemplateVariable) return;
            if (variable.SymbolDetails.Storage?.Name == "sc_extern") return;
            if (TryAdmitSelectedConstantStorageCompanion(variable)) return;
            if (TryAdmitConstantThreadStorage(variable)) return;
            if (TryAdmitStaticLocal(variable)) return;
            if (variable.IsThreadLocal || variable.LocalStaticInitializations.Count != 0)
                throw Limit($"Storage '{variable.Name}' requires thread/local-static initialization support.", "storage");
            if (variable.Details.IsWeak || variable.Details.IsCommon || variable.Details.IsGnuAlias || variable.Details.IsWeakref || variable.Details.AliasedVariable is not null)
                throw Limit($"Storage '{variable.Name}' requires an explicit weak/common/alias selection policy.", "link-policy");
            if (variable.Initializer is { } initializer && initializer.Kind.Name != "dik_constant" &&
                (!SelectedStartupEnabled || !ManagedAmbientPolicy.IsInertDescriptor(initializer)))
                throw Limit($"Storage '{variable.Name}' has a dynamic initializer '{initializer.Kind}'.", "initialization");
            AddStorage(variable, variable.Type, variable.ConstantInitializer ?? variable.Initializer?.Constant, variable.Name ?? "storage");
        }

        private void AddStorage(SemanticEntity entity, CppType type, Constant? initializer, string candidate)
        {
            if (storage.ContainsKey(entity)) return;
            ValidateValueAddress(type);
            _ = TypeName(type);
            _ = StorageSize(type);
            Name(entity, candidate, globalNames);
            var item = new StorageItem(entity, type, initializer, InternalName("__storage_" + names[entity].TrimStart('@')),
                entity is VariableSymbol variable && HasThreadStorage(variable));
            storage.Add(entity, item);
            storageOrder.Add(item);
        }

        private void EmitGlobalStorage(StringBuilder fields, CodeWriter initialization)
        {
            var constants = new CodeWriter();
            var threadConstants = new CodeWriter();
            var threadOwnerParameter = InternalName("__threadStorage");
            for (var index = 0; index < storageOrder.Count; ++index)
            {
                var item = storageOrder[index];
                if (item.Initializer is { } constant)
                {
                    nativeDataInitializerOwner = item.Entity;
                    threadInitializationOwner = item.ThreadLocal ? threadOwnerParameter : null;
                    try
                    {
                        var pointer = StorageInitializationAddress(item);
                        WriteConstant(item.Type, $"(byte*)({pointer})", constant.Value,
                            item.ThreadLocal ? threadConstants : constants, true, $"*({pointer})");
                    }
                    finally { nativeDataInitializerOwner = null; threadInitializationOwner = null; }
                }
            }
            // Drain constant discovery first: an address may introduce literal storage.
            // Each object keeps its own allocation, captured alignment and identity.
            var processItems = storageOrder.Where(item => !item.ThreadLocal).ToArray();
            var threadItems = storageOrder.Where(item => item.ThreadLocal).ToArray();
            if (processItems.Length != 0)
            {
                var ownerType = InternalName("__ProgramStorage");
                var ownerField = InternalName("__programStorage");
                var allocate = InternalName("__AllocateAndZeroStorage");
                var populate = InternalName("__PopulateConstantStorage");
                var fixup = InternalName("__ApplyStorageAddressFixups");
                EmitStorageOwner(fields, processItems, ownerType);
                fields.Append("    private static readonly ").Append(ownerType).Append(' ').Append(ownerField).AppendLine(";");
                EmitStorageProperties(fields, processItems, ownerField);
                fields.Append("    private static ").Append(ownerType).Append(' ').Append(allocate).AppendLine("() => new();").AppendLine();
                EmitStoragePhase(fields, populate, constants);
                EmitStoragePhase(fields, fixup, AddressFixupWriter(false));
                initialization.Line($"{ownerField} = {allocate}();");
                initialization.Line($"{populate}();");
                initialization.Line($"{fixup}();");
            }
            if (threadItems.Length != 0)
            {
                var ownerType = InternalName("__ThreadStorage");
                var ownerField = InternalName("__threadStorageOwner");
                var current = InternalName("__CurrentThreadStorage");
                var create = InternalName("__CreateThreadStorage");
                var allocate = InternalName("__AllocateAndZeroThreadStorage");
                var populate = InternalName("__PopulateThreadConstants");
                var fixup = InternalName("__ApplyThreadAddressFixups");
                EmitStorageOwner(fields, threadItems, ownerType);
                fields.AppendLine("    [ThreadStatic]")
                    .Append("    private static ").Append(ownerType).Append("? ").Append(ownerField).AppendLine(";");
                fields.Append("    private static ").Append(ownerType).Append(' ').Append(current)
                    .Append(" => ").Append(ownerField).Append(" ??= ").Append(create).AppendLine("();").AppendLine();
                EmitStorageProperties(fields, threadItems, current);
                fields.Append("    private static ").Append(ownerType).Append(' ').Append(allocate).AppendLine("() => new();").AppendLine();
                EmitStoragePhase(fields, populate, threadConstants, ownerType, threadOwnerParameter);
                EmitStoragePhase(fields, fixup, AddressFixupWriter(true), ownerType, threadOwnerParameter);
                fields.Append("    private static ").Append(ownerType).Append(' ').Append(create).AppendLine("()\n    {")
                    .Append("        var ").Append(threadOwnerParameter).Append(" = ").Append(allocate).AppendLine("();")
                    .Append("        ").Append(populate).Append('(').Append(threadOwnerParameter).AppendLine(");")
                    .Append("        ").Append(fixup).Append('(').Append(threadOwnerParameter).AppendLine(");")
                    .Append("        return ").Append(threadOwnerParameter).AppendLine(";\n    }").AppendLine();
            }
        }

        private string StorageInitializationAddress(StorageItem item)
            => item.ThreadLocal && threadInitializationOwner is { } owner
                ? $"({TypeName(item.Type)}*){owner}.{item.OwnerField}.Pointer" : OwnedName(item.Entity);

        private void EmitStorageOwner(StringBuilder fields, IReadOnlyList<StorageItem> items, string ownerType)
        {
            fields.Append("    private sealed class ").Append(ownerType).AppendLine("\n    {");
            foreach (var item in items)
            {
                var alignment = item.Type.Alignment ?? throw Limit("Global storage alignment is absent.", "storage");
                if (item.Entity is VariableSymbol variable && variable.Details.ExplicitAlignment is { } explicitAlignment)
                    alignment = BigInteger.Max(alignment, explicitAlignment);
                if (alignment <= 0 || alignment > int.MaxValue) throw Limit("Global alignment is unsupported.", "storage");
                fields.Append("        internal readonly PinnedStorage ").Append(item.OwnerField)
                    .Append(" = new(").Append(StorageSize(item.Type)).Append(", ").Append(alignment).AppendLine(");");
            }
            fields.AppendLine("    }").AppendLine();
        }

        private void EmitStorageProperties(StringBuilder fields, IReadOnlyList<StorageItem> items, string owner)
        {
            foreach (var item in items)
                fields.Append("    public static ").Append(TypeName(item.Type)).Append("* ").Append(names[item.Entity])
                    .Append(" => (").Append(TypeName(item.Type)).Append("*)").Append(owner).Append('.')
                    .Append(item.OwnerField).AppendLine(".Pointer;");
            fields.AppendLine();
        }

        private CodeWriter AddressFixupWriter(bool threadLocal)
        {
            var writer = new CodeWriter();
            foreach (var fixup in addressFixups.Where(fixup => fixup.ThreadLocal == threadLocal)) writer.Line(fixup.Assignment);
            return writer;
        }

        private static void EmitStoragePhase(StringBuilder fields, string name, CodeWriter body,
            string? parameterType = null, string? parameterName = null)
        {
            fields.Append("    private static void ").Append(name).Append('(');
            if (parameterType is not null) fields.Append(parameterType).Append(' ').Append(parameterName);
            fields.AppendLine(")\n    {").Append(body.TextWithIndent(2)).AppendLine("    }").AppendLine();
        }

        private void WriteConstant(CppType type, string destination, ConstantValue value, CodeWriter writer, bool deferAddress,
            string? typedDestination = null)
        {
            type = Unwrap(type);
            if (value is StringValue text)
            {
                if (text.UnitKind.Name != "target_bytes" || text.StorageUnitBits != 8)
                    throw Limit("String materialization requires exact target-byte units.", "constant");
                if (text.Length != text.StorageUnits.Count || text.Length > StorageSize(type) || text.StorageUnits.Any(unit => unit > byte.MaxValue))
                    throw Limit("String target units do not exactly match the retained length/storage.", "constant");
                for (var index = 0; index < text.StorageUnits.Count; ++index)
                    writer.Line($"({destination})[{index}] = {text.StorageUnits[index]};");
                return;
            }
            if (value is AggregateValue aggregate)
            {
                var position = 0;
                var fields = type is RecordType record
                    ? record.Layout!.Fields.Where(f => !f.Field.Details.IsBitField || f.Field.Name is not null && f.BitWidth > 0).ToArray()
                    : [];
                foreach (var element in aggregate.Elements)
                {
                    if (element.Value is ConstantValue<DesignatorValueDetails> designator)
                    {
                        if (type is ArrayType && designator.Payload.IndexValue is { } arrayIndex) position = checked((int)arrayIndex);
                        else if (type is RecordType && designator.Payload.Field is { } field)
                        {
                            position = Array.FindIndex(fields, f => f.Field.Equals(field));
                            if (position < 0) throw Limit("Resolved aggregate designator has no selected field.", "constant");
                        }
                        else throw Limit("Generic aggregate designators require retained resolved placement.", "constant");
                        continue;
                    }
                    if (element.Value is ConstantValue<RepeatValueDetails> repeat)
                    {
                        if (repeat.Payload.MultidimensionalTailNotRepeated) throw Limit("Multidimensional partial repeat requires explicit aggregate planning.", "constant");
                        for (BigInteger count = 0; count < repeat.Payload.Count; ++count)
                            WriteElement(repeat.Payload.Value);
                    }
                    else WriteElement(element.Value);
                }
                return;

                void WriteElement(ConstantValue child)
                {
                    if (type is ArrayType array)
                    {
                        if (array.Bound is not { } bound || position < 0 || position >= bound)
                            throw Limit("Array aggregate placement has no complete selected bound.", "constant");
                        WriteConstant(array.ElementType, $"({destination} + {position * StorageSize(array.ElementType)})", child, writer, deferAddress);
                    }
                    else if (type is RecordType)
                    {
                        if ((uint)position >= fields.Length) throw Limit("Record aggregate placement exceeds retained initializable fields.", "constant");
                        var field = fields[position];
                        if (field.Field.Details.IsBitField)
                        {
                            if (child is not IntegerValue integer) throw Limit("A bitfield initializer is not a selected integer value.", "constant");
                            var (offset, bit, width) = BitCoordinates(field);
                            writer.Line($"TargetBitField.Write({destination}, {offset}, {bit}, {width}, unchecked((ulong)({IntegerLiteral(integer.Value, field.Field.Type)})));");
                        }
                        else WriteConstant(field.Field.Type, $"({destination} + {field.Offset})", child, writer, deferAddress);
                    }
                    else throw Limit("An aggregate value does not have selected aggregate storage.", "constant");
                    ++position;
                }
            }
            var expression = value switch
            {
                IntegerValue integer => IntegerLiteral(integer.Value, type),
                FloatingValue floating => FloatingLiteral(floating, type),
                AddressValue address => AddressExpression(address, type),
                _ => throw Limit($"Stored constant '{value.Kind}' has no exact materialization.", "constant")
            };
            var assignment = $"{typedDestination ?? $"*({TypeName(type)}*)({destination})"} = {expression};";
            if (deferAddress && value is AddressValue) addressFixups.Add((threadInitializationOwner is not null, assignment));
            else writer.Line(assignment);
        }

        private string AddressExpression(AddressValue address, CppType type)
        {
            // ByteOffset is the complete recorded displacement from BaseEntity.
            // Stock form_lvalue_for_addressed_entity starts with address.offset and
            // subtracts any path displacement consumed for presentation; a path is
            // not an additional displacement. Emit the direct byte address instead.
            if (address.BaseEntity is null)
            {
                if (address.ByteOffset.IsZero) return $"({TypeName(type)})0";
                throw Limit("An absolute nonzero address has no runtime binding.", "constant-address");
            }
            if (TryNativeDataAddress(address, type, out var nativeDataAddress)) return nativeDataAddress;
            var entity = address.BaseEntity;
            if (entity is FunctionSymbol function)
            {
                if (!address.ByteOffset.IsZero) throw Limit("A function address has a nonzero retained byte displacement.", "function-pointer");
                var canonicalAddress = FunctionAddress(function);
                var destinationType = TypeName(type);
                return destinationType == "nint" ? canonicalAddress : $"unchecked(({destinationType})({canonicalAddress}))";
            }
            if (entity is Constant constant)
            {
                if (constant.AssociatedStorageVariable is { } variable) entity = variable;
                else
                {
                    AddStorage(constant, constant.Type ?? throw Limit("Literal storage has no captured type.", "constant-address"), constant, $"literal_{storageOrder.Count + 1}");
                    entity = constant;
                }
            }
            if (entity is VariableSymbol companion) _ = TryAdmitSelectedConstantStorageCompanion(companion);
            entity = CanonicalEntity(entity);
            if (!storage.TryGetValue(entity, out var item)) throw Limit($"Address base '{entity.Name}' has no selected storage.", "constant-address");
            if (item.ThreadLocal && nativeDataInitializerOwner is not null && threadInitializationOwner is null)
                throw Limit("A process-storage constant cannot capture a per-thread object address.", "thread-initialization");
            return $"({TypeName(type)})((byte*)({StorageInitializationAddress(item)}) + ({address.ByteOffset.ToString(CultureInfo.InvariantCulture)}))";
        }

        private string IntegerLiteral(BigInteger value, CppType type)
        {
            if (TryWideIntegerLiteral(value, type, out var wide)) return wide;
            var name = TypeName(type);
            var text = value.ToString(CultureInfo.InvariantCulture);
            var suffix = name switch { "uint" => "U", "long" => "L", "ulong" => "UL", _ => "" };
            return name is "int" or "uint" or "long" or "ulong" ? text + suffix : $"unchecked(({name})({text}))";
        }

        private static int StorageSize(CppType type) => type.Size is { } size && size > 0 && size <= int.MaxValue
            ? (int)size : throw Limit("Storage needs a complete positive captured size within the current allocation bound.", "storage");

        private void EmitArray(ArrayType array, StringBuilder output)
        {
            ValidateValueAddress(array.ElementType);
            if (array.Bound is not { } bound || bound <= 0 || array.VlaDimension is not null)
                throw Limit("Incomplete, zero-sized and variable-length arrays require a separate storage representation.", "array");
            if (array.Alignment is not { } alignment || alignment < 1 || alignment > 8)
                throw Limit("Array CLR alignment is unsupported.", "array");
            output.Append("[StructLayout(LayoutKind.Explicit, Size = ").Append(StorageSize(array)).Append(", Pack = ").Append(alignment).AppendLine(")]");
            output.Append("public unsafe struct ").Append(names[array]).AppendLine("\n{");
            output.Append("    [FieldOffset(0)] public ").Append(TypeName(array.ElementType)).AppendLine(" __firstElement;");
            output.AppendLine("}").AppendLine();
        }

        private static string AlignmentCarrier(BigInteger alignment) => (int)alignment switch
        { 1 => "byte", 2 => "ushort", 4 => "uint", 8 => "ulong", _ => throw Limit("CLR alignment carrier is unsupported.", "record") };

        private static (int Offset, int Bit, int Width) BitCoordinates(FieldLayout field)
        {
            if (field.Offset is not { } offset || offset < 0 || offset > int.MaxValue || field.BitOffset is not { } bit || bit < 0 || bit > 7 ||
                field.BitWidth is not { } width || width < 1 || width > 64)
                throw Limit("Bitfield has unavailable or unsupported exact byte/bit coordinates.", "bitfield");
            return ((int)offset, (int)bit, (int)width);
        }

        private void EmitBitField(RecordType record, FieldLayout field, StringBuilder output)
        {
            if (field.Field.Name is null || field.BitWidth == 0) return;
            var (offset, bit, width) = BitCoordinates(field);
            if (field.Field.Details.BitFieldIsSigned is not { } signed)
                throw Limit("Bitfield signedness was not retained.", "bitfield");
            var type = TypeName(field.Field.Type);
            output.Append("    public ").Append(type).Append(' ').Append(FieldName(field.Field)).AppendLine("\n    {");
            output.Append("        get { fixed (").Append(names[record]).Append("* self = &this) return unchecked((").Append(type)
                .Append(")TargetBitField.").Append(signed ? "ReadSigned" : "ReadUnsigned").Append("((byte*)self, ")
                .Append(offset).Append(", ").Append(bit).Append(", ").Append(width).AppendLine(")); }");
            output.Append("        set { fixed (").Append(names[record]).Append("* self = &this) TargetBitField.Write((byte*)self, ")
                .Append(offset).Append(", ").Append(bit).Append(", ").Append(width).AppendLine(", unchecked((ulong)value)); }");
            output.AppendLine("    }");
        }
    }
}
