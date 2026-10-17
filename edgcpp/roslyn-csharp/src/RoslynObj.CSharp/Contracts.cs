namespace RoslynObj.CSharp;

public enum DiagnosticSeverity { Info, Warning, Error }
public enum TranslationStatus { InventoryComplete, TranslationComplete, Failed, Cancelled }
/// <summary>Synchronous build-tool boundaries; observers may request cancellation.</summary>
public enum TranslationPhase { InventoryCompleted, EmissionCompleted, BeforePublication }
public enum CapabilityStatus { Unsupported, Implemented, Exercised, NotEncountered }
/// <summary>Explicit application-visible native error-state provider contract.</summary>
public enum NativeErrorProtocol { None, LinuxX64LogicalErrno }

public sealed record TranslationDiagnostic(string Code, DiagnosticSeverity Severity, string Message,
    string? Input = null, string? Locator = null, string? Family = null);
public sealed record GeneratedFile(string RelativePath, string Sha256, long SizeBytes);
public sealed record GeneratedSource(string RelativePath, string Content);

public sealed record InputArtifact
{
    public string Path { get; init; } = "";
    public string Sha256 { get; init; } = "";
    public string? ArtifactIdentity { get; init; }
    public string Producer { get; init; } = "";
    public string EdgRevision { get; init; } = "";
    public string EdgVersion { get; init; } = "";
    public string CaptureProfile { get; init; } = "native_lowered";
    public string[] RequiredCapabilities { get; init; } = ["normalized_semantic_schema_v1", "exact_target_values_v1"];
}

/// <summary>Explicit source contract; this is never inferred from the translator host or output RID.</summary>
public sealed record TargetConfiguration
{
    public string Abi { get; init; } = "";
    public int AddressableUnitBits { get; init; }
    public string Endianness { get; init; } = "";
    public int PointerBits { get; init; }
    public int PointerAlignment { get; init; }
    public string OperatingSystem { get; init; } = "";
    public string Architecture { get; init; } = "";
    public string Libc { get; init; } = "";
    public string OutputRid { get; init; } = "";
}

/// <summary>Explicit host ELF data evidence; none of these values supplies an EDG object size.</summary>
public sealed record NativeDataAddressContract
{
    public string Version { get; init; } = "";
    public string SymbolVersion { get; init; } = "";
    public ulong ExportRelativeVirtualAddress { get; init; }
    public int SymbolSizeBytes { get; init; }
}

public sealed record ExternalBinding
{
    public string LinkageName { get; init; } = "";
    public string Kind { get; init; } = "function";
    public string Provider { get; init; } = "";
    public string Symbol { get; init; } = "";
    /// <summary>Exact native provider file; executable binding never uses ambient library lookup.</summary>
    public string? LibraryPath { get; init; }
    public string? LibrarySha256 { get; init; }
    public NativeDataAddressContract? DataAddressContract { get; init; }
    public string Signature { get; init; } = "";
    public string CallingConvention { get; init; } = "";
    public string Ownership { get; init; } = "";
    public string ErrorContract { get; init; } = "";
    public string ThreadContract { get; init; } = "";
    public string CallbackContract { get; init; } = "";
}

public sealed record RuntimeConfiguration
{
    public string Version { get; init; } = "0.1.0";
    public string[] Providers { get; init; } = [];
    public ExternalBinding[] Bindings { get; init; } = [];
    public string? RuntimeProjectPath { get; init; }
    public NativeErrorProtocol NativeErrorProtocol { get; init; }
}
public sealed record ProgramRoots
{
    public string Mode { get; init; } = "library";
    public string? EntryPoint { get; init; }
    public string[] Exports { get; init; } = [];
    public bool RetainAllDefinitions { get; init; } = true;
    public ManagedDefinitionRoot[] ManagedDefinitions { get; init; } = [];
    public ManagedStartupConfiguration? ManagedStartup { get; init; }
}
/// <summary>Explicit target lifecycle policy, separate from captured source semantics.</summary>
public sealed record ManagedStartupConfiguration
{
    public string Contract { get; init; } = "edg7-itanium-explicit-startup-atexit-v1";
    public string[] UnitInputOrder { get; init; } = [];
    public ManagedDefinitionRoot[] Definitions { get; init; } = [];
}
/// <summary>Current-input indexed discovery plus exact captured linkage selection; never a source semantic rule.</summary>
public sealed record ManagedDefinitionRoot
{
    public string InputSha256 { get; init; } = "";
    public string LookupName { get; init; } = "";
    public string LinkageName { get; init; } = "";
}
public sealed record ManagedRootSelectionReport(string LookupName, string LinkageName,
    string? FunctionName, string Signature, string SelectedLocator);
public sealed record ManagedSelectionDependencyReport(string From, string Kind, string To);
public sealed record ManagedNonProviderBodyRequirementReport(string Function, string? CapturedFunctionName,
    string BodyState, bool IsDefined, bool HasRetainedBody, bool SuppressInlineBody,
    bool DefinitionForInliningOnly, string Requirement);
public sealed record ManagedUnitSelectionReport(int UnitOrdinal, string InputArtifactIdentity, string InputSha256, ManagedSelectionReport Selection);
public sealed record ManagedSelectionBindingReport(string Kind, string LinkageName, string Declaration, string Definition, string AbiContract);
public sealed record ManagedInlineDefinitionMemberReport(int UnitOrdinal, string InputSha256, string Function,
    string? SpecialFunctionKind, string? EntryKind, bool? NoexceptEffective, string AbiContract);
public sealed record ManagedInlineRepresentativeReport(string LinkageName, string Policy, int RepresentativeUnitOrdinal,
    string Representative, IReadOnlyList<ManagedInlineDefinitionMemberReport> Members)
{
    public bool BodyEquivalenceOrOdrValidated { get; init; }
    public string StoragePolicy { get; init; } = "No local-static/TLS storage admitted or storage merged by function selection; other required storage keeps its separate captured binding/allocation policy.";
}
public sealed record ManagedConstantStorageMemberReport(int UnitOrdinal, string InputSha256, string Variable,
    string Size, string Alignment, string ElementSignedness, string Encoding, IReadOnlyList<uint> Units);
public sealed record ManagedConstantStorageRepresentativeReport(string LinkageName, string Policy,
    int RepresentativeUnitOrdinal, string Representative, IReadOnlyList<ManagedConstantStorageMemberReport> Members)
{
    public bool CppOdrValidated { get; init; }
}
public sealed record ManagedNormalEhActionReport(string Locator, string Kind, string? Target,
    string? StorageKind, string? StorageVariable, string? StorageBaseEntity, string? StorageOffset,
    bool? IsIndirect, bool? UsesThisParameterOffset, string? Condition, string? ConditionVariable, string? ComparisonValue);
public sealed record ManagedNormalEhRegionReport(string Locator, long Ordinal, string? NextRegion,
    IReadOnlyList<ManagedNormalEhActionReport> Actions);
public sealed record ManagedNormalEhMarkerReport(string Locator, string Kind, string? FunctionEhOwner,
    string? Routine, string? Region, string? StateNumber, bool? IsUnreachable, string? Initialization);
public sealed record ManagedNormalEhReport(string Function, bool HasDescriptor, string? DescriptorRoutine,
    string? RegionTable, string? ArrayTable, string? InitialState, string? NullState, string? Prologue,
    IReadOnlyList<string> Epilogues, IReadOnlyList<ManagedNormalEhRegionReport> Regions)
{
    public string? CapturedFunctionName { get; init; }
    public IReadOnlyList<ManagedNormalEhMarkerReport> Markers { get; init; } = [];
}
public sealed record ManagedSelectionReport
{
    public string Scope { get; init; } = "selected-executable-closure";
    public IReadOnlyList<ManagedUnitSelectionReport> UnitSelections { get; init; } = [];
    public IReadOnlyList<ManagedSelectionBindingReport> Bindings { get; init; } = [];
    public string DefinitionSelectionPolicy { get; init; } = "strong-only";
    public IReadOnlyList<ManagedInlineRepresentativeReport> InlineRepresentatives { get; init; } = [];
    public string PromotedConstantStoragePolicy { get; init; } = "reject";
    public IReadOnlyList<ManagedConstantStorageRepresentativeReport> ConstantStorageRepresentatives { get; init; } = [];
    public IReadOnlyList<ManagedNonProviderBodyRequirementReport> NonProviderBodyRequirements { get; init; } = [];
    public string InputArtifactIdentity { get; init; } = "";
    public string InputSha256 { get; init; } = "";
    public bool ClosureComplete { get; init; }
    public bool WholeInputInventoryComplete { get; init; }
    public bool WholeInputTranslationComplete { get; init; }
    public bool RelatedTypeMetadataComplete { get; init; }
    public int ExecutableBodiesVisited { get; init; }
    public int RequiredDeclarationCount { get; init; }
    public int StorageDefinitionCount { get; init; }
    public int RequiredVariableCount { get; init; }
    public int ScopedTypeCount { get; init; }
    public int RequiredConstantCount { get; init; }
    public long BodyOperationCount { get; init; }
    public long SelectionWorkCount { get; init; }
    public long AmbientRootRowsInspected { get; init; }
    public string AmbientRoutineDiscoveryContract { get; init; } = "edg7-itanium-native-default-startup-prefix-v1";
    public int AmbientRoutineCandidatesInspected { get; init; }
    public string UnvisitedInput { get; init; } = "Unselected bodies, storage and related metadata are not inventoried or translated; no whole-input completion is claimed.";
    public string AmbientInitialization { get; init; } = "Required ambient translation-unit initialization/teardown is checked and unsupported execution is rejected, never erased.";
    public IReadOnlyList<ManagedRootSelectionReport> Roots { get; init; } = [];
    public IReadOnlyList<ManagedRootSelectionReport> StartupRoots { get; init; } = [];
    public string? StartupExecutionContract { get; init; }
    public IReadOnlyList<ManagedNormalEhReport> NormalExceptionHandling { get; init; } = [];
    public IReadOnlyList<ManagedSelectionDependencyReport> Dependencies { get; init; } = [];
}
public sealed record OutputConfiguration
{
    public string Directory { get; init; } = "";
    public string ProjectName { get; init; } = "TranslatedProgram";
    public string NamespaceName { get; init; } = "TranslatedProgram";
    public string ClassName { get; init; } = "Program";
    public string NamePolicy { get; init; } = "source-stable";
    public string FilePolicy { get; init; } = "declaration-size";
}
public sealed record LinkPolicy
{
    public string DefinitionSelection { get; init; } = "strong-only";
    public string PromotedConstantStorage { get; init; } = "reject";
    public bool DeduplicateIdenticalArtifacts { get; init; }
    public string InitializationOrder { get; init; } = "manifest-order";
    public string GlobalState { get; init; } = "process";
}
public sealed record ResourceBudgets
{
    public long MaximumWorkingSetBytes { get; init; } = 8L * 1024 * 1024 * 1024;
    public long MaximumOperationsPerBody { get; init; } = 2_000_000;
    public long MaximumOutputBytes { get; init; } = 512L * 1024 * 1024;
    public int MaximumElapsedSeconds { get; init; } = 1800;
    public int MaximumOpenUnits { get; init; } = 1;
    public int ReaderCachedRows { get; init; } = 4096;
    public int ReaderCachedPages { get; init; } = 128;
    public int ReaderPageSize { get; init; } = 128;
    public int MaximumSelectionCandidates { get; init; } = 32;
    public int MaximumAmbientRoutineCandidates { get; init; } = 64;
    public int MaximumSelectedFunctions { get; init; } = 64;
    public int MaximumSelectedStorage { get; init; } = 128;
    public int MaximumSelectionOperationsPerBody { get; init; } = 4096;
    public int MaximumSelectedTypes { get; init; } = 256;
    public int MaximumSelectionDepth { get; init; } = 128;
}
public sealed record TranslationRequest
{
    public int ManifestVersion { get; init; } = 1;
    public string TranslatorVersion { get; init; } = "0.1.0";
    public InputArtifact[] Inputs { get; init; } = [];
    public TargetConfiguration Target { get; init; } = new();
    public RuntimeConfiguration Runtime { get; init; } = new();
    public ProgramRoots Roots { get; init; } = new();
    public OutputConfiguration Output { get; init; } = new();
    public LinkPolicy Link { get; init; } = new();
    public ResourceBudgets Budgets { get; init; } = new();
}

public sealed record InputReport(int Ordinal, string Path, string Sha256, string ArtifactIdentity,
    int FormatMajor, int FormatMinor, string Producer, string EdgRevision, string EdgVersion,
    string CaptureProfile, string LanguageMode, IReadOnlyList<CapturedCapability> Capabilities,
    IReadOnlyList<string> InvocationArguments);
public sealed record CapturedCapability(string Name, string? Value, bool Required, bool? Enabled, string State);
public sealed record SignatureParameterReport(long Ordinal, string? Name, string Type, string Role);
public sealed record SignatureReport(string Display, string ReturnType, IReadOnlyList<SignatureParameterReport> Parameters,
    string? CallingConvention, string? LanguageLinkage, bool Variadic, bool HasPrototype,
    bool? ReturnsValueAsParameter, bool ReturnsValueViaCopyConstructor);
public sealed record FunctionReport(string Locator, string? Name, string? CurrentName, string? LinkageName,
    string? Linkage, bool IsDefined, string BodyState, bool RequiredBody, string BodyClassification,
    SignatureReport Signature, long OperationCount, string? Builtin, bool AddressExposed);
public sealed record StorageReport(string Locator, string? Name, string? LinkageName, string? Linkage,
    string Type, string Classification, string? StorageDuration, string? DeclaredStorageClass,
    string InitializationKind, bool HasInitializer, bool HasConstantInitializer,
    bool ThreadLocal, bool AddressTaken, bool Weak, bool Common, bool Alias)
{
    public string? CurrentStorage { get; init; }
}
public sealed record TypeReport(string Locator, string? Name, string Kind, string? Size, string? Alignment,
    bool Incomplete, bool Dependent, string Description, long Fields, long BitFields, long Bases)
{
    public IReadOnlyList<TypeMemberReport> Members { get; init; } = [];
}
public sealed record TypeMemberReport(long Ordinal, string? Name, string Type, string? Offset,
    string? BitOffset, string? BitWidth, string? ContainerOffset);
public sealed record ImportReport(string Locator, string Kind, string? Name, string? LinkageName,
    string Signature, string? CallingConvention, bool Required, long DirectCalls, long AddressReferences,
    bool AddressExposed, string? Provider, string? ProviderSymbol, string? BindingSignature,
    string? BindingCallingConvention, string? Ownership, string? ErrorContract,
    string? ThreadContract, string? CallbackContract, string BindingStatus);
public sealed record FamilyReport(string Family, string Form, long RequiredCount, long RetainedCount,
    CapabilityStatus Status, IReadOnlyList<string> Examples, string Note);
public sealed record InitializationRootReport(string Locator, string Kind, string? Target,
    long Ordinal, bool ThreadLocal);
public sealed record InitializationRecipeReport(string Locator, string Kind, string? Target,
    string? Constructor, string? Destructor, bool ExecutionRoot, bool CleanupDependency,
    bool RetainedMetadata, string? OperationRole);
public sealed record ProgramReport
{
    public int ReportVersion { get; init; } = 1;
    public string TranslatorVersion { get; init; } = "0.1.0";
    public bool InventoryComplete { get; init; }
    public bool TranslationComplete { get; init; }
    public ManagedSelectionReport? ManagedSelection { get; init; }
    public string InventorySemantics { get; init; } = "Counts are retained entity identities per body/dependency root; they are not dynamic execution counts or an evaluation schedule. Metadata backing expressions are excluded from required execution.";
    public TargetConfiguration Target { get; init; } = new();
    public RuntimeConfiguration Runtime { get; init; } = new();
    public ProgramRoots Roots { get; init; } = new();
    public LinkPolicy Link { get; init; } = new();
    public ResourceBudgets Budgets { get; init; } = new();
    public OutputConfiguration Output { get; init; } = new();
    public IReadOnlyList<InputReport> Inputs { get; init; } = [];
    public IReadOnlyList<FunctionReport> Functions { get; init; } = [];
    public IReadOnlyList<StorageReport> Storage { get; init; } = [];
    public IReadOnlyList<TypeReport> Types { get; init; } = [];
    public IReadOnlyList<ImportReport> Imports { get; init; } = [];
    public IReadOnlyList<FamilyReport> Families { get; init; } = [];
    public IReadOnlyList<InitializationRootReport> InitializationRoots { get; init; } = [];
    public IReadOnlyList<InitializationRecipeReport> InitializationRecipes { get; init; } = [];
    public IReadOnlyList<TranslationDiagnostic> Diagnostics { get; init; } = [];
}
public sealed record TranslationMetrics(double ElapsedSeconds, long PeakWorkingSetBytes, long ReaderCommands, long ReaderRowsRead);
public sealed record TranslationResult(TranslationStatus Status, IReadOnlyList<TranslationDiagnostic> Diagnostics,
    IReadOnlyList<GeneratedFile> GeneratedFiles, ProgramReport ProgramReport, TranslationMetrics Metrics)
{
    public bool Success => Status is TranslationStatus.InventoryComplete or TranslationStatus.TranslationComplete;
}
