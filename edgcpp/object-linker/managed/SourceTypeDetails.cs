using System.Globalization;
using System.Numerics;

namespace DotObj;

internal sealed record SourceEnumType(long TypeId, string IdentityKey, long UnderlyingTypeId,
    bool IsScoped, bool IsComplete, bool EnumeratorListComplete, bool HasExplicitBase);
internal sealed record SourceEnumerator(long TypeId, long Ordinal, string SourceName, string ValueText,
    long? SourceFileId, long Line, long Column);
internal sealed record SourceMemberPointerType(long TypeId, long OwnerTypeId, long MemberTypeId, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceEnumType> SourceEnums { get; } = [];
    public Dictionary<long, List<SourceEnumerator>> SourceEnumerators { get; } = [];
    public Dictionary<long, SourceMemberPointerType> SourceMemberPointers { get; } = [];
    public Dictionary<long, SortedSet<string>> SourceTypeQualifiers { get; } = [];

    private void ReadAndVerifySourceTypes()
    {
        using (var query = database.Prepare("SELECT id,kind,source_name,underlying_type_id,execution_type_id,is_const,is_volatile,source_file_id,line,column,element_count,is_restrict,is_atomic,array_bound_kind,source_form FROM source_types ORDER BY id"))
            while (query.Read())
            {
                var type = new SourceType(query.Integer(0), query.Text(1), query.Text(2), query.OptionalInteger(3), query.OptionalInteger(4),
                    query.Integer(5) == 1, query.Integer(6) == 1, query.OptionalInteger(7), query.Integer(8), query.Integer(9), query.OptionalInteger(10),
                    query.Integer(11) == 1, query.Integer(12) == 1, query.OptionalText(13),query.Text(14));
                if (type.Id <= 0 || type.Line < 0 || type.Column < 0 || query.Integer(5) is not (0 or 1) || query.Integer(6) is not (0 or 1) ||
                    query.Integer(11) is not (0 or 1) || query.Integer(12) is not (0 or 1))
                    throw new InvalidDataException("Invalid source type identity, qualifiers or location");
                SourceTypes.Add(type.Id, type);
            }
        ReadSourceTypeDetails();
        ReadDependentSourceTypes();
        ReadSourceTemplateMetadata();
        foreach (SourceType type in SourceTypes.Values)
        {
            string? execution = type.ExecutionRepresentationId is long id ? Type(id) : null;
            if (type.IsAtomic && execution is not null)
                throw new InvalidDataException("Atomic source types require an explicit atomic execution contract");
            bool qualified = type.IsConst || type.IsVolatile || type.IsRestrict || type.IsAtomic || SourceTypeQualifiers.ContainsKey(type.Id);
            if (type.Kind == "array" ? type.ArrayBoundKind is not ("constant" or "unspecified" or "dependent") ||
                (type.ArrayBoundKind == "constant") != (type.ElementCount is not null) : type.ArrayBoundKind is not null)
                throw new InvalidDataException("Source array bound kind differs from its original extent");
            if (type.Kind == "array" ? type.ElementCount is < 0 : type.ElementCount is not null)
                throw new InvalidDataException("Source array requires a nonnegative element count");
            if (type.Kind is "typedef" or "qualified")
            {
                if (type.UnderlyingTypeId is null || (type.Kind == "typedef" ? type.SourceName.Length == 0 : type.SourceName.Length != 0 || !qualified && type.SourceForm=="plain"))
                    throw new InvalidDataException("Invalid source typedef or qualified type");
            }
            else
            {
                if (qualified) throw new InvalidDataException("Source qualifiers require an owned qualification or typedef node");
                switch (type.Kind)
                {
                    case "dependent":
                        if (!SourceDependentTypes.ContainsKey(type.Id) || execution is not null || type.UnderlyingTypeId is not null)
                            throw new InvalidDataException("Dependent source type cannot fabricate an execution representation");
                        break;
                    case "array":
                        if (type.UnderlyingTypeId is null || type.SourceName.Length != 0 || execution is not null &&
                            (!ScalarTypes.IsArray(execution) || ScalarTypes.ArrayBound(execution) != type.ElementCount))
                            throw new InvalidDataException("Invalid source array type or execution extent");
                        break;
                    case "pointer": case "lvalue-reference": case "rvalue-reference":
                        if (type.UnderlyingTypeId is null || type.SourceName.Length != 0 || execution is not null && !ScalarTypes.IsPointer(execution))
                            throw new InvalidDataException("Invalid source pointer or reference type");
                        break;
                    case "routine":
                        if (!SourceRoutines.ContainsKey(type.Id) || type.SourceName.Length != 0 || type.UnderlyingTypeId is not null || execution is not null && !RoutineTypes.IsRoutine(execution))
                            throw new InvalidDataException("Invalid source routine type");
                        break;
                    case "enum":
                        if (!SourceEnums.ContainsKey(type.Id) || type.UnderlyingTypeId is not null || execution is not null && !ScalarTypes.IsInteger(execution))
                            throw new InvalidDataException("Invalid source enum representation");
                        break;
                    case "record":
                        if (!SourceRecordsByType.TryGetValue(type.Id, out SourceRecord? record) || type.SourceName != record.SourceName || type.UnderlyingTypeId is not null)
                            throw new InvalidDataException("Source record lacks its original nominal description");
                        if (execution is not null && (!Records.TryGetValue(type.ExecutionTypeId, out RecordType? layout) || layout.SourceRecordId != record.Id))
                            throw new InvalidDataException("Source record disagrees with its execution representation");
                        break;
                    case "member-pointer":
                        if (!SourceMemberPointers.TryGetValue(type.Id, out SourceMemberPointerType? member) || type.SourceName.Length != 0 || type.UnderlyingTypeId != member.MemberTypeId)
                            throw new InvalidDataException("Source member pointer lacks its owner/member description");
                        break;
                    default:
                        string expected = type.Kind switch
                        {
                            "long-double" or "float80" or "float64x" => "f80", "va-list" => "va-list", "void" => "void", "bool" => "u8", "float" => "f32", "double" => "f64",
                            "signed-char" => "i8", "unsigned-char" or "char8-t" => "u8", "short" => "i16", "unsigned-short" or "char16-t" => "u16",
                            "int" => "i32", "unsigned-int" or "char32-t" => "u32", "int128" => "i128", "unsigned-int128" => "u128",
                            "long" or "long-long" => "i64", "unsigned-long" or "unsigned-long-long" => "u64", "nullptr" => "ptr(void)",
                            "char" when execution is null or "i8" or "u8" => execution ?? "i8",
                            "wchar-t" when execution is null or "i16" or "u16" or "i32" or "u32" => execution ?? "i32",
                            _ => throw new InvalidDataException("Unsupported original source scalar type")
                        };
                        if (execution is not null && execution != expected || type.UnderlyingTypeId is not null || type.SourceName.Length != 0)
                            throw new InvalidDataException("Source builtin type disagrees with its execution representation");
                        break;
                }
            }
        }
        // Resolve original type relationships independently of execution support.
        var visited = new HashSet<long>();
        foreach (SourceType start in SourceTypes.Values)
        {
            var chain = new HashSet<long>();
            SourceType current = start;
            while (!visited.Contains(current.Id))
            {
                if (!chain.Add(current.Id)) throw new InvalidDataException("Cycle in source type references");
                if (current.UnderlyingTypeId is not long underlying) break;
                SourceType target = SourceTypeById(underlying);
                if (current.Kind is "pointer" or "lvalue-reference" or "rvalue-reference")
                {
                    if (Original(target).Kind is "lvalue-reference" or "rvalue-reference" || current.Kind != "pointer" && Original(target).Kind == "void" ||
                        current.ExecutionRepresentationId is long physical && ScalarTypes.Pointee(Type(physical)) != Type(target.ExecutionTypeId))
                        throw new InvalidDataException("Source pointer pointee disagrees with execution representation");
                }
                else if (current.Kind == "array")
                {
                    if (current.ExecutionRepresentationId is long physical && ScalarTypes.Element(Type(physical)) != Type(target.ExecutionTypeId))
                        throw new InvalidDataException("Source array element disagrees with execution representation");
                    if (Original(target).Kind == "array" && Original(target).ElementCount is null && !SourceArrayBounds.ContainsKey(Original(target).Id))
                        throw new InvalidDataException("Array element cannot be incomplete");
                }
                else if (current.Kind != "member-pointer" && current.ExecutionRepresentationId is long physical && physical != target.ExecutionRepresentationId)
                    throw new InvalidDataException("Source typedef/qualification changes the execution representation");
                current = target;
            }
            visited.UnionWith(chain);
        }
        VerifySourceTypeDetails();
    }

    private void ReadSourceTypeDetails()
    {
        using (var query = database.Prepare("SELECT type_id,name FROM source_type_qualifiers ORDER BY type_id,name"))
            while (query.Read())
            {
                long id = query.Integer(0); string name = query.Text(1);
                SourceTypeById(id);
                if (name is not ("nullable" or "non-null" or "null-unspecified" or "unaligned" or "near" or "far" or "upc-shared" or "upc-strict" or "upc-relaxed"))
                    throw new InvalidDataException("Unknown original source qualifier");
                if (name is "upc-shared" or "upc-strict" or "upc-relaxed")
                    throw new InvalidDataException("UPC source qualifiers require an owned block-size and shared-memory type contract");
                if (name is "unaligned" or "near" or "far" && SourceTypeById(id).ExecutionRepresentationId is not null)
                    throw new InvalidDataException("Extended source qualifier requires an owned access or address-space execution contract");
                if (!SourceTypeQualifiers.TryGetValue(id, out var qualifiers)) SourceTypeQualifiers.Add(id, qualifiers = new(StringComparer.Ordinal));
                if (!qualifiers.Add(name)) throw new InvalidDataException("Duplicate original source qualifier");
            }
        using (var query = database.Prepare("SELECT type_id,identity_key,underlying_type_id,is_scoped,is_complete,enumerator_list_complete,has_explicit_base FROM source_enum_types ORDER BY type_id"))
            while (query.Read())
            {
                var type = new SourceEnumType(query.Integer(0), query.Text(1), query.Integer(2), query.Integer(3) == 1, query.Integer(4) == 1, query.Integer(5) == 1, query.Integer(6) == 1);
                if (SourceTypeById(type.TypeId).Kind != "enum" || type.IdentityKey.Length == 0 ||
                    query.Integer(3) is not (0 or 1) || query.Integer(4) is not (0 or 1) || query.Integer(5) is not (0 or 1) || query.Integer(6) is not (0 or 1) ||
                    type.EnumeratorListComplete && !type.IsComplete)
                    throw new InvalidDataException("Invalid original enum identity or completeness");
                SourceEnums.Add(type.TypeId, type);
            }
        using (var query = database.Prepare("SELECT type_id,ordinal,source_name,value_text,source_file_id,line,column FROM source_enumerators ORDER BY type_id,ordinal"))
            while (query.Read())
            {
                var item = new SourceEnumerator(query.Integer(0), query.Integer(1), query.Text(2), query.Text(3), query.OptionalInteger(4), query.Integer(5), query.Integer(6));
                if (!SourceEnums.TryGetValue(item.TypeId, out SourceEnumType? type) || !type.EnumeratorListComplete || item.SourceName.Length == 0 || item.Line < 0 || item.Column < 0)
                    throw new InvalidDataException("Invalid original enumerator identity or location");
                if (!SourceEnumerators.TryGetValue(item.TypeId, out var entries)) SourceEnumerators.Add(item.TypeId, entries = []);
                if (item.Ordinal != entries.Count) throw new InvalidDataException("Invalid original enumerator declaration order");
                entries.Add(item);
            }
        using (var query = database.Prepare("SELECT type_id,owner_type_id,member_type_id,capture_proof FROM source_member_pointer_types ORDER BY type_id"))
            while (query.Read())
            {
                var item = new SourceMemberPointerType(query.Integer(0), query.Integer(1), query.Integer(2), query.Text(3));
                SourceType source = SourceTypeById(item.TypeId);
                if (source.Kind != "member-pointer" || item.CaptureProof != (source.ExecutionRepresentationId is null ? "observed-source-type" : "observed-member-pointer-lowering"))
                    throw new InvalidDataException("Invalid original member pointer lowering provenance");
                SourceMemberPointers.Add(item.TypeId, item);
            }
    }

    private void VerifySourceTypeDetails()
    {
        foreach (var qualifiers in SourceTypeQualifiers.Values)
            if (qualifiers.Count(name => name is "nullable" or "non-null" or "null-unspecified") > 1 ||
                qualifiers.Contains("near") && qualifiers.Contains("far") || qualifiers.Contains("upc-strict") && qualifiers.Contains("upc-relaxed"))
                throw new InvalidDataException("Conflicting original source qualifiers");
        foreach (SourceEnumType item in SourceEnums.Values)
        {
            SourceType source = SourceTypeById(item.TypeId), underlying = Original(SourceTypeById(item.UnderlyingTypeId));
            (int bits, bool signed) = SourceIntegerShape(underlying);
            if (source.ExecutionRepresentationId is long physical && Type(physical) != Type(underlying.ExecutionTypeId))
                throw new InvalidDataException("Enum execution representation differs from its original underlying type");
            BigInteger minimum = signed ? -(BigInteger.One << (bits - 1)) : BigInteger.Zero;
            BigInteger maximum = (BigInteger.One << (signed ? bits - 1 : bits)) - 1;
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (SourceEnumerator value in SourceEnumerators.GetValueOrDefault(item.TypeId, []))
                if (!names.Add(value.SourceName) || !BigInteger.TryParse(value.ValueText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger number) ||
                    number.ToString(CultureInfo.InvariantCulture) != value.ValueText || number < minimum || number > maximum)
                    throw new InvalidDataException("Invalid original enumerator name, exact integer value or underlying range");
        }
        foreach (SourceMemberPointerType item in SourceMemberPointers.Values)
        {
            SourceType member = Original(SourceTypeById(item.MemberTypeId));
            if (SourceTypeById(item.OwnerTypeId).Kind != "record" || member.Kind is "void" or "lvalue-reference" or "rvalue-reference")
                throw new InvalidDataException("Member pointer requires an original class and object/function member type");
            SourceType source = SourceTypeById(item.TypeId);
            if (source.ExecutionRepresentationId is long physical &&
                (member.Kind == "routine" ? !Records.TryGetValue(physical, out RecordType? representation) ||
                    representation is not { RepresentationKind: "abi", IdentityKey: "abi:member-function-pointer", CaptureProof: "observed-abi-creation" }
                    : Type(physical) != "i64"))
                throw new InvalidDataException("Observed member pointer representation differs from its data/member-function category");
        }
        foreach (RoutineType routine in SourceRoutines.Values)
        {
            if (SourceTypeById(routine.TypeId).Kind != "routine" || Original(SourceTypeById(routine.ReturnTypeId)).Kind is "array" or "routine" ||
                routine.ObjectTypeId is long owner && SourceTypeById(owner).Kind != "record")
                throw new InvalidDataException("Invalid original semantic routine relationship");
            foreach (long parameter in routine.Parameters)
                if (Original(SourceTypeById(parameter)).Kind == "void") throw new InvalidDataException("Void is not an original parameter type");
        }
    }

    private string SourceEnumSignature(SourceType type, int depth)
    {
        SourceEnumType enumeration = SourceEnums[type.Id];
        string underlying = SourceTypeSignature(enumeration.UnderlyingTypeId, depth + 1, false, false);
        // C permits compatible anonymous enum declarations in distinct units.
        // Preserve nominal source identities in the object, but compare these
        // types by corresponding enumerator names/values (declaration order is
        // not part of C compatibility). C++ remains nominal.
        if (SourceLanguage == "c" && type.SourceName.Length == 0 && enumeration.EnumeratorListComplete)
            return "c-anonymous-enum(" + underlying + ";" + string.Join(';',
                SourceEnumerators.GetValueOrDefault(type.Id, []).OrderBy(item => item.SourceName, StringComparer.Ordinal)
                    .Select(item => Convert.ToHexStringLower(System.Text.Encoding.UTF8.GetBytes(item.SourceName)) + "=" + item.ValueText)) + ")";
        return "enum(" + Convert.ToHexStringLower(System.Text.Encoding.UTF8.GetBytes(enumeration.IdentityKey)) + "," + underlying + "," + enumeration.IsScoped + ")";
    }

    private (int Bits, bool Signed) SourceIntegerShape(SourceType type) => type.Kind switch
    {
        "bool" => (1, false), "signed-char" => (8, true), "unsigned-char" or "char8-t" => (8, false),
        "short" => (16, true), "unsigned-short" or "char16-t" => (16, false), "int" => (32, true), "unsigned-int" or "char32-t" => (32, false),
        "long" or "long-long" => (64, true), "unsigned-long" or "unsigned-long-long" => (64, false), "int128" => (128, true), "unsigned-int128" => (128, false),
        "char" or "wchar-t" when type.ExecutionRepresentationId is long id && ScalarTypes.IsInteger(Type(id)) => (checked((int)StorageSize(Type(id)) * 8), Type(id).StartsWith('i')),
        _ => throw new InvalidDataException($"Expected a supported original integral type (source type {type.Id}, kind {type.Kind})")
    };
}
