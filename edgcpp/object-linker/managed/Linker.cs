namespace DotObj;

internal sealed partial class Linker
{
    public IReadOnlyList<ObjectFile> Objects { get; }
    public IReadOnlyList<ObjectFile> EmissionObjects { get; }
    public IReadOnlyList<Symbol> Definitions { get; }
    public IReadOnlyList<Symbol> InitializationActions { get; }
    public IReadOnlyList<StorageSymbol> StorageDefinitions { get; }
    public Dictionary<string, Symbol> Externs { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, StorageSymbol> ExternStorages { get; } = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> requiredStorageExtents = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> requiredStorageAlignments = new(StringComparer.Ordinal);
    public Dictionary<string, string> StorageNames { get; } = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StorageSymbol> storageDefinitions = new(StringComparer.Ordinal);
    private readonly Names names = new();
    private readonly Dictionary<string,Symbol> unresolvedWeakFunctions = new(StringComparer.Ordinal);
    public bool IsUndefinedWeak(Symbol symbol) => unresolvedWeakFunctions.ContainsKey(symbol.Key) && !Externs.ContainsKey(symbol.Key);
    private readonly bool allowExterns;
    private readonly bool sharedExceptionProvider;
    private readonly Dictionary<string, Symbol> definitions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> signatures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> sourceSignatures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> semanticFunctionSignatures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Symbol> completeDeclarations = new(StringComparer.Ordinal);
    private readonly Dictionary<ObjectFile, Dictionary<long, SourceRecordField[]>> sourceFieldIndex = [];
    public Dictionary<string, string> Names { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, (ObjectFile Object, RecordType Record)> Records { get; } = new(StringComparer.Ordinal);

    public Linker(IReadOnlyList<ObjectFile> objects, string className, bool allowExterns = false, IReadOnlySet<ObjectFile>? providers = null, bool sharedExceptionProvider = true)
    {
        this.allowExterns = allowExterns;
        this.sharedExceptionProvider = sharedExceptionProvider;
        Objects = objects.OrderBy(obj => obj.UnitId, StringComparer.Ordinal).ToArray();
        if (Objects.Select(obj => obj.UnitId).Distinct(StringComparer.Ordinal).Count() != Objects.Count)
            throw new InvalidDataException("The same translation unit was supplied more than once");
        var sourceConstraintContracts = new Dictionary<string,string>(StringComparer.Ordinal);
        foreach (var obj in Objects)
            foreach (var contract in obj.SourceConstraintContracts)
            {
                if (sourceConstraintContracts.TryGetValue(contract.Key, out string? previous) && previous != contract.Value)
                    throw new InvalidDataException("Original concept/constraint definition mismatch: " + contract.Key);
                sourceConstraintContracts[contract.Key] = contract.Value;
            }
        VerifyEnumDefinitions();
        ResolveDeferredRoutineAbis();
        // A provider's coalescible copy must not force archive extraction when
        // the primary inputs already supply that body. Strong definitions still
        // take precedence below, independently of this stable preference.
        var deferredDeclarationSymbols = Objects.SelectMany(obj => obj.DeferredFunctionDeclarations.Keys
            .Select(id => new Symbol(obj, obj.Entities[id]).Key)).ToHashSet(StringComparer.Ordinal);
        var resolutionOrder = objects.OrderBy(obj => providers?.Contains(obj) == true).ToArray();
        foreach (ObjectFile obj in resolutionOrder)
            foreach (Entity entity in obj.Entities.Values)
            {
                var symbol = new Symbol(obj, entity);
                if (deferredDeclarationSymbols.Contains(symbol.Key) &&
                    (!obj.EntitySourceRoutines.ContainsKey(entity.Id) || obj.SourceFunctionKinds.GetValueOrDefault(entity.Id) != "ordinary"))
                    throw new InvalidDataException("Deferred original declaration cannot resolve to a generated or special-member origin");
                string signature = CanonicalExecutionType(obj.CallableRoutine(entity.Id));
                if (signatures.TryGetValue(symbol.Key, out string? prior) && prior != signature)
                    throw new InvalidDataException($"Signature mismatch for {entity.ExternalKey ?? entity.SourceName}: {prior} versus {symbol.Signature}");
                signatures[symbol.Key] = signature;
                if (obj.EntitySourceRoutines.TryGetValue(entity.Id, out RoutineType? original))
                {
                    string semanticSignature = obj.SourceTypeSignature(original.TypeId);
                    if (semanticFunctionSignatures.TryGetValue(symbol.Key, out string? priorSemantic) && priorSemantic != semanticSignature)
                        throw new InvalidDataException($"Original semantic signature mismatch for {entity.ExternalKey ?? entity.SourceName}");
                    semanticFunctionSignatures[symbol.Key] = semanticSignature;
                }
                if (!obj.DeferredFunctionDeclarations.ContainsKey(entity.Id))
                {
                    string sourceSignature = obj.FunctionSourceSignature(entity.Id);
                    if (sourceSignatures.TryGetValue(symbol.Key, out string? priorSource) && priorSource != sourceSignature)
                        throw new InvalidDataException($"Source signature mismatch for {entity.ExternalKey ?? entity.SourceName}: {priorSource} versus {sourceSignature}");
                    sourceSignatures[symbol.Key] = sourceSignature;
                    completeDeclarations.TryAdd(symbol.Key, symbol);
                }
                if (entity.Definition != "declaration")
                {
                    if (!definitions.TryGetValue(symbol.Key, out Symbol? existing)) definitions.Add(symbol.Key, symbol);
                    else if (existing.Entity.Definition == "strong" && entity.Definition == "strong")
                        throw new InvalidDataException($"Duplicate strong definition: {entity.ExternalKey ?? entity.SourceName}");
                    else if (entity.Definition == "strong") definitions[symbol.Key] = symbol;
                }
            }
        var storageSignatures = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (ObjectFile obj in resolutionOrder)
            foreach (Storage storage in obj.Storages.Values)
            {
                var symbol = new StorageSymbol(obj, storage);
                requiredStorageAlignments[symbol.Key] = Math.Max(requiredStorageAlignments.GetValueOrDefault(symbol.Key, 1), obj.StorageRequiredAlignment(storage));
                if (storageSignatures.TryGetValue(symbol.Key, out string? prior) && !CompatibleStorage(prior, symbol.Signature))
                    throw new InvalidDataException($"Storage type/TLS mismatch: {storage.ExternalKey ?? storage.SourceName}");
                storageSignatures[symbol.Key] = symbol.Signature;
                if (storage.Definition == "declaration") continue;
                if (!storageDefinitions.TryGetValue(symbol.Key, out StorageSymbol? existing)) storageDefinitions.Add(symbol.Key, symbol);
                else if (existing.Storage.Definition == "strong" && storage.Definition == "strong")
                    throw new InvalidDataException($"Duplicate strong storage definition: {storage.ExternalKey ?? storage.SourceName}");
                else if (storage.Definition == "strong" || existing.Storage.Definition == "tentative" && storage.Definition == "linkonce")
                    storageDefinitions[symbol.Key] = symbol;
            }
        foreach (StorageSymbol definition in storageDefinitions.Values)
            if (definition.Storage.Alignment < requiredStorageAlignments[definition.Key])
                throw new InvalidDataException($"Storage definition does not satisfy the required declaration alignment: {definition.Storage.ExternalKey ?? definition.Storage.SourceName}");
        SelectProviderClosure(providers);
        EmissionObjects = Objects.Where(obj => !providerObjects.Contains(obj) || selectedProviderObjects.Contains(obj)).ToArray();
        // Equal-priority GNU actions follow original link input order and the
        // EDG routine-discovery ordinal within that translation unit.
        InitializationActions = objects.Select((obj, index) => (Object: obj, InputIndex: index))
            .Where(item => EmissionObjects.Contains(item.Object))
            .SelectMany(item => item.Object.InitializationActions.Select(action =>
                (item.Object, item.InputIndex, Action: action)))
            .OrderBy(item => item.Action.Priority).ThenBy(item => item.InputIndex).ThenBy(item => item.Action.Ordinal)
            .Select(item => definitions[new Symbol(item.Object, item.Object.Entities[item.Action.EntityId]).Key]).ToArray();
        foreach (ObjectFile obj in EmissionObjects)
            foreach (RecordType record in obj.Records.Values)
            {
                if (Records.TryGetValue(record.Key, out var prior))
                {
                    if (prior.Record.Size == 0 || record.Size == 0)
                    {
                        if (prior.Record.LayoutKind != record.LayoutKind || prior.Record.SourceName != record.SourceName ||
                            prior.Record.RepresentationKind != record.RepresentationKind)
                            throw new InvalidDataException("Incomplete record identity mismatch");
                        if (prior.Record.Size == 0 && record.Size != 0) Records[record.Key] = (obj, record);
                        continue;
                    }
                    RecordField[] expected = prior.Object.RecordFields(prior.Record.TypeId), actual = obj.RecordFields(record.TypeId);
                    if (record.SourceName != prior.Record.SourceName || record.Size != prior.Record.Size || record.Alignment != prior.Record.Alignment || record.LayoutKind != prior.Record.LayoutKind ||
                        record.RepresentationKind != prior.Record.RepresentationKind || record.ValueCopyKind != prior.Record.ValueCopyKind ||
                        CompleteRecordSignature(obj, record) != CompleteRecordSignature(prior.Object, prior.Record) ||
                        !CompatibleOriginalRecord(prior.Object, prior.Record, obj, record) ||
                        !CompatibleBaseSubobjects(prior.Object, prior.Record, obj, record) ||
                        expected.Length != actual.Length || actual.Where((field, index) => field.SourceName != expected[index].SourceName ||
                            field.ByteOffset != expected[index].ByteOffset || field.BitOffset != expected[index].BitOffset || field.BitWidth != expected[index].BitWidth || field.BitSigned != expected[index].BitSigned || CanonicalExecutionType(obj.Type(field.TypeId)) != CanonicalExecutionType(prior.Object.Type(expected[index].TypeId)) ||
                            field.FieldRole != expected[index].FieldRole || field.ObservedAlignment != expected[index].ObservedAlignment ||
                            FieldBaseOrdinal(obj, field) != FieldBaseOrdinal(prior.Object, expected[index]) ||
                            FieldSourceSignature(obj, field) != FieldSourceSignature(prior.Object, expected[index])).Any())
                        throw new InvalidDataException($"Record layout mismatch for {record.IdentityKey}");
                }
                else Records.Add(record.Key, (obj, record));
            }
        Definitions = definitions.Values.OrderBy(symbol => symbol.Key, StringComparer.Ordinal).ToArray();
        StorageDefinitions = storageDefinitions.Values.OrderBy(symbol => symbol.Key, StringComparer.Ordinal).ToArray();
        names.Reserve(className);
        foreach (Symbol symbol in Definitions)
            Names.Add(symbol.Key, names.Allocate(symbol.Entity.SourceName, "function_" + symbol.Entity.Id));
    }

    private void VerifyEnumDefinitions()
    {
        var definitions = new Dictionary<string, (ObjectFile Object, SourceEnumType Type)>(StringComparer.Ordinal);
        foreach (ObjectFile obj in Objects)
            foreach (SourceEnumType type in obj.SourceEnums.Values)
            {
                if (!definitions.TryGetValue(type.IdentityKey, out var prior))
                {
                    definitions.Add(type.IdentityKey, (obj, type));
                    continue;
                }
                if ((type.OwnerTypeId is long owner?obj.SourceTypeSignature(owner):null)!=
                    (prior.Type.OwnerTypeId is long priorOwner?prior.Object.SourceTypeSignature(priorOwner):null) ||
                    type.IsNonreal!=prior.Type.IsNonreal || type.IsPrototype!=prior.Type.IsPrototype ||
                    type.IsSpecialized!=prior.Type.IsSpecialized || type.OriginallyUnnamed!=prior.Type.OriginallyUnnamed ||
                    type.UnderlyingOrigin!=prior.Type.UnderlyingOrigin ||
                    type.IsTemplate != prior.Type.IsTemplate || type.IsScoped != prior.Type.IsScoped ||
                    obj.SourceTypes[type.TypeId].SourceName != prior.Object.SourceTypes[prior.Type.TypeId].SourceName ||
                    obj.SourceTypeSignature(type.UnderlyingTypeId) != prior.Object.SourceTypeSignature(prior.Type.UnderlyingTypeId))
                    throw new InvalidDataException($"Original enum type mismatch for {type.IdentityKey}");
                if (type.EnumeratorListComplete && prior.Type.EnumeratorListComplete)
                {
                    var current = obj.SourceEnumerators.GetValueOrDefault(type.TypeId, []);
                    var previous = prior.Object.SourceEnumerators.GetValueOrDefault(prior.Type.TypeId, []);
                    IEnumerable<(string Name, string Value)> currentValues = current.Select(item => (item.SourceName, obj.SourceEnumeratorValueIdentity(item)));
                    IEnumerable<(string Name, string Value)> previousValues = previous.Select(item => (item.SourceName, prior.Object.SourceEnumeratorValueIdentity(item)));
                    if (obj.SourceLanguage == "c" && prior.Object.SourceLanguage == "c")
                    {
                        currentValues = currentValues.OrderBy(item => item.Name, StringComparer.Ordinal);
                        previousValues = previousValues.OrderBy(item => item.Name, StringComparer.Ordinal);
                    }
                    if (!currentValues.SequenceEqual(previousValues))
                        throw new InvalidDataException($"Original enum definition mismatch for {type.IdentityKey}");
                }
                if (type.EnumeratorListComplete && !prior.Type.EnumeratorListComplete) definitions[type.IdentityKey] = (obj, type);
            }
    }

    private static string? FieldSourceSignature(ObjectFile obj, RecordField field) =>
        field.SourceTypeId is long id ? obj.SourceTypeSignature(id) : null;

    private static string? CompleteRecordSignature(ObjectFile obj, RecordType record) =>
        record.CompleteTypeId is long id ? obj.Type(id) : null;

    private static long? FieldBaseOrdinal(ObjectFile obj, RecordField field) =>
        field.SourceBaseId is long id ? obj.SourceBases[id].Ordinal : null;

    private static bool CompatibleBaseSubobjects(ObjectFile leftObject, RecordType left,
        ObjectFile rightObject, RecordType right)
    {
        string[] VirtualPlacements(ObjectFile obj, long type) => obj.RecordVirtualBases(type)
            .Select(item => obj.Type(item.CompleteBaseTypeId) + ":" +
                (item.PhysicalViewTypeId is long view ? obj.Type(view) : "empty") + ":" + item.ByteOffset + ":" + item.Extent + ":" +
                (item.FieldId is long field ? obj.Fields[field].Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture) : "alias"))
            .Order(StringComparer.Ordinal).ToArray();
        if (!VirtualPlacements(leftObject, left.TypeId).SequenceEqual(VirtualPlacements(rightObject, right.TypeId))) return false;
        var expected = leftObject.RecordBases(left.TypeId);
        var actual = rightObject.RecordBases(right.TypeId);
        return expected.Length == actual.Length && !actual.Where((item, index) =>
            rightObject.SourceBases[item.SourceBaseId].Ordinal != leftObject.SourceBases[expected[index].SourceBaseId].Ordinal ||
            rightObject.Type(item.CompleteBaseTypeId) != leftObject.Type(expected[index].CompleteBaseTypeId) ||
            (item.PhysicalViewTypeId is long rightView ? rightObject.Type(rightView) : null) !=
                (expected[index].PhysicalViewTypeId is long leftView ? leftObject.Type(leftView) : null) ||
            item.ByteOffset != expected[index].ByteOffset || item.Extent != expected[index].Extent ||
            (item.FieldId is long rightField ? rightObject.Fields[rightField].Ordinal : (long?)null) !=
                (expected[index].FieldId is long leftField ? leftObject.Fields[leftField].Ordinal : (long?)null)).Any();
    }

    private SourceRecordField[] OriginalFields(ObjectFile obj, long id)
    {
        if (!sourceFieldIndex.TryGetValue(obj, out var index))
            sourceFieldIndex.Add(obj, index = obj.SourceFields.Values.GroupBy(field => field.SourceRecordId)
                .ToDictionary(group => group.Key, group => group.OrderBy(field => field.Ordinal).ToArray()));
        return index.GetValueOrDefault(id, []);
    }

    private bool CompatibleOriginalRecord(ObjectFile leftObject, RecordType left,
        ObjectFile rightObject, RecordType right)
    {
        if (left.SourceRecordId is not long leftId || right.SourceRecordId is not long rightId)
            return left.SourceRecordId is null && right.SourceRecordId is null;
        SourceRecord a = leftObject.SourceRecords[leftId], b = rightObject.SourceRecords[rightId];
        if (a.IdentityKey != b.IdentityKey || a.SourceName != b.SourceName || a.RecordKind != b.RecordKind ||
            a.IsTriviallyCopyable != b.IsTriviallyCopyable || a.IsPacked != b.IsPacked ||
            a.LayoutKnown != b.LayoutKnown || a.IsDependent != b.IsDependent || a.IsPrototypeInstantiation != b.IsPrototypeInstantiation ||
            a.MaxMemberAlignment != b.MaxMemberAlignment || a.ExplicitAlignment != b.ExplicitAlignment ||
            leftObject.SourceLanguage == "c++" && rightObject.SourceLanguage == "c++" &&
            leftObject.RecordOperations.GetValueOrDefault(leftId) != rightObject.RecordOperations.GetValueOrDefault(rightId)) return false;
        var expectedBases = leftObject.OriginalBases(leftId);
        var actualBases = rightObject.OriginalBases(rightId);
        if (expectedBases.Length != actualBases.Length || actualBases.Where((item, index) =>
            item.Ordinal != expectedBases[index].Ordinal || item.AccessKind != expectedBases[index].AccessKind ||
            item.IsVirtual != expectedBases[index].IsVirtual ||
            rightObject.SourceTypeSignature(item.BaseSourceTypeId) !=
                leftObject.SourceTypeSignature(expectedBases[index].BaseSourceTypeId)).Any()) return false;
        var expected = OriginalFields(leftObject, leftId);
        var actual = OriginalFields(rightObject, rightId);
        return expected.Length == actual.Length && !actual.Where((field, index) =>
            field.SourceName != expected[index].SourceName || field.IsMutable != expected[index].IsMutable ||
            field.HasInitializer != expected[index].HasInitializer || field.IsPacked != expected[index].IsPacked ||
            field.ExplicitAlignment != expected[index].ExplicitAlignment ||
            field.HasNoUniqueAddress != expected[index].HasNoUniqueAddress || field.BitWidth != expected[index].BitWidth ||
            rightObject.SourceTypeSignature(field.SourceTypeId) != leftObject.SourceTypeSignature(expected[index].SourceTypeId)).Any();
    }

    public Symbol Resolve(ObjectFile obj, long id)
    {
        var declaration = new Symbol(obj, obj.Entities[id]);
        if (definitions.TryGetValue(declaration.Key, out Symbol? definition)) return definition;
        if (declaration.Entity.IsWeak)
        {
            unresolvedWeakFunctions.TryAdd(declaration.Key,declaration);
            return declaration;
        }
        if (obj.DeferredFunctionDeclarations.ContainsKey(id))
        {
            if (!completeDeclarations.TryGetValue(declaration.Key, out Symbol? completed) ||
                IsUnresolvedRoutine(CanonicalExecutionType(declaration.Signature)))
                throw new InvalidDataException($"Deferred function declaration requires a completed ABI provider: {declaration.Entity.ExternalKey}");
            declaration = completed;
        }
        if ((!allowExterns && RuntimeArrayProvider.TryGetMethod(declaration) is null) || declaration.Entity.Linkage != "external")
            throw new InvalidDataException($"Unresolved function: {declaration.Entity.ExternalKey ?? declaration.Entity.SourceName} (referenced from {obj.PrimarySource})");
        if (Externs.TryAdd(declaration.Key, declaration)) Names.Add(declaration.Key, names.Allocate(declaration.Entity.SourceName, "extern_" + declaration.Entity.Id));
        return Externs[declaration.Key];
    }

    private static bool CompatibleStorage(string left, string right)
    {
        if (left == right) return true;
        int a = left.LastIndexOf(':'), b = right.LastIndexOf(':');
        if (a < 0 || b < 0 || left[a..] != right[b..]) return false;
        string x = left[..a], y = right[..b];
        return ScalarTypes.IsArray(x) && ScalarTypes.IsArray(y) && ScalarTypes.Element(x) == ScalarTypes.Element(y) &&
            (ScalarTypes.IsIncompleteArray(x) || ScalarTypes.IsIncompleteArray(y));
    }

    public StorageSymbol ResolveStorage(ObjectFile obj, long id)
    {
        var declaration = new StorageSymbol(obj, obj.Storages[id]);
        if (storageDefinitions.TryGetValue(declaration.Key, out StorageSymbol? definition)) return definition;
        if (!allowExterns || declaration.Storage.Linkage != "external")
            throw new InvalidDataException($"Unresolved storage: {declaration.Storage.ExternalKey ?? declaration.Storage.SourceName}");
        ExternStorages.TryAdd(declaration.Key, declaration);
        return ExternStorages[declaration.Key];
    }

    public long StorageMinimumAlignment(StorageSymbol symbol) => Math.Max(
        symbol.Object.StorageRequiredAlignment(symbol.Storage), requiredStorageAlignments.GetValueOrDefault(symbol.Key, 1));

    public long StorageMinimumSize(StorageSymbol symbol) => Math.Max(
        symbol.Object.StorageSize(symbol.Object.Type(symbol.Storage.TypeId)),
        requiredStorageExtents.GetValueOrDefault(symbol.Key));

    private void RequireStorageExtent(ObjectFile obj, long id, long displacement)
    {
        StorageSymbol target = ResolveStorage(obj, id);
        long size = target.Object.StorageSize(target.Object.Type(target.Storage.TypeId));
        // A one-past address is permitted. Unknown declaration extents are
        // obligations on a resolved definition or on the external provider,
        // rather than evidence that a valid object occupies zero bytes.
        if (displacement < 0 ||
            (target.Storage.Definition != "declaration" || size != 0) && displacement > size)
            throw new InvalidDataException($"Storage address exceeds resolved extent: {target.Storage.SourceName}");
        requiredStorageExtents[target.Key] = Math.Max(requiredStorageExtents.GetValueOrDefault(target.Key), displacement);
    }

    public Symbol Entry(string key)
    {
        var matches = Definitions.Where(symbol => symbol.Entity.Linkage == "external" &&
            (symbol.Entity.ExternalKey == key || symbol.Entity.SourceName == key)).ToArray();
        if (matches.Length != 1) throw new InvalidDataException($"Entry must identify one externally defined function: {key}");
        Symbol entry = matches[0];
        var parameters = entry.Object.Parameters.GetValueOrDefault(entry.Entity.Id, []);
        string result = entry.Object.Type(entry.Entity.ReturnTypeId);
        bool noArguments = parameters.Count == 0 && result is "i32" or "void";
        bool commandLine = result == "i32" && parameters.SequenceEqual(["i32", "ptr(ptr(i8))"]);
        if (entry.Object.Routines[entry.Entity.RoutineTypeId].Variadic || !(noArguments || commandLine))
            throw new InvalidDataException("Executable entry must be void(), i32(), or i32(i32, ptr(ptr(i8)))");
        return entry;
    }

    public bool RequiresExceptionProvider { get; private set; }
    public Dictionary<(ObjectFile Object, long Id), SourceType> UsedCatchTypes { get; } = [];
    public bool RequiresSourceHandlers { get; private set; }
    public bool RequiresDynamicSpecifications { get; private set; }
    public Dictionary<(ObjectFile Object, long Id), ExceptionType> UsedExceptionTypes { get; } = [];

    public void VerifyBodies()
    {
        var exceptionShapes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Symbol symbol in Definitions)
        {
            FunctionBody body = symbol.Object.ReadBody(symbol.Entity.Id);
            if(symbol.Object.DynamicExceptionBoundaries.TryGetValue(symbol.Entity.Id,out long specificationId))
            {
                RequiresDynamicSpecifications=RequiresExceptionProvider=RequiresSourceHandlers=true;
                if(!allowExterns)throw new InvalidDataException("Dynamic exception specifications require the shared unexpected provider; use --allow-externs for generation-only output");
                foreach(var permitted in symbol.Object.DynamicExceptionSpecifications[specificationId].Types)
                    UsedCatchTypes.TryAdd((symbol.Object,permitted.MatchSourceTypeId),symbol.Object.SourceTypes[permitted.MatchSourceTypeId]);
            }

            foreach (CleanupAction action in symbol.Object.FunctionCleanupActions.GetValueOrDefault(symbol.Entity.Id, []))
                _ = Resolve(symbol.Object, action.CallbackEntityId);
            foreach (AllocationCleanupAction action in symbol.Object.AllocationCleanupActions.Values.Where(action => action.FunctionEntityId == symbol.Entity.Id))
                _ = Resolve(symbol.Object, action.CallbackEntityId);
            foreach (Node node in body.Nodes.Values)
            {
                if (node.Kind == "catch" && symbol.Object.SourceHandlers[node.Id].MatchTypeId is long matchType)
                    UsedCatchTypes.TryAdd((symbol.Object, matchType), symbol.Object.SourceTypes[matchType]);
                if (node.Kind is "throw" or "rethrow" or "try")
                {
                    RequiresExceptionProvider = true;
                    RequiresSourceHandlers |= node.Kind == "try";
                    if (!sharedExceptionProvider && !allowExterns) throw new InvalidDataException("Source exceptions require the shared Edg.Runtime.CppExceptions provider; use --allow-externs for generation-only output");
                    if (node.Kind == "throw")
                    {
                        ThrowExpression expression = symbol.Object.ThrowExpressions[node.Id];
                        ExceptionType descriptor = symbol.Object.ExceptionTypes[expression.ExceptionTypeId];
                        string destructorIdentity = descriptor.SourceDestructorEntityId is long primary
                            ? new Symbol(symbol.Object, symbol.Object.Entities[primary]).Key + ":" + symbol.Object.FunctionSourceSignature(primary) : "none";
                        string shape = descriptor.PayloadSize + ":" + descriptor.PayloadAlignment + ":" + destructorIdentity + ":" +
                            string.Join(";", symbol.Object.ExceptionBases[descriptor.Id].Select(item =>
                                symbol.Object.ExceptionTypeIdentity(item.BaseSourceTypeId) + ":" + item.ByteOffset + ":" + item.IsPublic + ":" + item.IsAmbiguous).Order(StringComparer.Ordinal));
                        if (exceptionShapes.TryGetValue(descriptor.IdentityKey, out string? previous) && previous != shape)
                            throw new InvalidDataException("Exception descriptor layout or resolved base facts disagree across translation units");
                        exceptionShapes[descriptor.IdentityKey] = shape;
                        UsedExceptionTypes.TryAdd((symbol.Object, expression.ExceptionTypeId), descriptor);
                        if (expression.DestructorEntityId is long callback) _ = Resolve(symbol.Object, callback);
                    }
                }
                if (node.Kind is "call" or "register-cleanup" or "function-address") _ = Resolve(symbol.Object, node.EntityId!.Value);
                if (node.Kind == "storage")
                {
                    _ = ResolveStorage(symbol.Object, node.StorageId!.Value);
                    // A completed expression view can also refer to an extern
                    // declaration whose major bound is still unknown here.
                    // Its retained extent is an obligation on the provider.
                    if (node.TypeId != symbol.Object.Storages[node.StorageId.Value].TypeId)
                        RequireStorageExtent(symbol.Object, node.StorageId.Value, symbol.Object.StorageSize(symbol.Object.Type(node.TypeId!.Value)));
                }
                if (node.Kind == "storage-address") RequireStorageExtent(symbol.Object, node.StorageId!.Value,
                    long.Parse(node.ConstantText!, System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        foreach (StorageSymbol symbol in StorageDefinitions)
            foreach (Relocation relocation in symbol.Object.Relocations.GetValueOrDefault(symbol.Storage.Id, []))
                if (relocation.TargetEntityId is long entity) _ = Resolve(symbol.Object, entity);
                else RequireStorageExtent(symbol.Object, relocation.TargetStorageId!.Value, relocation.Addend);
        // Resolve every referenced function first, including late externs.
        // Synthetic ref properties must not take a source function's name or
        // reserve its name through a generated get_<property> accessor.
        foreach (StorageSymbol symbol in StorageDefinitions.Concat(ExternStorages.Values.OrderBy(symbol => symbol.Key, StringComparer.Ordinal)))
            if (!StorageNames.ContainsKey(symbol.Key))
                StorageNames.Add(symbol.Key, names.AllocateProperty(symbol.Storage.SourceName,
                    (ExternStorages.ContainsKey(symbol.Key) ? "extern_storage_" : "__literal_") + symbol.Storage.Id));
    }
}
