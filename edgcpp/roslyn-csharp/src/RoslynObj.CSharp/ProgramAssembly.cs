namespace RoslynObj.CSharp;

public sealed record ProgramAssemblyOptions
{
    public int MaximumOpenUnits { get; init; } = 2;
    public bool DeduplicateIdenticalArtifacts { get; init; }
    public IReadOnlyList<string> InputSha256 { get; init; } = [];
    public IReadOnlyList<ExternalBinding> ExternalBindings { get; init; } = [];
}

public sealed record CapturedLinkName(string Name, string Linkage, string Origin,
    string? CurrentName, bool? NameHasBeenMangled, string? MangledEncoding);
public sealed record AssemblyDeclarationReport(string Locator, int UnitOrdinal, string Kind,
    string? SourceName, CapturedLinkName? ExternalName, string Classification, bool Required,
    string? SelectedDefinition, string? Provider, string Decision);
public sealed record ProgramAssemblyReport(IReadOnlyList<string> ArtifactIdentities,
    IReadOnlyList<AssemblyDeclarationReport> Declarations, IReadOnlyList<TranslationDiagnostic> Diagnostics);
public sealed record ProgramFunction(string Locator, int UnitOrdinal, FunctionSymbol Definition,
    CapturedLinkName? ExternalName, IReadOnlyList<FunctionSymbol> Declarations)
{
    public int SelectedOrdinal { get; init; }
}
public sealed record ProgramStorage(string Locator, int UnitOrdinal, VariableSymbol Definition,
    CapturedLinkName? ExternalName, IReadOnlyList<VariableSymbol> Declarations)
{
    public int SelectedOrdinal { get; init; }
}

/// <summary>A small, explicitly bounded assembly view. Input objects must remain open while using its symbols.</summary>
public sealed class ProgramAssembly
{
    private readonly IReadOnlyDictionary<FunctionSymbol, ProgramFunction> functions;
    private readonly IReadOnlyDictionary<VariableSymbol, ProgramStorage> storage;
    internal ProgramAssembly(IReadOnlyList<ProgramFunction> selectedFunctions, IReadOnlyList<ProgramStorage> selectedStorage,
        IReadOnlyDictionary<FunctionSymbol, ProgramFunction> functions, IReadOnlyDictionary<VariableSymbol, ProgramStorage> storage,
        ProgramAssemblyReport report)
    {
        Functions = selectedFunctions; Storage = selectedStorage; this.functions = functions; this.storage = storage; Report = report;
    }
    public IReadOnlyList<ProgramFunction> Functions { get; }
    public IReadOnlyList<ProgramStorage> Storage { get; }
    public ProgramAssemblyReport Report { get; }
    public ProgramFunction? Resolve(FunctionSymbol declaration) => functions.GetValueOrDefault(declaration);
    public ProgramStorage? Resolve(VariableSymbol declaration) => storage.GetValueOrDefault(declaration);
}

public sealed record ProgramAssemblyResult(ProgramAssembly? Program, ProgramAssemblyReport Report)
{
    public IReadOnlyList<TranslationDiagnostic> Diagnostics => Report.Diagnostics;
    public bool Success => Program is not null && !Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
