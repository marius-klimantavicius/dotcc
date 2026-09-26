using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>A selector for an original macro signature. Null parameters mean any parameter list.</summary>
public sealed record MacroSignature(bool FunctionLike, IReadOnlyList<string>? Parameters = null, bool? Variadic = null);

/// <summary>Replace the body of each selected active definition, preserving its signature and lifetime.</summary>
public sealed record MacroOverride(string Name, string Replacement, string? Exact = null, string? Pattern = null,
    MacroSignature? Signature = null, bool RequireMatch = false, IReadOnlyList<string>? Expect = null,
    bool Literal = false, string Origin = "API");

/// <summary>Immutable rules; invocation-local match state permits concurrent/repeated compiler calls.</summary>
public sealed partial class CPreprocessingOptions
{
    internal const int MaxText = 1024 * 1024;
    internal readonly CompiledMacroOverride[] Rules;
    public string? ProfilePath { get; }
    public string ProfileHash { get; }
    public TextWriter? Report { get; }
    public bool HasOverrides => Rules.Length != 0 || FieldTypeNames.Count != 0 || FunctionOverrides.Count != 0 || ExternalTypes.Count != 0;
    /// <summary>Stable names for anonymous aggregate types selected through C fields.</summary>
    public IReadOnlyList<FieldTypeNameOverride> FieldTypeNames { get; }
    /// <summary>Additional macro names or glob patterns to emit as public fields.</summary>
    public IReadOnlyList<string> EmitDefines { get; }
    public bool HasMacroExports => EmitDefines.Count != 0;
    internal MacroExportSelector ExportSelector { get; }

    public CPreprocessingOptions(IReadOnlyList<MacroOverride> macroOverrides, string? profilePath = null, TextWriter? report = null, IReadOnlyList<string>? emitDefines = null, IReadOnlyList<FieldTypeNameOverride>? fieldTypeNames = null, IReadOnlyList<FunctionOverride>? functionOverrides = null, IReadOnlyList<ExternalTypeOverride>? externalTypes = null)
    {
        FunctionOverrides = ValidateFunctionOverrides(functionOverrides, profilePath);
        ExternalTypes = ValidateExternalTypes(externalTypes);
        FieldTypeNames = ValidateFieldTypeNames(fieldTypeNames);
        EmitDefines = Array.AsReadOnly((emitDefines ?? Array.Empty<string>()).ToArray());
        ExportSelector = new MacroExportSelector(EmitDefines);
        ProfilePath = profilePath is null ? null : Path.GetFullPath(profilePath);
        Report = report;
        var lexer = LexerGrammar.C;
        Rules = macroOverrides.Select((rule, index) => new CompiledMacroOverride(rule, index, lexer)).ToArray();
        var unconditional = new HashSet<string>(StringComparer.Ordinal);
        foreach (var rule in Rules)
        {
            if (unconditional.Contains(rule.Rule.Name)) throw Error(rule.Rule, "unreachable rule after unconditional same-name rule");
            if (rule.Rule.Exact is null && rule.Rule.Pattern is null && rule.Rule.Signature is null) unconditional.Add(rule.Rule.Name);
        }
        using var bytes = new MemoryStream();
        using (var json = new Utf8JsonWriter(bytes))
        {
            json.WriteStartArray();
            foreach (var rule in Rules) rule.WriteProfile(json);
            foreach (var rule in FieldTypeNames)
            {
                json.WriteStartObject(); json.WriteString("field", rule.Field); json.WriteString("name", rule.Name);
                json.WriteBoolean("requireMatch", rule.RequireMatch); json.WriteEndObject();
            }
            WriteFunctionOverridesProfile(json);
            WriteExternalTypesProfile(json);
            json.WriteEndArray();
        }
        ProfileHash = Convert.ToHexString(SHA256.HashData(bytes.ToArray())).ToLowerInvariant();
    }

    public CPreprocessingOptions WithoutReport() => new(Rules.Select(r => r.Rule).ToArray(), ProfilePath, emitDefines: EmitDefines, fieldTypeNames: FieldTypeNames, functionOverrides: FunctionOverrides, externalTypes: ExternalTypes);

    /// <summary>Load strict version-1 JSON and optionally replace a name's profile rules with a literal CLI rule.</summary>
    public static CPreprocessingOptions Load(string? profilePath = null, IReadOnlyList<string>? overrides = null, TextWriter? report = null, IReadOnlyList<string>? emitDefines = null, IReadOnlyList<string>? typeNames = null)
    {
        var rules = new List<MacroOverride>();
        var fieldTypeNames = new List<FieldTypeNameOverride>();
        var functionOverrides = new List<FunctionOverride>();
        var externalTypes = new List<ExternalTypeOverride>();
        if (profilePath is not null)
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(profilePath));
                var root = doc.RootElement;
                Fields(root, "version", "macroOverrides", "fieldTypeNames", "functionOverrides", "externalTypes");
                functionOverrides.AddRange(ReadFunctionOverrides(root, profilePath));
                externalTypes.AddRange(ReadExternalTypes(root));
                if (root.TryGetProperty("fieldTypeNames", out var names))
                    foreach (var entry in names.EnumerateArray())
                    {
                        Fields(entry, "field", "name", "requireMatch");
                        fieldTypeNames.Add(new(RequiredString(entry, "field"), RequiredString(entry, "name"),
                            entry.TryGetProperty("requireMatch", out var required) && required.GetBoolean()));
                    }
                if (root.GetProperty("version").GetInt32() != 1) throw new CompileException("unsupported translation override profile version");
                var index = 0;
                foreach (var entry in root.TryGetProperty("macroOverrides", out var macros) ? macros.EnumerateArray() : Enumerable.Empty<JsonElement>())
                {
                    Fields(entry, "name", "replacement", "match", "signature", "requireMatch", "expect");
                    string? exact = null, pattern = null;
                    if (entry.TryGetProperty("match", out var match))
                    {
                        Fields(match, "exact", "regex");
                        exact = OptionalString(match, "exact"); pattern = OptionalString(match, "regex");
                        if ((exact is null) == (pattern is null)) throw new CompileException("match requires exactly one of exact or regex");
                    }
                    MacroSignature? signature = null;
                    if (entry.TryGetProperty("signature", out var sig))
                    {
                        Fields(sig, "kind", "parameters", "variadic");
                        var kind = RequiredString(sig, "kind");
                        if (kind is not ("object" or "function")) throw new CompileException("signature kind must be object or function");
                        signature = new(kind == "function", sig.TryGetProperty("parameters", out var ps) ? Strings(ps) : null,
                            sig.TryGetProperty("variadic", out var va) ? va.GetBoolean() : null);
                    }
                    rules.Add(new(RequiredString(entry, "name"), RequiredString(entry, "replacement"), exact, pattern, signature,
                        entry.TryGetProperty("requireMatch", out var required) && required.GetBoolean(),
                        entry.TryGetProperty("expect", out var expect) ? Strings(expect) : null,
                        Origin: $"{profilePath}:macroOverrides[{index++}]"));
                }
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or FormatException or IOException)
            { throw new CompileException($"invalid translation override profile '{profilePath}': {ex.Message}", ex); }
        }
        var cliNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var definition in overrides ?? Array.Empty<string>())
        {
            var equals = definition.IndexOf('=');
            if (equals < 1) throw new CompileException("--override-macro requires NAME=BODY");
            var name = definition[..equals];
            if (!cliNames.Add(name)) throw new CompileException("duplicate --override-macro: " + name);
            if (rules.RemoveAll(rule => rule.Name == name) != 0)
                WriteEvent(report, "cli-precedence", ("name", name), ("action", "replaced profile rules"));
            rules.Add(new(name, definition[(equals + 1)..], Literal: true, Origin: "--override-macro"));
        }
        externalTypes.AddRange((typeNames ?? Array.Empty<string>()).Select(name => new ExternalTypeOverride(name)));
        return new(rules, profilePath, report, emitDefines, fieldTypeNames, functionOverrides, externalTypes);
    }

    private static void Fields(JsonElement element, params string[] allowed)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
            if (!seen.Add(property.Name) || !allowed.Contains(property.Name, StringComparer.Ordinal))
                throw new CompileException("unknown or duplicate translation profile field: " + property.Name);
    }
    private static string RequiredString(JsonElement e, string name) => e.GetProperty(name).GetString()
        ?? throw new CompileException("null translation profile string: " + name);
    private static string? OptionalString(JsonElement e, string name) => e.TryGetProperty(name, out _) ? RequiredString(e, name) : null;
    private static string[] Strings(JsonElement e) => e.EnumerateArray().Select(v => v.GetString()
        ?? throw new CompileException("null translation profile string")).ToArray();
    internal static bool Identifier(string name) => name.Length != 0 && (char.IsAsciiLetter(name[0]) || name[0] == '_')
        && name.All(c => char.IsAsciiLetterOrDigit(c) || c == '_');
    internal static CompileException Error(MacroOverride rule, string message) => new($"macro override '{rule.Name}' ({rule.Origin}): {message}");
    internal static void WriteEvent(TextWriter? output, string kind, params (string Name, string Value)[] values)
    {
        if (output is null) return;
        using var stream = new MemoryStream();
        using (var json = new Utf8JsonWriter(stream))
        {
            json.WriteStartObject(); json.WriteString("event", kind);
            foreach (var (name, value) in values) json.WriteString(name, value);
            json.WriteEndObject();
        }
        output.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
    }
}

internal sealed class CompiledMacroOverride
{
    internal readonly MacroOverride Rule;
    internal readonly int Index;
    private readonly Regex? _pattern;
    private readonly string[]? _exact;
    private readonly string[][]? _expect;
    private readonly (string Text, string? Hole)[] _template;
    private readonly LexerGrammar _lexer;

    internal CompiledMacroOverride(MacroOverride rule, int index, LexerGrammar lexer)
    {
        Rule = rule with { Expect = rule.Expect?.ToArray(), Signature = rule.Signature is { } sig ? sig with { Parameters = sig.Parameters?.ToArray() } : null };
        Index = index; _lexer = lexer;
        if (!CPreprocessingOptions.Identifier(rule.Name) || rule.Name == RuntimeIntrinsicNames.IsLittleEndian)
            throw CPreprocessingOptions.Error(rule, "invalid or reserved macro name");
        if (rule.Exact is not null && rule.Pattern is not null) throw CPreprocessingOptions.Error(rule, "exact and regex are mutually exclusive");
        if (Rule.Signature is { } signature)
        {
            if (!signature.FunctionLike && (signature.Parameters is not null || signature.Variadic is not null))
                throw CPreprocessingOptions.Error(rule, "object signature cannot specify parameters or variadic");
            if (signature.Parameters is { } names && (names.Any(n => !CPreprocessingOptions.Identifier(n) || n == "__VA_ARGS__")
                || names.Distinct(StringComparer.Ordinal).Count() != names.Count)) throw CPreprocessingOptions.Error(rule, "invalid signature parameters");
        }
        if (rule.Exact is { } exact) _exact = Spellings(Lex(exact));
        if (Rule.Expect is { } expected) _expect = expected.Select(s => Spellings(Lex(s))).ToArray();
        if (rule.Pattern is { } pattern)
        {
            if (pattern.Length > 16384) throw CPreprocessingOptions.Error(rule, "regex exceeds 16384 characters");
            try { _pattern = new Regex("\\A(?:" + pattern + ")\\z", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)); }
            catch (ArgumentException ex) { throw CPreprocessingOptions.Error(rule, "invalid regex: " + ex.Message); }
            if (_pattern.GetGroupNames().Any(n => n.StartsWith("__dotcc_", StringComparison.Ordinal)))
                throw CPreprocessingOptions.Error(rule, "regex group prefix __dotcc_ is reserved for macro parameters");
        }
        _template = ParseTemplate(rule.Replacement, rule.Literal).ToArray();
        // Validate invariant text even if the rule is never selected.
        var validation = new StringBuilder();
        foreach (var part in _template)
        {
            if (part.Hole is null) { validation.Append(part.Text); continue; }
            if (part.Hole.StartsWith("__dotcc_", StringComparison.Ordinal)) validation.Append(part.Hole[8..]);
            else
            {
                if (_pattern is null || !_pattern.GetGroupNames().Contains(part.Hole, StringComparer.Ordinal)
                    || !CPreprocessingOptions.Identifier(part.Hole))
                    throw CPreprocessingOptions.Error(rule, "unknown named capture: " + part.Hole);
                validation.Append('0');
            }
        }
        Lex(validation.ToString());
    }

    private IEnumerable<(string Text, string? Hole)> ParseTemplate(string text, bool literal)
    {
        if (text.Length > CPreprocessingOptions.MaxText || text.Contains('\n') || text.Contains('\r'))
            throw CPreprocessingOptions.Error(Rule, "replacement exceeds size limit or contains a newline");
        if (literal) { yield return (text, null); yield break; }
        var segment = new StringBuilder();
        char quote = '\0';
        bool blockComment = false, lineComment = false;
        for (var i = 0; i < text.Length; ++i)
        {
            var c = text[i];
            if (c == '$')
            {
                if (i + 1 < text.Length && text[i + 1] == '$') { segment.Append('$'); ++i; continue; }
                var end = text.IndexOf('}', i + 2);
                if (i + 1 >= text.Length || text[i + 1] != '{' || end < 0) throw CPreprocessingOptions.Error(Rule, "invalid template escape; use ${name} or $$");
                var hole = text[(i + 2)..end];
                if (!CPreprocessingOptions.Identifier(hole)) throw CPreprocessingOptions.Error(Rule, "invalid template placeholder: " + hole);
                if (hole.StartsWith("__dotcc_", StringComparison.Ordinal))
                {
                    static bool Word(char v) => char.IsAsciiLetterOrDigit(v) || v == '_';
                    if (quote != '\0' || blockComment || lineComment || (i > 0 && Word(text[i - 1])) || (end + 1 < text.Length && Word(text[end + 1])))
                        throw CPreprocessingOptions.Error(Rule, "parameter placeholder must occupy a complete token outside a literal or comment");
                }
                yield return (segment.ToString(), null); segment.Clear();
                yield return ("", hole); i = end; continue;
            }
            if (quote == '\0' && !lineComment)
            {
                if (blockComment && c == '*' && i + 1 < text.Length && text[i + 1] == '/')
                { segment.Append("*/"); ++i; blockComment = false; continue; }
                if (!blockComment && c == '/' && i + 1 < text.Length)
                {
                    if (text[i + 1] == '*') { blockComment = true; segment.Append("/*"); ++i; continue; }
                    if (text[i + 1] == '/') { lineComment = true; segment.Append("//"); ++i; continue; }
                }
            }
            segment.Append(c);
            if (blockComment || lineComment) continue;
            if (quote != '\0' && c == '\\' && i + 1 < text.Length) segment.Append(text[++i]);
            else if (c == quote) quote = '\0';
            else if (quote == '\0' && c is '\'' or '"') quote = c;
        }
        yield return (segment.ToString(), null);
    }

    internal Item[] Lex(string text)
    {
        if (text.Length > CPreprocessingOptions.MaxText) throw CPreprocessingOptions.Error(Rule, "body exceeds 1 MiB character limit");
        try
        {
            using var lexer = _lexer.FromString(text);
            var items = new List<Item>();
            while (lexer.MoveNext())
            {
                var token = lexer.Current;
                // A # at the start of a line is lexed as a directive; use padded text for macro operators below.
                if (token.Content?.ToString() is { } t && t.StartsWith('#') && t is not ("#" or "##"))
                    throw CPreprocessingOptions.Error(Rule, "replacement cannot contain a directive");
                items.Add(token);
            }
            return items.ToArray();
        }
        catch (LexerException ex) { throw CPreprocessingOptions.Error(Rule, "invalid replacement tokens: " + ex.Message); }
    }
    private static string[] Spellings(IEnumerable<Item> tokens) => tokens.Select(t => t.Content?.ToString() ?? "").ToArray();

    internal string? TryReplace(MacroDef macro, Func<string> originalText, out string reason, out string captures)
    {
        captures = "";
        if (Rule.Signature is { } sig && (sig.FunctionLike != macro.IsFunctionLike
            || (sig.Parameters is { } ps && !ps.SequenceEqual(macro.Params ?? Array.Empty<string>(), StringComparer.Ordinal))
            || (sig.Variadic is { } variadic && variadic != macro.IsVariadic)))
        { reason = "signature-nonmatch"; return null; }
        if (_exact is not null && !_exact.SequenceEqual(Spellings(macro.Body), StringComparer.Ordinal))
        { reason = "body-nonmatch"; return null; }
        Match? match = null;
        if (_pattern is not null)
        {
            var body = originalText();
            if (body.Length > CPreprocessingOptions.MaxText) throw CPreprocessingOptions.Error(Rule, "body exceeds 1 MiB character limit");
            try { match = _pattern.Match(body); }
            catch (RegexMatchTimeoutException) { throw CPreprocessingOptions.Error(Rule, "regex match timed out (100 ms)"); }
            if (!match.Success) { reason = "body-nonmatch"; return null; }
            captures = string.Join(", ", _pattern.GetGroupNames().Where(CPreprocessingOptions.Identifier)
                .Select(n => n + "=" + match.Groups[n].Value));
        }
        if (_expect is not null && !_expect.Any(expected => expected.SequenceEqual(Spellings(macro.Body), StringComparer.Ordinal)))
            throw CPreprocessingOptions.Error(Rule, "selected definition does not satisfy expect");
        var replacement = new StringBuilder();
        foreach (var (text, hole) in _template)
        {
            if (hole is null) replacement.Append(text);
            else if (hole.StartsWith("__dotcc_", StringComparison.Ordinal))
            {
                var parameter = hole[8..];
                if (macro.Params is null || !macro.Params.Contains(parameter, StringComparer.Ordinal))
                    throw CPreprocessingOptions.Error(Rule, "missing formal parameter: " + parameter);
                replacement.Append(parameter);
            }
            else
            {
                var group = match!.Groups[hole];
                if (group.Captures.Count != 1) throw CPreprocessingOptions.Error(Rule, "capture must participate exactly once: " + hole);
                replacement.Append(group.Value);
            }
            if (replacement.Length > CPreprocessingOptions.MaxText) throw CPreprocessingOptions.Error(Rule, "replacement exceeds 1 MiB character limit");
        }
        reason = "selected";
        return replacement.ToString();
    }

    internal void WriteProfile(Utf8JsonWriter json)
    {
        json.WriteStartObject(); json.WriteString("name", Rule.Name); json.WriteString("replacement", Rule.Replacement);
        json.WriteString("exact", Rule.Exact); json.WriteString("regex", Rule.Pattern); json.WriteBoolean("literal", Rule.Literal);
        json.WriteBoolean("requireMatch", Rule.RequireMatch);
        if (Rule.Expect is { } expect) { json.WriteStartArray("expect"); foreach (var s in expect) json.WriteStringValue(s); json.WriteEndArray(); }
        if (Rule.Signature is { } sig)
        {
            json.WriteStartObject("signature"); json.WriteBoolean("function", sig.FunctionLike);
            if (sig.Variadic is { } va) json.WriteBoolean("variadic", va);
            if (sig.Parameters is { } ps) { json.WriteStartArray("parameters"); foreach (var p in ps) json.WriteStringValue(p); json.WriteEndArray(); }
            json.WriteEndObject();
        }
        json.WriteEndObject();
    }
}

internal static class RuntimeIntrinsicNames
{
    internal const string IsLittleEndian = "__dotcc_is_little_endian";
}
