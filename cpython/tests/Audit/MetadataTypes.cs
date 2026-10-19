using System.Collections.Immutable;
using System.Reflection.Metadata;

// Decode generic/array method owners as well as simple names. An opaque
// <type-specification> cannot distinguish Span<T> from an allocating array ctor.
internal sealed class MetadataTypes : ISignatureTypeProvider<string, object?>
{
    public string Name(MetadataReader reader, EntityHandle handle) => handle.Kind switch {
        HandleKind.TypeDefinition => Definition(reader, (TypeDefinitionHandle)handle),
        HandleKind.TypeReference => Reference(reader, (TypeReferenceHandle)handle),
        HandleKind.TypeSpecification => reader.GetTypeSpecification((TypeSpecificationHandle)handle).DecodeSignature(this, (object?)null),
        _ => "<" + handle.Kind + ">" };
    public string Definition(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var type = reader.GetTypeDefinition(handle); string name = reader.GetString(type.Name);
        return !type.GetDeclaringType().IsNil ? Definition(reader, type.GetDeclaringType()) + "+" + name
            : reader.GetString(type.Namespace) is { Length: > 0 } ns ? ns + "." + name : name;
    }
    private string Reference(MetadataReader reader, TypeReferenceHandle handle)
    {
        var type = reader.GetTypeReference(handle); string name = reader.GetString(type.Name);
        return type.ResolutionScope.Kind == HandleKind.TypeReference ? Reference(reader, (TypeReferenceHandle)type.ResolutionScope) + "+" + name
            : reader.GetString(type.Namespace) is { Length: > 0 } ns ? ns + "." + name : name;
    }
    public string GetArrayType(string element, ArrayShape shape) => element + "[" + new string(',', shape.Rank - 1) + "]";
    public string GetByReferenceType(string element) => element + "&";
    public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
    public string GetGenericInstantiation(string type, ImmutableArray<string> args) => type + "<" + string.Join(",", args) + ">";
    public string GetGenericMethodParameter(object? context, int index) => "!!" + index;
    public string GetGenericTypeParameter(object? context, int index) => "!" + index;
    public string GetModifiedType(string modifier, string type, bool required) => type;
    public string GetPinnedType(string element) => element;
    public string GetPointerType(string element) => element + "*";
    public string GetPrimitiveType(PrimitiveTypeCode code) => code switch {
        PrimitiveTypeCode.Byte => "byte", PrimitiveTypeCode.SByte => "sbyte", PrimitiveTypeCode.Int16 => "short", PrimitiveTypeCode.UInt16 => "ushort",
        PrimitiveTypeCode.Int32 => "int", PrimitiveTypeCode.UInt32 => "uint", PrimitiveTypeCode.Int64 => "long", PrimitiveTypeCode.UInt64 => "ulong",
        PrimitiveTypeCode.Char => "char", PrimitiveTypeCode.Boolean => "bool", PrimitiveTypeCode.Single => "float", PrimitiveTypeCode.Double => "double", _ => code.ToString() };
    public string GetSZArrayType(string element) => element + "[]";
    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte kind) => Definition(reader, handle);
    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte kind) => Reference(reader, handle);
    public string GetTypeFromSpecification(MetadataReader reader, object? context, TypeSpecificationHandle handle, byte kind) => reader.GetTypeSpecification(handle).DecodeSignature(this, context);
}
