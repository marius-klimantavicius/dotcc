using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private readonly List<SemanticNativeDataImplementation> nativeDataImplementations = [];
        private readonly Dictionary<VariableSymbol, string> nativeDataFields = [];
        private readonly HashSet<AddressValue> nativeDataAddresses = [];
        private readonly Dictionary<AddressValue, HashSet<VariableSymbol>> nativeDataAddressOwners = [];
        private SemanticEntity? nativeDataInitializerOwner;
        private readonly HashSet<VariableSymbol> nativeTypeinfoStorage = [];
        private readonly HashSet<FieldSymbol> nativeTypeinfoFields = [];

        private void InitializeNativeDataBindings()
        {
            if (options.ExternalData.Count == 0) return;
            if (inputs.Count != 1 || selectedProgram is not null || options.ExternalData.Count is not (2 or 3) ||
                options.ExternalData.Select(binding => binding.LinkageName).Distinct(StringComparer.Ordinal).Count() != options.ExternalData.Count ||
                options.ExternalData.Any(binding => !SemanticNativeDataImplementation.IsSupportedSymbol(binding.LinkageName)) ||
                !options.ExternalData.Any(binding => binding.LinkageName == "_ZTVN10__cxxabiv117__class_type_infoE"))
                throw Limit("The address-only RTTI provider requires its exact two- or three-export single-unit profile.", "native-data");
            foreach (var binding in options.ExternalData)
            {
                if (!SemanticNativeDataImplementation.IsSupportedSymbol(binding.LinkageName) || binding.EntryPoint != binding.LinkageName ||
                    binding.AddressContract is not { Version: SemanticNativeDataImplementation.Contract, SymbolVersion: "CXXABI_1.3",
                        SymbolSizeBytes: 88, ExportRelativeVirtualAddress: > 0 } ||
                    !Path.IsPathFullyQualified(binding.LibraryPath) ||
                    !binding.LibrarySha256.Equals(SemanticNativeDataImplementation.ProviderSha256, StringComparison.OrdinalIgnoreCase) ||
                    binding.AddressContract.ExportRelativeVirtualAddress != SemanticNativeDataImplementation.ExportRva(binding.LinkageName))
                    throw Limit("Native RTTI data needs its exact symbol, pinned provider and explicit address-only ELF contract.", "native-data");
                var declarations = inputs[0].TranslationUnit.Variables.Where(variable =>
                    (variable.AssemblyName ?? variable.CurrentName) == binding.LinkageName).ToArray();
                if (declarations.Length != 1) throw Limit("Native data does not select one exact public declaration.", "native-data");
                var declaration = declarations[0];
                if (declaration.SymbolDetails.Storage?.Name != "sc_extern" || declaration.IsThreadLocal ||
                    declaration.Initializer is not null || declaration.ConstantInitializer is not null || declaration.LocalStaticInitializations.Count != 0 ||
                    declaration.Details.IsWeak || declaration.Details.IsWeakref || declaration.Details.IsCommon || declaration.Details.IsGnuAlias || declaration.Details.AliasedVariable is not null ||
                    declaration.Type is not ArrayType { IsIncomplete: true, Size: null, Alignment: null, Bound: null } array ||
                    array.Array.BoundKind.Name != "incomplete" || !IsNativeDataElement(array.ElementType))
                    throw Limit("The borrowed RTTI declaration must remain its incomplete const-signed64 array; no ELF extent becomes captured storage.", "native-data");
                nativeDataImplementations.Add(new(declaration, binding));
                nativeDataFields.Add(declaration, InternalName("__native_rtti_data_" + nativeDataImplementations.Count));
            }
            foreach (var item in storageOrder)
            {
                if (item.Entity is VariableSymbol variable && item.Initializer is { } initializer)
                    ValidateNativeDataInitializer(variable, item.Type, initializer.Value, RawRecord(item.Type), BigInteger.Zero, 0, []);
            }
            foreach (var implementation in nativeDataImplementations)
                if (nativeDataAddresses.Count(address => address.BaseEntity?.Equals(implementation.Variable) == true) != 1)
                    throw Limit("Each native RTTI export must have exactly its validated typeinfo-vptr address value; allowed initializer uses are tracked separately.", "native-data");
            foreach (var function in functions)
            foreach (var node in ExceptionExecutionNodes(function.Body!.Root))
            {
                if (node is ReferenceOperation reference && reference.Target is { } target &&
                    (target is VariableSymbol variable && (nativeDataFields.ContainsKey(variable) || nativeTypeinfoStorage.Contains(variable)) ||
                     target is FieldSymbol field && nativeTypeinfoFields.Contains(field)))
                    throw Limit("The borrowed RTTI provider admits constant typeinfo addresses, not executable data/typeinfo access.", "native-data");
                if (node is ConstantOperation constant && ContainsNativeDataAddress(constant.Constant.Value, []))
                    throw Limit("A native RTTI address cannot be projected as an executable value in this slice.", "native-data");
            }
        }

        private static bool IsNativeDataElement(CppType type)
        {
            var isConst = false;
            while (type is TypeReference wrapper)
            {
                if (type.Details.IsAtomic) return false;
                if (wrapper is QualifiedType qualified)
                {
                    var qualifier = qualified.Qualification;
                    if (qualifier.IsVolatile || qualifier.IsAtomic || qualifier.AddressSpace is not (null or 0) || qualifier.IsShared ||
                        qualifier.IsUpcStrict || qualifier.IsUpcRelaxed || qualifier.IsNear || qualifier.IsFar || qualifier.IsUnaligned) return false;
                    isConst |= qualifier.IsConst;
                }
                type = wrapper.UnderlyingType;
            }
            return isConst && !type.Details.IsAtomic && type is ArithmeticType arithmetic && arithmetic.Arithmetic.Bits == 64 &&
                arithmetic.Arithmetic.Signed == true && type.Size == 8 && type.Alignment == 8;
        }

        private static RecordType? RawRecord(CppType type)
        {
            while (type is TypeReference reference) type = reference.UnderlyingType;
            return type as RecordType;
        }

        private void ValidateNativeDataInitializer(VariableSymbol root, CppType type, ConstantValue value, RecordType? rootRecord,
            BigInteger offset, int fieldDepth, HashSet<ConstantValue> active)
        {
            if (!active.Add(value)) throw Limit("An initializer value cycle has no bounded RTTI address projection.", "native-data");
            try
            {
                if (value is AddressValue address && address.BaseEntity is VariableSymbol external && nativeDataFields.ContainsKey(external))
                {
                    var raw = MemoryRepresentationType(type);
                    if (rootRecord is null || !IsNativeDataTypeinfoRoot(root, rootRecord, external) ||
                        offset != 0 || fieldDepth < 1 || raw is not PointerType { IsReference: false } pointer || pointer.Size != 8 || pointer.Alignment != 8 ||
                        !IsNativeDataElement(pointer.PointeeType) || address.BaseKind.Name != "abk_variable" || address.ByteOffset != 16 ||
                        address.OnePastEnd || address.SubobjectPath is not null || address.Details.IsObjectReflection || address.Details.ParameterNumber is not null)
                        throw Limit("The native RTTI export is only its captured +16 typeinfo-vptr constant, without path arithmetic or executable access.", "native-data");
                    if (!nativeDataAddressOwners.TryGetValue(address, out var owners))
                        nativeDataAddressOwners.Add(address, owners = []);
                    // A value identity can be shared by distinct captured initializers.
                    // Authorization is the actual (root, value) context, not value identity alone.
                    owners.Add(root);
                    nativeDataAddresses.Add(address);
                    nativeTypeinfoStorage.Add(root);
                    AddNativeTypeinfoFields(rootRecord, []);
                    return;
                }
                if (value is not AggregateValue aggregate) return;
                var rawType = MemoryRepresentationType(type);
                var fields = rawType is RecordType record ? record.Layout?.Fields.ToArray() ?? [] : [];
                var children = aggregate.Elements.ToArray();
                for (var index = 0; index < children.Length; ++index)
                {
                    var child = children[index].Value;
                    if (rawType is RecordType)
                    {
                        if (index >= fields.Length) throw Limit("RTTI aggregate placement exceeds its retained fields.", "native-data");
                        var field = fields[index];
                        var fieldOffset = field.Offset ?? throw Limit("A native data initializer has no captured field displacement.", "native-data");
                        ValidateNativeDataInitializer(root, field.Field.Type, child, rootRecord, offset + fieldOffset, fieldDepth + 1, active);
                    }
                    else if (rawType is ArrayType array && array.Bound is { } bound && index < bound && array.ElementType.Size is { } stride)
                        ValidateNativeDataInitializer(root, array.ElementType, child, rootRecord, offset + index * stride, fieldDepth, active);
                    else if (ContainsNativeDataAddress(child, []))
                        throw Limit("A native RTTI address has no exact retained aggregate placement.", "native-data");
                }
            }
            finally { active.Remove(value); }
        }

        private bool IsNativeDataTypeinfoRoot(VariableSymbol root, RecordType record, VariableSymbol external)
        {
            if (record.Alignment != 8) return false;
            var symbol = external.AssemblyName ?? external.CurrentName;
            if (record.VirtualFunctionTableVariable?.Equals(external) == true)
            {
                if (symbol == "_ZTVN10__cxxabiv117__class_type_infoE") return record.Size == 16;
                if (symbol == "_ZTVN10__cxxabiv120__si_class_type_infoE") return record.Size == 24;
                // The one-base VMI record retains its real vtable association.
                // Its application owner supplies the selected virtual-base context.
                return symbol == "_ZTVN10__cxxabiv121__vmi_class_type_infoE" && IsNativeVmiLayout(record, 1) &&
                    NativeTypeinfoOwners(root) is [var owner] && owner.HasVirtualBases && owner.Bases.Count == 1 && owner.Bases[0].IsVirtual;
            }
            if (symbol != "_ZTVN10__cxxabiv121__vmi_class_type_infoE" || !IsNativeVmiLayout(record, 2)) return false;
            // Lowered two-base VMI storage is an anonymous expanded record. It
            // has no VirtualFunctionTableVariable link; do not invent one.
            return NativeTypeinfoOwners(root) is [var derived] && !derived.HasVirtualBases && derived.Bases.Count == 2 &&
                derived.Size == 32 && derived.Alignment == 8 && derived.Bases.All(@base => !@base.IsVirtual);
        }

        private RecordType[] NativeTypeinfoOwners(VariableSymbol root) => functions.Where(function => function.IsVirtual)
            .Select(function => function.ContainingType).OfType<RecordType>().Distinct()
            .Where(type => type.Details.TypeinfoVariable?.Equals(root) == true).ToArray();

        private static bool IsNativeVmiLayout(RecordType record, int count)
        {
            if (record.Size != 24 + count * 16 || record.Alignment != 8 ||
                record.Layout is not { IsComplete: true } layout || layout.Fields.Count != 4) return false;
            var fields = layout.Fields;
            return fields[0].Offset == 0 && fields[1].Offset == 16 && fields[2].Offset == 20 && fields[3].Offset == 24 &&
                fields.All(field => field.BitWidth is null) && RawRecord(fields[0].Field.Type) is { Size: { } baseSize, Alignment: { } baseAlignment } &&
                baseSize == 16 && baseAlignment == 8 && fields[1].Field.Type.Size == 4 && fields[2].Field.Type.Size == 4 &&
                MemoryRepresentationType(fields[3].Field.Type) is ArrayType { Bound: { } bound, Size: { } arraySize, Alignment: { } arrayAlignment } array &&
                bound == count && arraySize == count * 16 && arrayAlignment == 8 &&
                RawRecord(array.ElementType) is { Size: { } elementSize, Alignment: { } elementAlignment } && elementSize == 16 && elementAlignment == 8;
        }

        private void AddNativeTypeinfoFields(RecordType record, HashSet<RecordType> seen)
        {
            if (!seen.Add(record)) return;
            foreach (var field in record.Fields)
            {
                nativeTypeinfoFields.Add(field);
                if (RawRecord(field.Type) is { } nested) AddNativeTypeinfoFields(nested, seen);
            }
        }

        private bool ContainsNativeDataAddress(ConstantValue value, HashSet<ConstantValue> seen)
        {
            if (!seen.Add(value)) return false;
            if (value is AddressValue { BaseEntity: VariableSymbol variable })
                return nativeDataFields.ContainsKey(variable) || nativeTypeinfoStorage.Contains(variable);
            if (value is ConstantValue<RepeatValueDetails> repeat) return ContainsNativeDataAddress(repeat.Payload.Value, seen);
            return value is AggregateValue aggregate && aggregate.Elements.Any(element => ContainsNativeDataAddress(element.Value, seen));
        }

        private bool TryNativeDataAddress(AddressValue address, CppType destination, out string expression)
        {
            expression = "";
            if (address.BaseEntity is not VariableSymbol variable || !nativeDataFields.TryGetValue(variable, out var field)) return false;
            if (!nativeDataAddresses.Contains(address) || nativeDataInitializerOwner is not VariableSymbol owner ||
                !nativeDataAddressOwners[address].Contains(owner)) throw Limit("Native data address escaped its validated constant-initializer role.", "native-data");
            expression = $"({TypeName(destination)})((byte*){Owner}.{field} + ({address.ByteOffset.ToString(CultureInfo.InvariantCulture)}))";
            return true;
        }

        private void EmitNativeDataFields(StringBuilder fields)
        {
            foreach (var implementation in nativeDataImplementations)
            {
                var binding = implementation.Binding;
                var contract = binding.AddressContract;
                fields.Append("    private static readonly nint ").Append(nativeDataFields[implementation.Variable])
                    .Append(" = global::RoslynObj.CSharp.Runtime.BorrowedNativeRttiData.ResolveAddressOnlyVtable(")
                    .Append(JsonSerializer.Serialize(binding.LibraryPath)).Append(", ").Append(JsonSerializer.Serialize(binding.LibrarySha256)).Append(", ")
                    .Append(JsonSerializer.Serialize(binding.EntryPoint)).Append(", ").Append(contract.ExportRelativeVirtualAddress.ToString(CultureInfo.InvariantCulture)).Append("UL, ")
                    .Append(contract.SymbolSizeBytes).Append(", ").Append(JsonSerializer.Serialize(contract.SymbolVersion)).AppendLine(");");
            }
        }

        private string NativeDataImportReport() => JsonSerializer.Serialize(new
        {
            ReportVersion = 1,
            Contract = SemanticNativeDataImplementation.Contract,
            AddressOnly = true,
            IsCapturedSize = false,
            SymbolVersionMeaning = "Audited metadata of the hash-pinned provider's default export; no dlvsym lookup is performed.",
            ExecutionScope = "No native RTTI query, destructor slot, application method or unwinding helper is invoked by this data projection.",
            Imports = nativeDataImplementations.Select(implementation => new
            {
                implementation.Binding.LinkageName, implementation.Binding.LibraryPath, implementation.Binding.LibrarySha256,
                implementation.Binding.EntryPoint, implementation.Binding.AddressContract,
                CapturedDeclarationSize = implementation.Variable.Type.Size?.ToString(CultureInfo.InvariantCulture),
                CapturedAddressByteOffset = "16",
                RuntimeChecks = "Provider hash, resolved export BASE owner/RVA, full ELF extent read-only mapping; retained ByteOffset applied once by generated initializer."
            }).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
