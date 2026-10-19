using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

// The v1 byte auditor remains intact. This reader proves the expanded fixed-width
// data contract against the exact PE; it never executes or rewrites the product.
internal static class TypedAudit
{
    public static void Run(string[] args, JsonElement manifest)
    {
        string Digest(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        Require(Digest(manifest.GetProperty("translationReceipt").GetString()!) == manifest.GetProperty("receiptSha256").GetString(), "receipt changed");
        foreach (var file in manifest.GetProperty("files").EnumerateArray())
            Require(Digest(file.GetProperty("path").GetString()!) == file.GetProperty("sha256").GetString(), "source changed");
        using var pe = new PEReader(File.OpenRead(args[0]));
        var metadata = pe.GetMetadataReader();
        var types = new TypeNames();
        var methods = metadata.MethodDefinitions.ToDictionary(handle => MetadataTokens.GetToken(handle), handle => new Method(metadata, pe, types, handle));
        var byName = methods.Values.GroupBy(method => method.Key).ToDictionary(group => group.Key, group => group.ToArray());
        var properties = manifest.GetProperty("properties").EnumerateArray().ToArray();
        var views = manifest.GetProperty("typedViews").EnumerateArray().ToArray();
        string literalOwner = properties.Select(row => row.GetProperty("owner").GetString()!).Distinct().Single();
        string productOwner = literalOwner[..literalOwner.LastIndexOf('+')];
        string runtimeOwner = productOwner + "+Libc";
        Require(properties.Select(row => "get_" + row.GetProperty("name").GetString()).Order().SequenceEqual(
            methods.Values.Where(method => method.Owner == literalOwner && Regex.IsMatch(method.Name, "^get_(B|P(ushort|uint|ulong)_)[0-9A-F]{64}$"))
                .Select(method => method.Name).Order()), "incomplete backing getter manifest");
        Require(views.Select(row => row.GetProperty("owner").GetString() + ".get_Values").Order().SequenceEqual(
            methods.Values.Where(method => method.Owner.StartsWith(productOwner + "+__DotccPrimitiveView_", StringComparison.Ordinal) && method.Name == "get_Values")
                .Select(method => method.Key).Order()), "incomplete typed view manifest");
        var observed = new List<object>();
        var observedViews = new List<object>();
        var getterTokens = new HashSet<int>();
        var propertyTokens = new Dictionary<string, int>();
        var dataTokens = new HashSet<int>();
        Method Find(JsonElement row, string name)
        {
            string key = row.GetProperty("owner").GetString() + "." + name;
            Require(byName.TryGetValue(key, out var candidates) && candidates.Length == 1, "missing/ambiguous getter: " + key);
            var method = candidates![0];
            Require((method.Attributes & MethodAttributes.Static) != 0 && method.IL.Length > 0, "nonstatic/empty getter: " + key);
            Require(method.Instructions.All(instruction => instruction.Code != OpCodes.Newarr && !instruction.Code.Name!.StartsWith("conv.", StringComparison.Ordinal)), "allocating/converting getter: " + key);
            getterTokens.Add(method.Token);
            return method;
        }
        foreach (var row in properties)
        {
            string name = row.GetProperty("name").GetString()!;
            var method = Find(row, "get_" + name);
            string element = row.GetProperty("elementType").GetString()!;
            int width = row.GetProperty("elementWidth").GetInt32(), length = row.GetProperty("length").GetInt32();
            Require(method.ReturnType == "System.ReadOnlySpan`1<" + element + ">", "wrong backing signature: " + method.Key);
            Require(Width(element) == width && length > 0 && length % width == 0 && length / width <= 4096, "invalid backing width/extent");
            var calls = method.Instructions.Where(IsCall).Select(instruction => Resolve(metadata, types, instruction.Token)).ToArray();
            Require(calls.All(call => call.Owner == "System.Runtime.CompilerServices.RuntimeHelpers" && call.Name is "CreateSpan" or "CreateReadOnlySpan"
                || call.Owner == method.ReturnType && call.Name == ".ctor"), "unexpected backing call: " + method.Key);
            var fields = method.Instructions.Where(instruction => instruction.Code == OpCodes.Ldtoken || instruction.Code == OpCodes.Ldsflda)
                .Where(instruction => (instruction.Token & unchecked((int)0xff000000)) == 0x04000000).ToArray();
            Require(fields.Length == 1, "backing must reference one exact RVA field: " + method.Key);
            var field = metadata.GetFieldDefinition(MetadataTokens.FieldDefinitionHandle(fields[0].Token & 0xffffff));
            int rva = field.GetRelativeVirtualAddress();
            string fieldType = field.DecodeSignature(types, (object?)null);
            int extent = FieldWidth(metadata, types, fieldType);
            Require(rva > 0 && extent == length, "RVA field extent differs: " + method.Key + " " + extent + "/" + length);
            byte[] bytes = pe.GetSectionData(rva).GetContent(0, length).ToArray();
            string sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            Require(sha == row.GetProperty("sha256").GetString() && name.EndsWith(sha, StringComparison.OrdinalIgnoreCase), "RVA bytes differ: " + method.Key);
            dataTokens.Add(fields[0].Token);
            propertyTokens.Add(name, method.Token);
            observed.Add(new { name = method.Name, owner = method.Owner, token = method.Token, signature = method.ReturnType,
                elementType = element, elementWidth = width, elements = length / width, length, sha256 = sha,
                rva, fieldToken = fields[0].Token, fieldType, fieldExtent = extent, ilBytes = method.IL.Length,
                ilSha256 = Convert.ToHexString(SHA256.HashData(method.IL)).ToLowerInvariant(), newarr = 0,
                calls = calls.Select(call => call.Owner + "." + call.Name).ToArray() });
        }
        foreach (var row in views)
        {
            var method = Find(row, "get_Values");
            string target = row.GetProperty("elementType").GetString()!, backing = row.GetProperty("backingType").GetString()!;
            string actualElement = SpanElement(method.ReturnType);
            int width = Width(actualElement);
            if (width == 0)
            {
                var definition = metadata.TypeDefinitions.Single(handle => types.Definition(metadata, handle) == actualElement);
                var valueField = metadata.GetTypeDefinition(definition).GetFields()
                    .Select(handle => metadata.GetFieldDefinition(handle)).Single(field => (field.Attributes & FieldAttributes.Static) == 0);
                width = Width(valueField.DecodeSignature(types, (object?)null));
            }
            Require(width == row.GetProperty("elementWidth").GetInt32() && width == Width(backing), "typed view scalar layout mismatch: " + method.Key);
            string expectedElement = row.TryGetProperty("csharpType", out var expected) ? expected.GetString()!.Replace("global::", "") : target;
            Require(actualElement.Replace('+', '.') == expectedElement, "typed view signature mismatch: " + method.Key);
            var calls = method.Instructions.Where(IsCall).Select(instruction => Resolve(metadata, types, instruction.Token)).ToArray();
            int backingToken = propertyTokens[row.GetProperty("backing").GetString()!];
            Require(calls.Length == 2 && calls.Count(call => call.Definition == backingToken) == 1
                && calls.Count(call => call.Owner == "System.Runtime.InteropServices.MemoryMarshal" && call.Name == "Cast"
                    && call.GenericArguments.SequenceEqual(new[] { backing, actualElement })) == 1, "view must use exact backing plus bit cast: " + method.Key);
            observedViews.Add(new { name = row.GetProperty("name").GetString(), owner = method.Owner, token = method.Token,
                signature = method.ReturnType, elementType = actualElement, elementWidth = width,
                backing = row.GetProperty("backing").GetString(), length = row.GetProperty("length").GetInt32(),
                sha256 = row.GetProperty("sha256").GetString(), ilBytes = method.IL.Length,
                ilSha256 = Convert.ToHexString(SHA256.HashData(method.IL)).ToLowerInvariant(), newarr = 0, conversions = 0 });
        }

        // Direct copy/view consumers and owned-array recipes are the roots. Walk
        // every local callee, retaining offsets/edges. Required destination and
        // lifetime allocations are named boundaries, never payload exemptions.
        var direct = methods.Values.SelectMany(method => method.Instructions.Where(IsCall)
            .Select(instruction => (Caller: method, Instruction: instruction, Target: Resolve(metadata, types, instruction.Token))))
            .Where(edge => getterTokens.Contains(edge.Target.Definition)).ToArray();
        Require(getterTokens.Count > 0 && getterTokens.All(token => direct.Any(edge => edge.Target.Definition == token)), "unreferenced getter/view");
        var roots = direct.Select(edge => edge.Caller.Token).Concat(methods.Values
            .Where(method => method.Name.StartsWith("__dotcc_owned_array_", StringComparison.Ordinal)
                || method.Name == ".cctor" && method.Owner == literalOwner).Select(method => method.Token)).Distinct().Order().ToArray();
        var pending = new Queue<int>(roots);
        var visited = new HashSet<int>();
        var edges = new List<object>();
        var boundaries = new List<object>();
        var pointerPool = new List<object>();
        var external = new HashSet<string>();
        var externalSites = new List<object>();
        var initializerEdges = new List<object>();
        var trackedAddressCallbacks = new List<object>();
        var trackedAcquireTargets = new List<object>();
        while (pending.TryDequeue(out int token))
        {
            if (!visited.Add(token)) continue;
            var method = methods[token];
            Require(method.IL.Length > 0 && (method.Attributes & (MethodAttributes.PinvokeImpl | MethodAttributes.Abstract)) == 0,
                "uninspectable local payload helper: " + method.Key);
            // Local helper type initialization is implicit in IL. Its array
            // cache must not escape the call graph. Shared Libc startup is a
            // separate concrete lifetime contract checked by the address gate.
            var initializerOwners = method.Instructions.Where(item => item.Code.OperandType == OperandType.InlineField
                    && (item.Token & unchecked((int)0xff000000)) == 0x04000000)
                .Select(item => types.Definition(metadata, metadata.GetFieldDefinition(
                    MetadataTokens.FieldDefinitionHandle(item.Token & 0xffffff)).GetDeclaringType()))
                .Append(method.Owner).Distinct();
            foreach (string initializerOwner in initializerOwners)
            {
                if (initializerOwner == runtimeOwner || initializerOwner == productOwner
                    || !byName.TryGetValue(initializerOwner + "..cctor", out var initializers)) continue;
                Require(initializers.Length == 1, "ambiguous local type initializer");
                if (initializers[0].Token == token) continue;
                initializerEdges.Add(new { callerToken = token, targetToken = initializers[0].Token });
                pending.Enqueue(initializers[0].Token);
            }
            foreach (var instruction in method.Instructions)
            {
                Require(instruction.Code != OpCodes.Newarr, "temporary array in transitive copy closure: " + method.Key + " IL_" + instruction.Offset.ToString("X4"));
                Require(instruction.Code != OpCodes.Box, "boxing in payload closure: " + method.Key);
                if (instruction.Code == OpCodes.Calli)
                {
                    var signature = metadata.GetStandaloneSignature(MetadataTokens.StandaloneSignatureHandle(instruction.Token & 0xffffff));
                    var callback = signature.DecodeMethodSignature(types, (object?)null);
                    var definition = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(token & 0xffffff));
                    var acquire = definition.DecodeSignature(types, (object?)null);
                    var cacheType = metadata.GetTypeDefinition(definition.GetDeclaringType());
                    var fields = cacheType.GetFields()
                        .Select(handle => metadata.GetFieldDefinition(handle)).ToArray();
                    Require(method.Owner == productOwner + "+__DotccGlobalAddressCache" && method.Name == "Get"
                        && acquire.GenericParameterCount == 1 && acquire.ReturnType == "!!0*"
                        && acquire.ParameterTypes.SequenceEqual(new[] { "fnptr" })
                        && callback.Header.CallingConvention == SignatureCallingConvention.Default && !callback.Header.IsInstance
                        && callback.ReturnType == "!!0&" && callback.ParameterTypes.Length == 0
                        && fields.Length == 1 && metadata.GetString(fields[0].Name) == "value"
                        && fields[0].DecodeSignature(types, (object?)null) == "byte&" && (fields[0].Attributes & FieldAttributes.Static) == 0
                        && cacheType.GetCustomAttributes().Any(handle => Resolve(metadata, types,
                            MetadataTokens.GetToken(metadata.GetCustomAttribute(handle).Constructor)).Owner == "System.Runtime.CompilerServices.IsByRefLikeAttribute"),
                        "unreviewed indirect payload call: " + method.Key);
                    trackedAddressCallbacks.Add(new { caller = method.Key, callerToken = token, offset = instruction.Offset,
                        signature = Convert.ToHexString(metadata.GetBlobBytes(signature.Signature)),
                        reason = "existing tracked-ref cache acquire callback; complete source/lifetime address qualification remains mandatory" });
                }
                if (!IsCall(instruction)) continue;
                var call = Resolve(metadata, types, instruction.Token);
                if (call.Owner == productOwner + "+__DotccGlobalAddressCache" && call.Name == "Get")
                {
                    int index = Array.IndexOf(method.Instructions, instruction);
                    Require(index > 0 && method.Instructions[index - 1].Code == OpCodes.Ldftn && call.Definition != 0
                        && call.GenericArguments.Length == 1, "unbound tracked-ref acquire target: " + method.Key);
                    var pointer = method.Instructions[index - 1];
                    var target = Resolve(metadata, types, pointer.Token);
                    Require(target.Definition != 0, "foreign tracked-ref acquire target");
                    var acquire = methods[target.Definition];
                    var acquireDefinition = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(target.Definition & 0xffffff));
                    Require((acquire.Attributes & MethodAttributes.Static) != 0
                        && acquire.ReturnType == call.GenericArguments[0] + "&"
                        && acquireDefinition.DecodeSignature(types, (object?)null).ParameterTypes.Length == 0,
                        "wrong tracked-ref acquire signature: " + acquire.Key);
                    trackedAcquireTargets.Add(new { callerToken = token, offset = instruction.Offset,
                        pointerOffset = pointer.Offset, cacheToken = call.Definition, targetToken = target.Definition });
                    edges.Add(new { caller = method.Key, callerToken = token, offset = pointer.Offset,
                        target = acquire.Key, targetToken = acquire.Token });
                    pending.Enqueue(acquire.Token);
                }
                if (ExistingOwnerContextBoundary(metadata, pe, types, methods, method, instruction, call, productOwner) is { } ownerBoundary)
                {
                    boundaries.Add(ownerBoundary);
                    continue;
                }
                Require(instruction.Code != OpCodes.Newobj || IsValueConstruction(metadata, types, call.Owner),
                    "array/object/delegate construction in payload closure: " + method.Key);
                if (call.Owner == runtimeOwner && call.Definition != 0 && IsDestination(call))
                {
                    boundaries.Add(new { caller = method.Key, callerToken = token, offset = instruction.Offset, target = call.Owner + "." + call.Name,
                        reason = "required independently writable C destination allocation; payload callers remain inspected" });
                    continue;
                }
                if (call.Owner == "System.GC" && call.Name is "AllocateArray" or "AllocateUninitializedArray")
                {
                    int index = Array.IndexOf(method.Instructions, instruction);
                    bool pointerStorage = method.Owner == literalOwner && method.Name == ".cctor" && call.GenericArguments.SequenceEqual(new[] { "byte" });
                    bool widePool = method.Owner == runtimeOwner && method.Name == "L16" && call.GenericArguments.SequenceEqual(new[] { "char" });
                    var lifetimeFields = method.Instructions.Where(item => item.Code.OperandType == OperandType.InlineField
                        && (item.Token & unchecked((int)0xff000000)) == 0x04000000)
                        .Select(item => metadata.GetFieldDefinition(MetadataTokens.FieldDefinitionHandle(item.Token & 0xffffff)))
                        .Select(field => types.Definition(metadata, field.GetDeclaringType()) + "." + metadata.GetString(field.Name)).Distinct().Order().ToArray();
                    if (widePool)
                    {
                        var outgoing = method.Instructions.Where(IsCall).Select(item => Resolve(metadata, types, item.Token).Name).ToHashSet();
                        Require(new[] { "GetArrayDataReference", "AsPointer", "GetOrAdd", "Add" }.All(outgoing.Contains), "literal pool lost concrete pin/root/publication edges");
                        Require(new[] { runtimeOwner + "._u16Literals", runtimeOwner + "._globalArrayRoots" }.All(lifetimeFields.Contains),
                            "literal pool lost concrete root/cache fields");
                    }
                    if (pointerStorage) Require(lifetimeFields.Contains(literalOwner + ".Storage"), "literal pool lost static storage root");
                    Require((pointerStorage || widePool) && index > 0 && method.Instructions[index - 1].Code == OpCodes.Ldc_I4_1,
                        "unreviewed GC array allocation: " + method.Key);
                    pointerPool.Add(new { caller = method.Key, callerToken = token, offset = instruction.Offset, target = call.Owner + "." + call.Name,
                        pinned = true, elementTypes = call.GenericArguments, lifetimeFields,
                        reason = "required literal pointer-pool owned final storage; not an initializer temporary" });
                    continue;
                }
                Require(!(call.Name is "ToArray" or "CreateInstance" || call.Owner == "System.Activator"), "equivalent temporary construction in payload closure: " + method.Key);
                if (call.Definition != 0)
                {
                    edges.Add(new { caller = method.Key, callerToken = token, offset = instruction.Offset,
                        target = methods[call.Definition].Key, targetToken = call.Definition });
                    pending.Enqueue(call.Definition);
                }
                else
                {
                    string target = call.Owner + "." + call.Name;
                    bool span = (call.Owner.StartsWith("System.ReadOnlySpan`1<", StringComparison.Ordinal)
                        || call.Owner.StartsWith("System.Span`1<", StringComparison.Ordinal)) && call.Name is ".ctor" or "CopyTo" or "op_Implicit" or "Slice" or "get_Length";
                    bool helper = call.Owner == "System.Runtime.CompilerServices.RuntimeHelpers" && call.Name is "CreateSpan" or "CreateReadOnlySpan"
                        || call.Owner == "System.Runtime.CompilerServices.Unsafe" && call.Name is "As" or "AsPointer" or "IsNullRef"
                        || call.Owner == "System.Runtime.InteropServices.MemoryMarshal" && call.Name is "Cast" or "GetArrayDataReference"
                        || call.Owner == "System.IntPtr" && call.Name == "op_Explicit";
                    bool literalLifetime = method.Owner == runtimeOwner && method.Name == "L16"
                        && (call.Owner.StartsWith("System.Collections.Concurrent.ConcurrentDictionary`2<", StringComparison.Ordinal) && call.Name is "TryGetValue" or "GetOrAdd"
                            || call.Owner.StartsWith("System.Collections.Generic.List`1<", StringComparison.Ordinal) && call.Name == "Add"
                            || call.Owner == "System.String" && call.Name is "CopyTo" or "get_Length"
                            || call.Owner == "System.Threading.Monitor" && call.Name is "Enter" or "Exit");
                    bool ownerBaseConstructor = IsExistingOwnerBaseConstructor(metadata, types, method, instruction, call, productOwner);
                    Require(span || helper || literalLifetime || ownerBaseConstructor, "unreviewed external payload-closure call: " + method.Key + " -> " + target);
                    external.Add(target);
                    externalSites.Add(new { caller = method.Key, callerToken = token, offset = instruction.Offset, target,
                        classification = ownerBaseConstructor ? "existing owner instance: first ldarg.0 then direct parameterless System.Object::.ctor; no allocation"
                            : literalLifetime ? "required literal pointer-pool storage/root lifetime" : "span/value/bit-copy intrinsic" });
                }
            }
        }
        var report = new { status = "passed", schemaVersion = 2, auditRevision = 5, assembly = Path.GetFullPath(args[0]),
            assemblySha256 = Digest(args[0]), mvid = metadata.GetGuid(metadata.GetModuleDefinition().Mvid).ToString(),
            manifest = Path.GetFullPath(args[1]), manifestSha256 = Digest(args[1]),
            propertyCount = properties.Length, viewCount = views.Length, newarr = 0,
            rvaBytes = properties.Sum(row => (long)row.GetProperty("length").GetInt32()),
            getterCallSites = direct.Length, properties = observed, views = observedViews,
            transitiveCopyClosure = new { roots, methods = visited.Order().Select(token => new { token, name = methods[token].Key,
                ilBytes = methods[token].IL.Length, ilSha256 = Convert.ToHexString(SHA256.HashData(methods[token].IL)).ToLowerInvariant(),
                newarr = 0 }).ToArray(), edges, initializerEdges, trackedAddressCallbacks, trackedAcquireTargets, requiredDestinationBoundaries = boundaries,
                requiredPinnedPointerPoolAllocations = pointerPool, externalCalls = external.Order().ToArray(), externalCallSites = externalSites },
            scope = "exact actual typed backing/view IL/RVA and transitive copy/owned-helper closure; warmed allocation/native semantics and interpreter acceptance remain separate" };
        File.WriteAllText(args[2], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"{properties.Length} backings, {views.Length} views, {direct.Length} call sites, {visited.Count} transitive methods, no payload arrays");
    }

    // A class owner already exists when its constructor copies initializer payloads.
    // Review precisely its base call; never admit construction of another object.
    private static bool IsExistingOwnerBaseConstructor(MetadataReader metadata, TypeNames types,
        Method method, Instruction instruction, Call call, string productOwner)
    {
        if (method.Owner != productOwner || method.Name != ".ctor"
            || (method.Attributes & MethodAttributes.Static) != 0
            || call.Owner != "System.Object" || call.Name != ".ctor" || call.Definition != 0
            || call.GenericArguments.Length != 0 || instruction.Code != OpCodes.Call
            || method.Instructions.Length < 2 || method.Instructions[0].Offset != 0
            || method.Instructions[0].Code != OpCodes.Ldarg_0
            || method.Instructions[1] != instruction || instruction.Offset != 1
            || (instruction.Token & unchecked((int)0xff000000)) != 0x0a000000) return false;
        var owner = metadata.GetTypeDefinition(metadata.GetMethodDefinition(
            MetadataTokens.MethodDefinitionHandle(method.Token & 0xffffff)).GetDeclaringType());
        if (owner.GetGenericParameters().Count != 0 || types.Name(metadata, owner.BaseType) != "System.Object") return false;
        var signature = metadata.GetMemberReference(MetadataTokens.MemberReferenceHandle(instruction.Token & 0xffffff))
            .DecodeMethodSignature(types, (object?)null);
        return signature.Header.IsInstance && signature.GenericParameterCount == 0
            && signature.ReturnType == "Void" && signature.ParameterTypes.Length == 0;
    }

    private static object? ExistingOwnerContextBoundary(MetadataReader metadata, PEReader pe, TypeNames types,
        Dictionary<int, Method> methods, Method method, Instruction site, Call call, string ownerName)
    {
        if (method.Owner != ownerName || method.Name != ".ctor" || (method.Attributes & MethodAttributes.Static) != 0) return null;
        var ownerHandle = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(method.Token & 0xffffff)).GetDeclaringType();
        string contextName = ownerName + "+__DotCcContext", bindingName = ownerName + "+Libc+RuntimeBinding";
        var state = metadata.GetTypeDefinition(ownerHandle).GetFields().Where(handle =>
            metadata.GetString(metadata.GetFieldDefinition(handle).Name) == "__DotCcState").ToArray();
        if (state.Length != 1) return null;
        var field = metadata.GetFieldDefinition(state[0]);
        int fieldToken = MetadataTokens.GetToken(state[0]);
        if ((field.Attributes & (FieldAttributes.InitOnly | FieldAttributes.Static)) != FieldAttributes.InitOnly
            || field.DecodeSignature(types, (object?)null) != contextName) return null;
        int index = Array.IndexOf(method.Instructions, site);
        string? role = null;
        if (site.Code == OpCodes.Newobj && call.Owner == contextName && call.Name == ".ctor" && call.Definition != 0
            && index > 0 && index + 1 < method.Instructions.Length
            && method.Instructions[index - 1].Code == OpCodes.Ldarg_0
            && method.Instructions[index + 1].Code == OpCodes.Stfld && method.Instructions[index + 1].Token == fieldToken)
            role = "allocate independently owned C global/TLS context and immediately publish to readonly owner field";
        else if (site.Code == OpCodes.Call && call.Owner == ownerName && call.Name == "__DotCcEnter" && call.Definition != 0
            && index > 0 && method.Instructions[index - 1].Code == OpCodes.Ldarg_0)
        {
            var enter = methods[call.Definition];
            var instructions = enter.Instructions;
            if (instructions.Length == 4 && instructions[0].Code == OpCodes.Ldarg_0
                && instructions[1].Code == OpCodes.Ldfld && instructions[1].Token == fieldToken
                && instructions[2].Code == OpCodes.Callvirt && instructions[3].Code == OpCodes.Ret)
            {
                var target = Resolve(metadata, types, instructions[2].Token);
                if (target.Owner == contextName && target.Name == "Enter" && target.Definition != 0)
                    role = "bind existing readonly owner context while copying initializer payloads";
            }
        }
        else if (site.Code == OpCodes.Callvirt && call.Owner == contextName && call.Name == "Dispose" && call.Definition != 0
            && index >= 2 && method.Instructions[index - 2].Code == OpCodes.Ldarg_0
            && method.Instructions[index - 1].Code == OpCodes.Ldfld && method.Instructions[index - 1].Token == fieldToken)
            role = "release the same readonly owner context on constructor failure";
        else if (site.Code == OpCodes.Callvirt && call.Owner == "System.IDisposable" && call.Name == "Dispose" && index >= 2
            && method.Instructions[index - 1].Code == OpCodes.Constrained
            && types.Name(metadata, MetadataTokens.EntityHandle(method.Instructions[index - 1].Token)) == bindingName
            && method.Instructions[index - 2].Code is var previous && (previous == OpCodes.Ldloca || previous == OpCodes.Ldloca_S))
        {
            int local = method.Instructions[index - 2].Token;
            var definition = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(method.Token & 0xffffff));
            var body = pe.GetMethodBody(definition.RelativeVirtualAddress);
            var locals = metadata.GetStandaloneSignature(body.LocalSignature).DecodeLocalSignature(types, (object?)null);
            if (local >= 0 && local < locals.Length && locals[local] == bindingName
                && body.ExceptionRegions.Any(region => region.Kind == ExceptionRegionKind.Finally
                    && site.Offset >= region.HandlerOffset && site.Offset < region.HandlerOffset + region.HandlerLength))
                role = "dispose exactly the local value-type owner binding in its finally handler";
        }
        if (role is null) return null;
        var contexts = metadata.TypeDefinitions.Where(handle => types.Definition(metadata, handle) == contextName).ToArray();
        if (contexts.Length != 1) return null;
        var contextFields = metadata.GetTypeDefinition(contexts[0]).GetFields().Select(handle => metadata.GetFieldDefinition(handle)).ToArray();
        if (!contextFields.Any(item => metadata.GetString(item.Name) == "Values" && item.DecodeSignature(types, (object?)null) == "byte[]")
            || !contextFields.Any(item => metadata.GetString(item.Name) == "runtime" && item.DecodeSignature(types, (object?)null) == ownerName + "+Libc+RuntimeContext"
                && (item.Attributes & FieldAttributes.InitOnly) != 0)) return null;
        var constructor = methods.Values.SingleOrDefault(item => item.Owner == contextName && item.Name == ".ctor");
        if (constructor is null || constructor.Instructions.Any(item => item.Code == OpCodes.Newarr)) return null;
        bool pinnedValues = false;
        foreach (var instruction in constructor.Instructions.Where(IsCall))
        {
            var target = Resolve(metadata, types, instruction.Token);
            int offset = Array.IndexOf(constructor.Instructions, instruction);
            if (target.Owner == "System.GC" && target.Name == "AllocateArray" && target.GenericArguments.SequenceEqual(new[] { "byte" })
                && offset > 0 && constructor.Instructions[offset - 1].Code == OpCodes.Ldc_I4_1)
                pinnedValues = true;
            if (target.Owner.Contains("Literals", StringComparison.Ordinal) || target.Owner.Contains("PrimitiveView", StringComparison.Ordinal)) return null;
        }
        if (!pinnedValues) return null;
        return new { caller = method.Key, callerToken = method.Token, offset = site.Offset, target = call.Owner + "." + call.Name,
            reason = role, readonlyOwnerField = fieldToken, contextConstructor = constructor.Token,
            contextConstructorILSha256 = Convert.ToHexString(SHA256.HashData(constructor.IL)).ToLowerInvariant(), pinnedValues = true,
            scope = "required cold owner/storage lifetime; no payload getter or temporary array in context constructor; actual owner/GC/TLS execution is separate" };
    }

    private static bool IsDestination(Call call) => (call.Owner.EndsWith("+Libc", StringComparison.Ordinal) || call.Owner.EndsWith(".Libc", StringComparison.Ordinal))
        && call.Name is "GlobalArrayZeroed" or "GlobalArrayFrom" or "GlobalAlignedZeroed" or "GlobalAlignedFrom";
    private static bool IsValueConstruction(MetadataReader metadata, TypeNames names, string name)
    {
        if (name.StartsWith("System.ReadOnlySpan`1<", StringComparison.Ordinal) || name.StartsWith("System.Span`1<", StringComparison.Ordinal)) return true;
        foreach (var handle in metadata.TypeDefinitions)
            if (names.Definition(metadata, handle) == name)
                return names.Name(metadata, metadata.GetTypeDefinition(handle).BaseType) is "System.ValueType" or "System.Enum";
        return false;
    }
    private static bool IsDelegate(MetadataReader metadata, TypeNames names, string name)
    {
        if (name.StartsWith("System.Action", StringComparison.Ordinal) || name.StartsWith("System.Func", StringComparison.Ordinal)) return true;
        foreach (var handle in metadata.TypeDefinitions)
        {
            if (names.Definition(metadata, handle) != name) continue;
            var parent = metadata.GetTypeDefinition(handle).BaseType;
            return names.Name(metadata, parent) is "System.MulticastDelegate" or "System.Delegate";
        }
        return false;
    }
    private static int Width(string type) => type switch { "byte" or "sbyte" or "bool" => 1, "ushort" or "short" or "char" => 2,
        "uint" or "int" or "float" => 4, "ulong" or "long" or "double" => 8, _ => 0 };
    private static int FieldWidth(MetadataReader metadata, TypeNames names, string type)
    {
        if (Width(type) is > 0 and var width) return width;
        foreach (var handle in metadata.TypeDefinitions)
            if (names.Definition(metadata, handle) == type) return metadata.GetTypeDefinition(handle).GetLayout().Size;
        throw new InvalidDataException("unknown RVA field extent: " + type);
    }
    private static string SpanElement(string signature)
    {
        Require(signature.StartsWith("System.ReadOnlySpan`1<", StringComparison.Ordinal) && signature.EndsWith('>'), "not a typed span signature");
        return signature[22..^1];
    }
    private static bool IsCall(Instruction instruction) => instruction.Code == OpCodes.Call || instruction.Code == OpCodes.Callvirt || instruction.Code == OpCodes.Newobj;
    private static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }

    private sealed class Method
    {
        public int Token; public string Owner, Name, Key, ReturnType; public byte[] IL; public Instruction[] Instructions; public MethodAttributes Attributes;
        public Method(MetadataReader metadata, PEReader pe, TypeNames names, MethodDefinitionHandle handle)
        {
            var method = metadata.GetMethodDefinition(handle); Token = MetadataTokens.GetToken(handle);
            Owner = names.Definition(metadata, method.GetDeclaringType()); Name = metadata.GetString(method.Name); Key = Owner + "." + Name;
            ReturnType = method.DecodeSignature(names, (object?)null).ReturnType; Attributes = method.Attributes;
            IL = method.RelativeVirtualAddress == 0 ? [] : pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!;
            Instructions = Decode(IL).ToArray();
        }
    }
    private sealed record Call(string Owner, string Name, int Definition, ImmutableArray<string> GenericArguments);
    private static Call Resolve(MetadataReader metadata, TypeNames names, int token)
    {
        var handle = MetadataTokens.EntityHandle(token);
        if (handle.Kind == HandleKind.MethodSpecification)
        {
            var specification = metadata.GetMethodSpecification((MethodSpecificationHandle)handle);
            return Resolve(metadata, names, MetadataTokens.GetToken(specification.Method)) with { GenericArguments = specification.DecodeSignature(names, (object?)null) };
        }
        if (handle.Kind == HandleKind.MethodDefinition)
        {
            var definition = metadata.GetMethodDefinition((MethodDefinitionHandle)handle);
            return new(names.Definition(metadata, definition.GetDeclaringType()), metadata.GetString(definition.Name), token, []);
        }
        Require(handle.Kind == HandleKind.MemberReference, "unknown method operand");
        var member = metadata.GetMemberReference((MemberReferenceHandle)handle);
        return new(names.Name(metadata, member.Parent), metadata.GetString(member.Name), 0, []);
    }
    private sealed record Instruction(int Offset, OpCode Code, int Token);
    private static readonly Dictionary<ushort, OpCode> Codes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
        .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!).ToDictionary(code => unchecked((ushort)code.Value));
    private static IEnumerable<Instruction> Decode(byte[] il)
    {
        for (int offset = 0; offset < il.Length;)
        {
            int start = offset; ushort value = il[offset++];
            if (value == 0xfe) value = (ushort)(0xfe00 | il[offset++]);
            var code = Codes[value];
            int size = code.OperandType switch { OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2, OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => checked(4 + 4 * BitConverter.ToInt32(il, offset)), _ => 4 };
            Require(offset + size <= il.Length, "truncated IL");
            yield return new(start, code, size == 4 ? BitConverter.ToInt32(il, offset) : code.OperandType == OperandType.ShortInlineVar ? il[offset] : code.OperandType == OperandType.InlineVar ? BitConverter.ToUInt16(il, offset) : 0); offset += size;
        }
    }

    private sealed class TypeNames : ISignatureTypeProvider<string, object?>
    {
        public string Name(MetadataReader reader, EntityHandle handle) => handle.Kind switch {
            HandleKind.TypeDefinition => Definition(reader, (TypeDefinitionHandle)handle),
            HandleKind.TypeReference => Reference(reader, (TypeReferenceHandle)handle),
            HandleKind.TypeSpecification => reader.GetTypeSpecification((TypeSpecificationHandle)handle).DecodeSignature(this, (object?)null),
            _ => "<" + handle.Kind + ">" };
        public string Definition(MetadataReader reader, TypeDefinitionHandle handle)
        {
            var type = reader.GetTypeDefinition(handle); string name = reader.GetString(type.Name); var parent = type.GetDeclaringType();
            if (!parent.IsNil) return Definition(reader, parent) + "+" + name;
            string ns = reader.GetString(type.Namespace); return ns.Length == 0 ? name : ns + "." + name;
        }
        private string Reference(MetadataReader reader, TypeReferenceHandle handle)
        {
            var type = reader.GetTypeReference(handle); string name = reader.GetString(type.Name);
            if (type.ResolutionScope.Kind == HandleKind.TypeReference) return Reference(reader, (TypeReferenceHandle)type.ResolutionScope) + "+" + name;
            string ns = reader.GetString(type.Namespace); return ns.Length == 0 ? name : ns + "." + name;
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
        public string GetPrimitiveType(PrimitiveTypeCode code) => code switch { PrimitiveTypeCode.Byte => "byte", PrimitiveTypeCode.SByte => "sbyte",
            PrimitiveTypeCode.Int16 => "short", PrimitiveTypeCode.UInt16 => "ushort", PrimitiveTypeCode.Int32 => "int", PrimitiveTypeCode.UInt32 => "uint",
            PrimitiveTypeCode.Int64 => "long", PrimitiveTypeCode.UInt64 => "ulong", PrimitiveTypeCode.Char => "char", PrimitiveTypeCode.Boolean => "bool",
            PrimitiveTypeCode.Single => "float", PrimitiveTypeCode.Double => "double", _ => code.ToString() };
        public string GetSZArrayType(string element) => element + "[]";
        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte kind) => Definition(reader, handle);
        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte kind) => Reference(reader, handle);
        public string GetTypeFromSpecification(MetadataReader reader, object? context, TypeSpecificationHandle handle, byte kind) => reader.GetTypeSpecification(handle).DecodeSignature(this, context);
    }
}
