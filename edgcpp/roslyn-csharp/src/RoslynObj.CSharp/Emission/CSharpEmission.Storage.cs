using System.Globalization;
using System.Numerics;
using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record StorageItem(SemanticEntity Entity, CppType Type, Constant? Initializer, string OwnerField);
        private readonly Dictionary<SemanticEntity, StorageItem> storage = [];
        private readonly List<StorageItem> storageOrder = [];
        private readonly List<ArrayType> arrays = [];
        private readonly List<string> addressFixups = [];

        private void AdmitGlobal(VariableSymbol variable)
        {
            if (variable.Details.IsNonreal || variable.Details.IsPrototypeInstantiation || variable.Details.IsTemplateVariable) return;
            if (variable.SymbolDetails.Storage?.Name == "sc_extern") return;
            if (variable.IsThreadLocal || variable.LocalStaticInitializations.Count != 0)
                throw Limit($"Storage '{variable.Name}' requires thread/local-static initialization support.", "storage");
            if (variable.Details.IsWeak || variable.Details.IsCommon || variable.Details.IsGnuAlias || variable.Details.IsWeakref || variable.Details.AliasedVariable is not null)
                throw Limit($"Storage '{variable.Name}' requires an explicit weak/common/alias selection policy.", "link-policy");
            if (variable.Initializer is { } initializer && initializer.Kind.Name != "dik_constant")
                throw Limit($"Storage '{variable.Name}' has a dynamic initializer '{initializer.Kind}'.", "initialization");
            AddStorage(variable, variable.Type, variable.ConstantInitializer ?? variable.Initializer?.Constant, variable.Name ?? "storage");
        }

        private void AddStorage(SemanticEntity entity, CppType type, Constant? initializer, string candidate)
        {
            if (storage.ContainsKey(entity)) return;
            _ = TypeName(type);
            _ = StorageSize(type);
            Name(entity, candidate, globalNames);
            var item = new StorageItem(entity, type, initializer, InternalName("__storage_" + names[entity].TrimStart('@')));
            storage.Add(entity, item);
            storageOrder.Add(item);
        }

        private void EmitGlobalStorage(StringBuilder fields, CodeWriter initialization)
        {
            for (var index = 0; index < storageOrder.Count; ++index)
            {
                var item = storageOrder[index];
                if (item.Initializer is { } constant)
                    WriteConstant(item.Type, $"(byte*){names[item.Entity]}", constant.Value, initialization, true);
            }
            // Constant processing can discover a literal storage occurrence. All allocations
            // are field initializers, so every address exists before these fixups execute.
            foreach (var item in storageOrder)
            {
                var name = names[item.Entity];
                var alignment = item.Type.Alignment ?? throw Limit("Global storage alignment is absent.", "storage");
                if (item.Entity is VariableSymbol variable && variable.Details.ExplicitAlignment is { } explicitAlignment)
                    alignment = BigInteger.Max(alignment, explicitAlignment);
                if (alignment <= 0 || alignment > int.MaxValue) throw Limit("Global alignment is unsupported.", "storage");
                fields.Append("    private static readonly PinnedStorage ").Append(item.OwnerField)
                    .Append(" = new(").Append(StorageSize(item.Type)).Append(", ").Append(alignment).AppendLine(");");
                fields.Append("    public static ").Append(TypeName(item.Type)).Append("* ").Append(name)
                    .Append(" => (").Append(TypeName(item.Type)).Append("*)").Append(item.OwnerField).AppendLine(".Pointer;");
            }
            if (storageOrder.Count > 0) fields.AppendLine();
            foreach (var fixup in addressFixups) initialization.Line(fixup);
        }

        private void WriteConstant(CppType type, string destination, ConstantValue value, CodeWriter writer, bool deferAddress)
        {
            type = Unwrap(type);
            if (value is StringValue text)
            {
                if (text.UnitKind.Name != "target_bytes" || text.StorageUnitBits != 8)
                    throw Limit("String materialization requires exact target-byte units.", "constant");
                if (text.Length > StorageSize(type)) throw Limit("String target bytes exceed selected storage.", "constant");
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
            var assignment = $"*({TypeName(type)}*)({destination}) = {expression};";
            if (deferAddress && value is AddressValue) addressFixups.Add(assignment);
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
            var entity = address.BaseEntity;
            if (entity is FunctionSymbol function)
            {
                if (!address.ByteOffset.IsZero) throw Limit("A function address has a nonzero retained byte displacement.", "function-pointer");
                return FunctionAddress(function);
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
            if (!storage.ContainsKey(entity)) throw Limit($"Address base '{entity.Name}' has no selected storage.", "constant-address");
            return $"({TypeName(type)})((byte*){OwnedName(entity)} + ({address.ByteOffset.ToString(CultureInfo.InvariantCulture)}))";
        }

        private string IntegerLiteral(BigInteger value, CppType type)
        {
            var name = TypeName(type);
            var text = value.ToString(CultureInfo.InvariantCulture);
            var suffix = name switch { "uint" => "U", "long" => "L", "ulong" => "UL", _ => "" };
            return name is "int" or "uint" or "long" or "ulong" ? text + suffix : $"unchecked(({name})({text}))";
        }

        private static int StorageSize(CppType type) => type.Size is { } size && size > 0 && size <= int.MaxValue
            ? (int)size : throw Limit("Storage needs a complete positive captured size within the current allocation bound.", "storage");

        private void EmitArray(ArrayType array, StringBuilder output)
        {
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
