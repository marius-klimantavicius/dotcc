using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Edg.SourceFormatter;

// Keep translated API declarations/storage in their original owner. Only
// synthetic static helpers move when the CLR's per-type method limit is near.
internal static class MethodShards
{
    private const string Begin = "// dot-gen:member-begin";
    private const string End = "// dot-gen:member-end";
    private sealed record Edit(int Start, int End, string Text);
    private sealed record Candidate(MethodDeclarationSyntax Method, int Start, int End, int Cost);

    public static int Run()
    {
        try
        {
            using var request = JsonDocument.Parse(Console.In.ReadToEnd());
            var data = request.RootElement;
            string source = data.GetProperty("source").GetString()!;
            string qualifiedOwner = data.GetProperty("owner").GetString()!;
            int threshold = data.TryGetProperty("threshold", out var t) ? t.GetInt32() : 62000;
            int target = data.TryGetProperty("target", out var v) ? v.GetInt32() : 61000;
            if (target < 1 || threshold < target || threshold > 64000)
                throw new ArgumentException("Invalid method sharding threshold/target.");
            var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var errors = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Take(5).ToArray();
            if (errors.Length != 0) throw new ArgumentException(string.Join("\n", errors.Select(e => e.ToString())));
            string canonicalOwner = Canonical(qualifiedOwner);
            var owner = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .SingleOrDefault(c => QualifiedName(c) == canonicalOwner)
                ?? throw new ArgumentException("Generated owner not found: " + qualifiedOwner);
            int before = MethodBudget(owner);
            if (before <= threshold)
            {
                Write(source, before, before, target, Array.Empty<object>());
                return 0;
            }
            var privateTypes = owner.Members.OfType<BaseTypeDeclarationSyntax>()
                .Where(c => !c.Modifiers.Any(SyntaxKind.PublicKeyword) && !c.Modifiers.Any(SyntaxKind.InternalKeyword))
                .Select(c => c.Identifier.ValueText).Concat(owner.Members.OfType<DelegateDeclarationSyntax>()
                    .Where(c => !c.Modifiers.Any(SyntaxKind.PublicKeyword) && !c.Modifiers.Any(SyntaxKind.InternalKeyword))
                    .Select(c => c.Identifier.ValueText)).ToHashSet(StringComparer.Ordinal);
            // An outer private helper can also be called by a nested public
            // type. Leave such helpers in place: moving that entry point could
            // bypass the original owner's static-constructor trigger. Ordinary
            // calls originating in owner methods already entered that owner.
            var outsideReferences = root.DescendantNodes().OfType<IdentifierNameSyntax>()
                .Where(n => EligibleName(n.Identifier.ValueText) &&
                    !ReferenceEquals(n.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault(), owner))
                .Select(n => n.Identifier.ValueText).ToHashSet(StringComparer.Ordinal);
            var candidates = new List<Candidate>();
            int after = before;
            foreach (var method in owner.Members.OfType<MethodDeclarationSyntax>())
            {
                if (after <= target) break;
                string name = method.Identifier.ValueText;
                if (!EligibleName(name) || !method.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                    !method.Modifiers.Any(SyntaxKind.StaticKeyword) || outsideReferences.Contains(name)) continue;
                if (method.ReturnType.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(n => privateTypes.Contains(n.Identifier.ValueText)) ||
                    method.ParameterList.DescendantNodes().OfType<IdentifierNameSyntax>().Any(n => privateTypes.Contains(n.Identifier.ValueText))) continue;
                int begin = source.LastIndexOf(Begin, method.SpanStart, StringComparison.Ordinal);
                int end = source.IndexOf(End, method.Span.End, StringComparison.Ordinal);
                if (begin < 0 || end < 0 || source.AsSpan(begin, method.SpanStart - begin).Contains("// dot-gen:origin", StringComparison.Ordinal)) continue;
                // Delimiters describe whole declarations. Never capture a prior
                // declaration if a malformed/foreign source lacks our protocol.
                if (source.AsSpan(begin + Begin.Length, method.SpanStart - begin - Begin.Length)
                    .Contains(End, StringComparison.Ordinal)) continue;
                end += End.Length;
                while (end < source.Length && source[end] is '\r' or '\n') ++end;
                int cost = 1 + BodySynthesisBudget(method);
                candidates.Add(new(method, begin, end, cost));
                after -= cost;
            }
            if (after > target)
                throw new ArgumentException($"Generated owner {canonicalOwner} needs {before} method slots; only {before - after} can move safely as static synthetic helpers (target {target}). Split the translation into smaller owners; instance methods and public APIs were left unchanged.");
            var shardFor = new Dictionary<string, string>(StringComparer.Ordinal);
            var shards = new List<List<Candidate>>();
            foreach (var candidate in candidates)
            {
                if (shards.Count == 0 || shards[^1].Sum(c => c.Cost) + candidate.Cost > 10000) shards.Add(new());
                string shard = "__EdgMethodShard" + (shards.Count - 1);
                if (owner.Members.OfType<BaseTypeDeclarationSyntax>().Any(c => c.Identifier.ValueText == shard))
                    throw new ArgumentException("Reserved method shard name already exists: " + shard);
                if (!shardFor.TryAdd(candidate.Method.Identifier.ValueText, shard))
                    throw new ArgumentException("Overloaded synthetic helpers cannot be sharded: " + candidate.Method.Identifier.ValueText);
                shards[^1].Add(candidate);
            }
            var references = new List<Edit>();
            foreach (var access in owner.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
                if (shardFor.TryGetValue(access.Name.Identifier.ValueText, out var shard) &&
                    (Canonical(access.Expression.ToString()) == canonicalOwner ||
                     Canonical(access.Expression.ToString()) == owner.Identifier.ValueText))
                    references.Add(new(access.Expression.Span.End, access.Expression.Span.End, "." + shard));
            foreach (var identifier in owner.DescendantNodes().OfType<IdentifierNameSyntax>())
                if (shardFor.TryGetValue(identifier.Identifier.ValueText, out var shard) &&
                    !(identifier.Parent is MemberAccessExpressionSyntax access && ReferenceEquals(access.Name, identifier)))
                    references.Add(new(identifier.SpanStart, identifier.SpanStart, shard + "."));
            var edits = new List<Edit>();
            var nested = new StringBuilder();
            var metadata = new List<object>();
            for (int i = 0; i < shards.Count; ++i)
            {
                string name = "__EdgMethodShard" + i;
                nested.Append('\n').Append(Begin).Append("\ninternal static unsafe class ").Append(name).Append(" {\n");
                foreach (var candidate in shards[i])
                {
                    var local = references.Where(e => e.Start >= candidate.Start && e.End <= candidate.End).ToList();
                    var modifier = candidate.Method.Modifiers.First(m => m.IsKind(SyntaxKind.PrivateKeyword));
                    local.Add(new(modifier.SpanStart, modifier.Span.End, "internal"));
                    string text = Apply(source, local, candidate.Start, candidate.End);
                    int firstLine = text.IndexOf('\n');
                    int lastMarker = text.LastIndexOf(End, StringComparison.Ordinal);
                    if (firstLine < 0 || lastMarker <= firstLine)
                        throw new ArgumentException("Malformed method shard delimiter.");
                    // Remove just the surrounding protocol lines, never marker
                    // spelling that may occur in a string literal or comment.
                    text = text.Substring(firstLine + 1, lastMarker - firstLine - 1);
                    nested.Append(text);
                    edits.Add(new(candidate.Start, candidate.End, ""));
                }
                nested.Append("}\n").Append(End).Append('\n');
                metadata.Add(new { name, methods = shards[i].Count, methodBudget = shards[i].Sum(c => c.Cost) });
            }
            foreach (var reference in references)
                if (!candidates.Any(c => reference.Start >= c.Start && reference.End <= c.End)) edits.Add(reference);
            // Keep enormous literal collections after ordinary method bodies.
            // Roslyn pools parser list builders: parsing a multi-million-token
            // collection first makes later small statements repeatedly clear its
            // oversized pooled array. Nested method declarations have no runtime
            // initialization effects, so placing shards first preserves ordering.
            edits.Add(new(owner.OpenBraceToken.Span.End, owner.OpenBraceToken.Span.End, nested.ToString()));
            Write(Apply(source, edits, 0, source.Length), before, after, target, metadata);
            return 0;
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or JsonException)
        {
            Console.Error.WriteLine("Method sharding failed: " + error.Message);
            return 2;
        }
    }

    private static bool EligibleName(string name) =>
        name.StartsWith("__edg_expr", StringComparison.Ordinal) || name.StartsWith("__edg_cleanup_", StringComparison.Ordinal) ||
        name.StartsWith("__edg_initialize_storage_", StringComparison.Ordinal) || name.StartsWith("__edg_assign", StringComparison.Ordinal);

    private static int BodySynthesisBudget(SyntaxNode member) => member.DescendantNodes()
        .Count(n => n is LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax);

    private static int MethodBudget(ClassDeclarationSyntax owner)
    {
        int count = 0;
        foreach (var member in owner.Members)
        {
            count += member switch
            {
                BaseMethodDeclarationSyntax => 1,
                PropertyDeclarationSyntax p => p.AccessorList?.Accessors.Count ?? 1,
                IndexerDeclarationSyntax p => p.AccessorList?.Accessors.Count ?? 1,
                EventDeclarationSyntax p => p.AccessorList?.Accessors.Count ?? 2,
                EventFieldDeclarationSyntax e => e.Declaration.Variables.Count * 2,
                _ => 0
            };
            if (member is not BaseTypeDeclarationSyntax and not DelegateDeclarationSyntax)
                count += BodySynthesisBudget(member);
        }
        // Reserve possible implicit instance/static constructors even where a
        // compiler can omit them. Local functions/lambdas above are also charged
        // conservatively, including those Roslyn places in a closure type.
        if (!owner.Modifiers.Any(SyntaxKind.StaticKeyword) && !owner.Members.OfType<ConstructorDeclarationSyntax>().Any(c => !c.Modifiers.Any(SyntaxKind.StaticKeyword))) ++count;
        if (!owner.Members.OfType<ConstructorDeclarationSyntax>().Any(c => c.Modifiers.Any(SyntaxKind.StaticKeyword))) ++count;
        return count;
    }

    private static string Apply(string source, IEnumerable<Edit> edits, int start, int end)
    {
        var result = new StringBuilder();
        int position = start;
        foreach (var edit in edits.OrderBy(e => e.Start).ThenBy(e => e.End))
        {
            if (edit.Start < position || edit.End > end) throw new ArgumentException("Overlapping method shard source edits.");
            result.Append(source, position, edit.Start - position).Append(edit.Text);
            position = edit.End;
        }
        return result.Append(source, position, end - position).ToString();
    }

    private static string QualifiedName(ClassDeclarationSyntax owner)
    {
        var parts = owner.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(n => n.Name.ToString()).ToList();
        parts.AddRange(owner.Ancestors().OfType<ClassDeclarationSyntax>().Reverse().Select(c => c.Identifier.ValueText));
        parts.Add(owner.Identifier.ValueText);
        return Canonical(string.Join('.', parts));
    }

    private static string Canonical(string value) => value.Replace("global::", "", StringComparison.Ordinal).Replace("@", "", StringComparison.Ordinal);

    private static void Write(string source, int before, int after, int target, IEnumerable<object> shards)
    {
        // System.Text.Json bounds a single escaped string token. Full compiler
        // sources exceed that bound; use the formatter's lossless chunk transport.
        var chunks = new List<string>();
        for (int start = 0; start < source.Length;)
        {
            int length = Math.Min(16 * 1024 * 1024, source.Length - start);
            if (start + length < source.Length && char.IsHighSurrogate(source[start + length - 1])) --length;
            chunks.Add(source.Substring(start, length));
            start += length;
        }
        Console.WriteLine(JsonSerializer.Serialize(new { source = chunks, metadata = new { mode = "static-synthetic-method-shards-v1", before, after, target, shards } }));
    }
}
