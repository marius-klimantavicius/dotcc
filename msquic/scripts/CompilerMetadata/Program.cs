using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

// Inspect the assembly table and custom-attribute blobs; never load the target
// assemblies into the runtime or resolve their dependencies.
try
{
    if (args.Length == 0)
        throw new ArgumentException("Supply one or more managed assembly paths.");
    var versions = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (string path in args)
        versions.Add(Path.GetFileName(path), ReadVersion(path));
    Console.WriteLine(JsonSerializer.Serialize(versions));
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}

static string ReadVersion(string path)
{
    using var stream = File.OpenRead(path);
    using var pe = new PEReader(stream);
    if (!pe.HasMetadata)
        throw new BadImageFormatException($"No managed metadata: {path}");
    MetadataReader reader = pe.GetMetadataReader();
    if (!reader.IsAssembly)
        throw new BadImageFormatException($"Not an assembly: {path}");

    string? version = null;
    foreach (CustomAttributeHandle handle in reader.GetAssemblyDefinition().GetCustomAttributes())
    {
        CustomAttribute attribute = reader.GetCustomAttribute(handle);
        EntityHandle type;
        StringHandle methodName;
        BlobHandle signature;
        switch (attribute.Constructor.Kind)
        {
            case HandleKind.MemberReference:
                MemberReference member = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                type = member.Parent;
                methodName = member.Name;
                signature = member.Signature;
                break;
            case HandleKind.MethodDefinition:
                MethodDefinition method = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
                type = method.GetDeclaringType();
                methodName = method.Name;
                signature = method.Signature;
                break;
            default:
                throw new BadImageFormatException($"Invalid attribute constructor: {path}");
        }

        string name;
        string ns;
        switch (type.Kind)
        {
            case HandleKind.TypeReference:
                TypeReference reference = reader.GetTypeReference((TypeReferenceHandle)type);
                name = reader.GetString(reference.Name);
                ns = reader.GetString(reference.Namespace);
                break;
            case HandleKind.TypeDefinition:
                TypeDefinition definition = reader.GetTypeDefinition((TypeDefinitionHandle)type);
                name = reader.GetString(definition.Name);
                ns = reader.GetString(definition.Namespace);
                break;
            default:
                continue;
        }
        if (ns != "System.Reflection" || name != "AssemblyInformationalVersionAttribute")
            continue;

        BlobReader ctor = reader.GetBlobReader(signature);
        SignatureHeader header = ctor.ReadSignatureHeader();
        if (reader.GetString(methodName) != ".ctor" || header.Kind != SignatureKind.Method ||
            !header.IsInstance || header.IsGeneric || ctor.ReadCompressedInteger() != 1 ||
            ctor.ReadSignatureTypeCode() != SignatureTypeCode.Void ||
            ctor.ReadSignatureTypeCode() != SignatureTypeCode.String || ctor.RemainingBytes != 0)
            throw new BadImageFormatException($"Unexpected informational-version constructor: {path}");

        BlobReader value = reader.GetBlobReader(attribute.Value);
        if (value.ReadUInt16() != 1)
            throw new BadImageFormatException($"Invalid informational-version attribute prolog: {path}");
        string? candidate = value.ReadSerializedString();
        if (string.IsNullOrEmpty(candidate) || value.ReadUInt16() != 0 || value.RemainingBytes != 0)
            throw new BadImageFormatException($"Invalid informational-version attribute value: {path}");
        if (version is not null)
            throw new BadImageFormatException($"Duplicate informational-version attributes: {path}");
        version = candidate;
    }
    return version ?? throw new BadImageFormatException($"Missing informational-version attribute: {path}");
}
