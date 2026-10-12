namespace RoslynObj;

public sealed class CppModule : SemanticEntity
{
    internal CppModule(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public ModuleDetails Details { get; }
    public string? Name => Details.Name;
    public SemanticKind Kind => Details.Kind;
    public SemanticKind FileKind => Details.FileKind;
    public string? ResolvedFile => Details.ResolvedFile;
    public string? ResolvedHeader => Details.ResolvedHeader;
    public CppModule? Unit => Details.Unit;
    public bool UsesAngleBrackets => Details.UsesAngleBrackets;
    public bool IsInternalPartition => Details.IsInternalPartition;
    public bool SuppressMacroExport => Details.SuppressMacroExport;
    public bool ContainsUnsupportedConstructs => Details.ContainsUnsupportedConstructs;
}

public sealed class ModuleImport : SemanticEntity
{
    internal ModuleImport(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public ModuleImportDetails Details { get; }
    public CppModule Module => Details.Module;
    public SourcePosition? SourcePosition => Details.SourcePosition;
    public SourcePosition? ModuleNamePosition => Details.ModuleNamePosition;
    public bool ImplementationUnitImportingSelf => Details.ImplementationUnitImportingSelf;
}
