using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args.Length != 3) throw new ArgumentException("inventory actual-release-PE output-report");
using var inventory = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var manifest = inventory.RootElement;
string Digest(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
if (Digest(manifest.GetProperty("translationReceipt").GetString()!) != manifest.GetProperty("receiptSha256").GetString())
    throw new InvalidDataException("translation receipt changed");
var files = manifest.GetProperty("files").EnumerateArray().ToDictionary(
    row => row.GetProperty("path").GetString()!, row => row.GetProperty("sha256").GetString()!);
var trees = new List<SyntaxTree>();
foreach (var pair in files)
{
    if (Digest(pair.Key) != pair.Value) throw new InvalidDataException("source changed: " + pair.Key);
    trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(pair.Key), new CSharpParseOptions(LanguageVersion.Preview), pair.Key));
}
trees.Add(CSharpSyntaxTree.ParseText("""
    global using global::System;
    global using global::System.Collections.Generic;
    global using global::System.IO;
    global using global::System.Linq;
    global using global::System.Net.Http;
    global using global::System.Threading;
    global using global::System.Threading.Tasks;
    """, new CSharpParseOptions(LanguageVersion.Preview), path: "<SDK implicit usings>"));
var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path));
var compilation = CSharpCompilation.Create("AddressReview", trees, references,
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.ToString()).ToArray();
var sites = new List<Site>();
var pins = new List<object>();
var fixedScopes = new List<object>();
var unknown = new List<object>();
var remaining = new List<object>();
var caches = new List<object>();
var mechanisms = new List<object>();
foreach (var tree in trees.Where(tree => files.ContainsKey(tree.FilePath)))
{
    var model = compilation.GetSemanticModel(tree);
    var root = tree.GetRoot();
    object SymbolFacts(ISymbol? symbol, ExpressionSyntax expression) => new {
        symbol = symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        symbol_kind = symbol?.Kind.ToString(),
        containing_type = symbol?.ContainingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        type = model.GetTypeInfo(expression).Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        ref_kind = symbol switch { ILocalSymbol local => local.RefKind.ToString(), IParameterSymbol parameter => parameter.RefKind.ToString(),
            IFieldSymbol field => field.RefKind.ToString(), IPropertySymbol property => property.RefKind.ToString(), IMethodSymbol method => method.RefKind.ToString(), _ => null },
        is_static = symbol?.IsStatic, is_volatile = symbol is IFieldSymbol { IsVolatile: true },
        is_readonly = symbol is IFieldSymbol { IsReadOnly: true },
        thread_static = symbol?.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.ThreadStaticAttribute") ?? false,
        returns_by_ref = symbol is IMethodSymbol { ReturnsByRef: true } or IPropertySymbol { ReturnsByRef: true },
        owner_expression = expression is MemberAccessExpressionSyntax member ? member.Expression.ToString() : null,
        declarations = symbol?.DeclaringSyntaxReferences.Select(reference => new {
            path = reference.SyntaxTree.FilePath, span_start = reference.Span.Start, span_length = reference.Span.Length,
            expression = reference.GetSyntax().ToString(),
        }).ToArray(),
    };
    string? GlobalStorage(ExpressionSyntax expression)
    {
        var symbol = model.GetSymbolInfo(expression).Symbol;
        ITypeSymbol? type = symbol switch { IFieldSymbol field => field.Type, IPropertySymbol property => property.Type, _ => null };
        if (type is not { IsUnmanagedType: true }) return null;
        if (expression is not MemberAccessExpressionSyntax member) return null;
        if (symbol is IFieldSymbol cField && cField.ContainingType.Name.EndsWith("Globals", StringComparison.Ordinal)
            && model.GetSymbolInfo(member.Expression).Symbol is IFieldSymbol { IsStatic: true, Type.IsUnmanagedType: true })
            return symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + symbol.Name;
        if (symbol is IPropertySymbol { IsStatic: true, ReturnsByRef: true } propertySymbol
            && propertySymbol.ContainingType.Name.EndsWith("GlobalsSpecial", StringComparison.Ordinal))
            return symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + symbol.Name;
        return null;
    }
    (string Region, bool Loop) Region(SyntaxNode node, bool cacheGetter)
    {
        bool loop = false;
        foreach (var ancestor in node.Ancestors())
        {
            if (ancestor is WhileStatementSyntax or DoStatementSyntax or ForStatementSyntax or ForEachStatementSyntax) loop = true;
            if (ancestor is AnonymousFunctionExpressionSyntax)
                return ($"delegate:{ancestor.SpanStart}", loop);
            if (ancestor is LocalFunctionStatementSyntax local)
            {
                if (cacheGetter && local.Identifier.ValueText.StartsWith("__dotcc_", StringComparison.Ordinal)
                    && local.Identifier.ValueText.EndsWith("_Get", StringComparison.Ordinal)) continue;
                return ($"local:{local.Identifier.ValueText}:{local.SpanStart}", loop);
            }
            if (ancestor is BaseMethodDeclarationSyntax method)
                return ($"method:{(method is MethodDeclarationSyntax named ? named.Identifier.ValueText : ".cctor")}:{method.SpanStart}", loop);
        }
        return ($"initializer:{node.SpanStart}", loop);
    }
    foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
    {
        if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { Name: "AsPointer", ContainingType.Name: "Unsafe" }) continue;
        if (invocation.ArgumentList.Arguments is not [var argument]) continue;
        if (GlobalStorage(argument.Expression) is not { } storage)
        {
            var symbol = model.GetSymbolInfo(argument.Expression).Symbol;
            var otherRegion = Region(invocation, false);
            remaining.Add(new { path = tree.FilePath,
                line = tree.GetLineSpan(invocation.Span).StartLinePosition.Line + 1,
                region = otherRegion.Region, repeats_in_loop = otherRegion.Loop,
                span_start = invocation.SpanStart, span_length = invocation.Span.Length,
                expression = invocation.ToString(), argument_expression = argument.Expression.ToString(),
                symbol = symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                symbol_kind = symbol?.Kind.ToString(), type = model.GetTypeInfo(argument.Expression).Type?.ToDisplayString(),
                bound_symbol = SymbolFacts(symbol, argument.Expression),
                bound_references = argument.Expression.DescendantNodesAndSelf().OfType<ExpressionSyntax>()
                    .Where(part => model.GetSymbolInfo(part).Symbol is ILocalSymbol or IParameterSymbol or IFieldSymbol or IPropertySymbol)
                    .Select(part => new { span_start = part.SpanStart, span_length = part.Span.Length, expression = part.ToString(),
                        facts = SymbolFacts(model.GetSymbolInfo(part).Symbol, part) }).ToArray(),
                classification = "requires-concrete-review: not a bound invariant C static storage root" });
            continue;
        }
        var location = tree.GetLineSpan(invocation.Span).StartLinePosition;
        var region = Region(invocation, false);
        sites.Add(new(tree.FilePath, storage, region.Region, location.Line + 1, "direct", region.Loop,
            invocation.SpanStart, invocation.Span.Length, invocation.ToString()));
    }
    foreach (var getter in root.DescendantNodes().OfType<LocalFunctionStatementSyntax>())
    {
        if (!getter.Identifier.ValueText.StartsWith("__dotcc_", StringComparison.Ordinal)
            || !getter.Identifier.ValueText.EndsWith("_Get", StringComparison.Ordinal)) continue;
        if (getter.ExpressionBody?.Expression is not RefExpressionSyntax reference) continue;
        if (GlobalStorage(reference.Expression) is not { } storage)
        {
            unknown.Add(new { path = tree.FilePath, line = tree.GetLineSpan(getter.Span).StartLinePosition.Line + 1,
                reason = "generated address getter does not bind to actual unmanaged static C storage" });
            continue;
        }
        if (!getter.Modifiers.Any(SyntaxKind.StaticKeyword))
            unknown.Add(new { path = tree.FilePath, getter = getter.Identifier.ValueText, reason = "address getter captures state" });
        var getterSymbol = model.GetDeclaredSymbol(getter)!;
        var localName = getter.Identifier.ValueText[..^4];
        var regionNode = getter.Parent!;
        var local = regionNode.DescendantNodes().OfType<VariableDeclaratorSyntax>()
            .SingleOrDefault(variable => variable.Identifier.ValueText == localName);
        var localSymbol = local is null ? null : model.GetDeclaredSymbol(local) as ILocalSymbol;
        if (localSymbol?.Type.Name != "__DotccGlobalAddressCache" || local?.Initializer?.Value is not LiteralExpressionSyntax value
            || !value.IsKind(SyntaxKind.DefaultLiteralExpression))
            unknown.Add(new { path = tree.FilePath, getter = getter.Identifier.ValueText, reason = "cache is not a default initialized tracked-ref local" });
        var uses = new List<object>();
        foreach (var call in regionNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (call.ArgumentList.Arguments is not [var acquire]
                || acquire.Expression is not PrefixUnaryExpressionSyntax address
                || !address.IsKind(SyntaxKind.AddressOfExpression)
                || !SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(address.Operand).Symbol, getterSymbol)) continue;
            var method = model.GetSymbolInfo(call).Symbol as IMethodSymbol;
            var receiver = call.Expression is MemberAccessExpressionSyntax access ? model.GetSymbolInfo(access.Expression).Symbol : null;
            if (method?.Name != "Get" || method.ContainingType.Name != "__DotccGlobalAddressCache"
                || !SymbolEqualityComparer.Default.Equals(receiver, localSymbol))
                unknown.Add(new { path = tree.FilePath, getter = getter.Identifier.ValueText, reason = "getter escapes its bound local cache" });
            var useRegion = Region(call, false);
            uses.Add(new { line = tree.GetLineSpan(call.Span).StartLinePosition.Line + 1,
                span_start = call.SpanStart, span_length = call.Span.Length,
                region = useRegion.Region, repeats_in_loop = useRegion.Loop, expression = call.ToString() });
        }
        if (uses.Count == 0)
            unknown.Add(new { path = tree.FilePath, getter = getter.Identifier.ValueText, reason = "unused cache getter" });
        caches.Add(new { path = tree.FilePath, storage, getter = getter.Identifier.ValueText,
            declaration_line = local is null ? 0 : tree.GetLineSpan(local.Span).StartLinePosition.Line + 1,
            declaration_span_start = local?.SpanStart, declaration_span_length = local?.Span.Length, declaration_expression = local?.ToString(),
            getter_line = tree.GetLineSpan(getter.Span).StartLinePosition.Line + 1,
            span_start = getter.SpanStart, span_length = getter.Span.Length, expression = getter.ToString(),
            storage_type = getterSymbol.ReturnType.ToDisplayString(), uses,
            lifetime = "default local + lazy static ref-return getter; no acquisition on an untaken use; Release semantic compilation enforces definite assignment and ref-struct escape rules" });
        var location = tree.GetLineSpan(getter.Span).StartLinePosition;
        var region = Region(reference, true);
        sites.Add(new(tree.FilePath, storage, region.Region, location.Line + 1, "tracked-lazy-cache", false,
            getter.SpanStart, getter.Span.Length, getter.ToString()));
    }
    foreach (var pin in root.DescendantNodes().OfType<FixedStatementSyntax>())
    {
        bool initializer = pin.Declaration.Variables.Any(variable => variable.Identifier.ValueText == "__dotcc_staticBase"
            || variable.Identifier.ValueText.EndsWith("_destination", StringComparison.Ordinal));
        fixedScopes.Add(new { path = tree.FilePath,
            span_start = pin.Declaration.SpanStart, span_length = pin.Declaration.Span.Length,
            expression = pin.Declaration.ToString(), pin_scope_start = pin.SpanStart, pin_scope_length = pin.Span.Length,
            initializer, variables = pin.Declaration.Variables.Select(variable => new {
                name = variable.Identifier.ValueText,
                symbol_type = (model.GetDeclaredSymbol(variable) as ILocalSymbol)?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                initializer = variable.Initializer?.Value.ToString(),
            }).ToArray() });
        if (initializer)
        {
            if (pin.Declaration.Variables.Count != 1)
                unknown.Add(new { path = tree.FilePath, reason = "initializer pin has multiple destinations" });
            pins.Add(new { path = tree.FilePath, line = tree.GetLineSpan(pin.Span).StartLinePosition.Line + 1,
                span_start = pin.Declaration.SpanStart, span_length = pin.Declaration.Span.Length, expression = pin.Declaration.ToString(),
                pin_scope_start = pin.SpanStart, pin_scope_length = pin.Span.Length,
                variable_name = pin.Declaration.Variables.First().Identifier.ValueText,
                scope = "explicit initializer pin, bounded by its generated fixed block" });
        }
    }
    foreach (var cache in root.DescendantNodes().OfType<StructDeclarationSyntax>().Where(type => type.Identifier.ValueText == "__DotccGlobalAddressCache"))
    {
        var fields = cache.Members.OfType<FieldDeclarationSyntax>().ToArray();
        var method = cache.Members.OfType<MethodDeclarationSyntax>().SingleOrDefault(member => member.Identifier.ValueText == "Get");
        var expected = CSharpSyntaxTree.ParseText("""
            unsafe class Expected {
                T* Get<T>(delegate* managed<ref T> acquire) where T : unmanaged {
                    if (global::System.Runtime.CompilerServices.Unsafe.IsNullRef(ref value))
                        value = ref global::System.Runtime.CompilerServices.Unsafe.As<T, byte>(ref acquire());
                    return (T*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref value);
                }
            }
            """).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        bool valid = cache.Modifiers.Any(SyntaxKind.RefKeyword) && fields.Length == 1
            && fields[0].Declaration.Type is RefTypeSyntax { Type: PredefinedTypeSyntax byteType }
            && byteType.Keyword.IsKind(SyntaxKind.ByteKeyword)
            && fields[0].Declaration.Variables is [var refField] && refField.Identifier.ValueText == "value"
            && method?.Body is { } body && SyntaxFactory.AreEquivalent(body, expected.Body)
            && model.GetDeclaredSymbol(method) is { TypeParameters: [{ HasUnmanagedTypeConstraint: true }], ReturnType: IPointerTypeSymbol };
        if (!valid) unknown.Add(new { path = tree.FilePath, reason = "tracked cache implementation differs from inspected lazy-ref mechanism" });
        mechanisms.Add(new { path = tree.FilePath, line = tree.GetLineSpan(cache.Span).StartLinePosition.Line + 1,
            span_start = cache.SpanStart, span_length = cache.Span.Length, expression = cache.ToString(),
            valid, source_sha256 = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(cache.ToString()))).ToLowerInvariant(),
            fields = fields.Select(field => field.ToString()).ToArray(),
            invariant = "one tracked ref byte field; acquire() only under IsNullRef; reconstruct pointer at each reached Get; no stored raw pointer or cached mutable C value" });
    }
}
var groups = sites.GroupBy(site => (site.Path, site.Region, site.Storage)).Select(group => new
{
    path = group.Key.Path, region = group.Key.Region, storage = group.Key.Storage,
    caches = group.Count(site => site.Kind == "tracked-lazy-cache"),
    direct = group.Count(site => site.Kind == "direct"),
    repeated_loop_direct = group.Count(site => site.Kind == "direct" && site.Loop),
    sites = group.ToArray(),
}).ToArray();
var unresolved = groups.Where(group => group.direct + group.caches > 1 || group.repeated_loop_direct > 0).ToArray();
using var peStream = File.OpenRead(args[1]);
using var pe = new PEReader(peStream);
var metadata = pe.GetMetadataReader();
var cacheMetadata = metadata.TypeDefinitions.Select(handle => metadata.GetTypeDefinition(handle))
    .Where(type => metadata.GetString(type.Name) == "__DotccGlobalAddressCache")
    .Select(type => new
    {
        fields = type.GetFields().Select(handle => metadata.GetFieldDefinition(handle)).Select(field => new
        {
            name = metadata.GetString(field.Name),
            signature = Convert.ToHexString(metadata.GetBlobBytes(field.Signature)),
        }).ToArray(),
        methods = type.GetMethods().Select(handle => metadata.GetMethodDefinition(handle)).Where(method => metadata.GetString(method.Name) == "Get")
            .Select(method => new { name = metadata.GetString(method.Name), signature = Convert.ToHexString(metadata.GetBlobBytes(method.Signature)),
                il_sha256 = Convert.ToHexString(SHA256.HashData(pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!)).ToLowerInvariant(),
                il_bytes = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!.Length }).ToArray(),
    }).ToArray();
if (caches.Count > 0 && (mechanisms.Count != 1 || cacheMetadata.Length != 1
    || cacheMetadata[0].fields is not [{ name: "value", signature: "061005" }]
    || cacheMetadata[0].methods.Length != 1))
    unknown.Add(new { reason = "actual PE does not expose the reviewed single GC-tracked ref byte cache field and Get method" });
var report = new
{
    status = errors.Length == 0 && unknown.Count == 0 && unresolved.Length == 0 ? "passed" : "failed-required-gate",
    receipt = manifest.GetProperty("translationReceipt").GetString(), receipt_sha256 = manifest.GetProperty("receiptSha256").GetString(),
    inventory = Path.GetFullPath(args[0]), inventory_sha256 = Digest(args[0]),
    analyzer = new {
        tool = typeof(Site).Assembly.Location, tool_sha256 = Digest(typeof(Site).Assembly.Location),
        mvid = typeof(Site).Assembly.ManifestModule.ModuleVersionId,
        cwd = Environment.CurrentDirectory,
        invocation = Environment.GetCommandLineArgs(),
        embedded_sources = new[] { "GeneratedAddressAudit.Program.cs", "GeneratedAddressAudit.Project" }.ToDictionary(name => name, name => {
            using var source = typeof(Site).Assembly.GetManifestResourceStream(name)!;
            return Convert.ToHexString(SHA256.HashData(source)).ToLowerInvariant();
        }),
    },
    assembly = Path.GetFullPath(args[1]), assembly_sha256 = Digest(args[1]),
    mvid = metadata.GetGuid(metadata.GetModuleDefinition().Mvid),
    source_files = files, semantic_errors = errors, unknown_cache_bindings = unknown,
    cache_mechanisms = mechanisms, cache_bindings_and_uses = caches,
    actual_pe_cache_metadata = cacheMetadata,
    remaining_address_conversions = remaining,
    unmanaged_static_storage_regions = groups, scoped_initializer_pins = pins, fixed_scope_inventory = fixedScopes,
    unresolved_pure_sites = unresolved,
    scope = "Full source semantic binding and repeated-static-base review per method/delegate region; cache getter acquisition once, tracked byref storage, explicit initializer pin scopes. Reduced native/emitted evaluation/GC/context tests and actual execution remain separate.",
};
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[2]))!);
File.WriteAllText(args[2], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"{report.status}: {sites.Count} bound sites, {groups.Length} storage regions, {unresolved.Length} unresolved pure groups, {errors.Length} semantic errors");
return report.status == "passed" ? 0 : 1;

internal sealed record Site(string Path, string Storage, string Region, int Line, string Kind, bool Loop,
    int span_start, int span_length, string expression);
