using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LALR.CC.LexicalGrammar;

namespace DotCC;

internal sealed class MacroOverrideSession
{
    internal readonly CPreprocessingOptions Options;
    private readonly int[] _selected;
    private readonly int[] _expansions;
    private readonly Dictionary<string, CompiledMacroOverride[]> _rules;
    internal MacroOverrideSession(CPreprocessingOptions options, IEnumerable<string>? defines)
    {
        Options = options;
        _rules = options.Rules.GroupBy(r => r.Rule.Name).ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
        _selected = new int[options.Rules.Length]; _expansions = new int[options.Rules.Length];
        foreach (var d in defines ?? Array.Empty<string>())
        {
            var name = d.Split('=')[0];
            if (_rules.ContainsKey(name)) throw new CompileException("cannot combine -D and macro override for '" + name + "'");
        }
        Event("profile", ("hash", options.ProfileHash), ("path", options.ProfilePath ?? "API/CLI"));
    }
    internal MacroDef Apply(MacroDef macro, Item origin)
    {
        if (!_rules.TryGetValue(macro.Name, out var rules)) return macro;
        string? original = null;
        string Original() => original ??= RemoveComments(SourceMappedItem.LogicalSpelling(macro.Body)).Trim();
        foreach (var rule in rules)
        {
            string? replacement;
            string reason, captures;
            try { replacement = rule.TryReplace(macro, Original, out reason, out captures); }
            catch (CompileException ex) { throw new CompileException($"{Location(origin)}: {ex.Message}", ex); }
            if (replacement is null)
            {
                Event(reason, ("name", macro.Name), ("rule", rule.Rule.Origin), ("index", rule.Index.ToString()), ("source", Location(origin)));
                continue;
            }
            Item[] tokens;
            try
            {
                tokens = rule.Lex(replacement);
                ValidateBody(tokens, macro);
            }
            catch (CompileException ex) { throw new CompileException($"{Location(origin)}: macro override '{macro.Name}' ({rule.Rule.Origin}): {ex.Message}", ex); }
            _selected[rule.Index]++;
            Event("selected", ("name", macro.Name), ("rule", rule.Rule.Origin), ("index", rule.Index.ToString()),
                ("source", Location(origin)), ("original", Options.Report is null ? "" : Original()), ("effective", replacement), ("captures", captures));
            foreach (var shadowed in rules.SkipWhile(r => r != rule).Skip(1))
                Event("shadowed", ("name", macro.Name), ("index", shadowed.Index.ToString()), ("source", Location(origin)));
            // Retain replacement spelling coordinates (macro whitespace), but origin is the original definition.
            return macro with { Body = tokens.Select(t => (Item)new SourceMappedItem(t, origin)).ToArray(), OverrideRule = rule.Index };
        }
        return macro;
    }
    private static void ValidateBody(Item[] tokens, MacroDef macro)
    {
        var text = tokens.Select(t => t.Content?.ToString()).ToArray();
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == "##" && (i == 0 || i == text.Length - 1 || text[i - 1] == "##" || text[i + 1] == "##"))
                throw new CompileException("## requires tokens on both sides");
            if (text[i] == "#" && (!macro.IsFunctionLike || i + 1 == text.Length ||
                !(macro.Params!.Contains(text[i + 1]!, StringComparer.Ordinal) || macro.IsVariadic && text[i + 1] == "__VA_ARGS__")))
                throw new CompileException("# requires a formal parameter");
            if (text[i] is "__VA_ARGS__" or "__VA_OPT__" && !macro.IsVariadic)
                throw new CompileException("variadic replacement token in a nonvariadic macro");
        }
    }
    internal void Expansion(MacroDef macro, Item? origin)
    {
        if (macro.OverrideRule is not { } i) return;
        _expansions[i]++;
        Event("expansion", ("name", macro.Name), ("index", i.ToString()), ("source", origin is null ? "<#if/#elif>" : Location(origin)));
    }
    internal void Complete(bool requireMatches = true)
    {
        foreach (var r in Options.Rules)
        {
            Event("summary", ("name", r.Rule.Name), ("index", r.Index.ToString()), ("selected", _selected[r.Index].ToString()), ("expansions", _expansions[r.Index].ToString()));
            if (requireMatches && r.Rule.RequireMatch && _selected[r.Index] == 0)
                throw CPreprocessingOptions.Error(r.Rule, "requireMatch: no active definition was selected");
        }
    }
    internal void Event(string kind, params (string, string)[] fields) => CPreprocessingOptions.WriteEvent(Options.Report, kind, fields);
    private static string Location(Item item) => (SourceMappedItem.FileOf(item)?.Name ?? "<source>") + ":" + SourceMappedItem.Physical(item);
    private static string RemoveComments(string text)
    {
        var result = new StringBuilder(); char quote = '\0';
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quote != '\0')
            {
                result.Append(c);
                if (c == '\\' && i + 1 < text.Length) result.Append(text[++i]);
                else if (c == quote) quote = '\0';
            }
            else if (c is '\'' or '"') { quote = c; result.Append(c); }
            else if (c == '/' && i + 1 < text.Length && text[i + 1] == '/') { result.Append(' '); while (i < text.Length && text[i] != '\n') i++; }
            else if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
            {
                result.Append(' '); i += 2;
                while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/')) i++;
                i++;
            }
            else result.Append(c);
        }
        return result.ToString();
    }
}

internal sealed partial class CPreprocessor
{
    private readonly MacroOverrideSession? _overrides;
    internal void RecordExpansion(MacroDef macro, Item invocation) => _overrides?.Expansion(macro, invocation);
    private void InstallMacro(MacroDef macro, Item origin)
    {
        if (macro.Name == RuntimeIntrinsicNames.IsLittleEndian) throw new CompileException("cannot redefine runtime intrinsic '" + macro.Name + "'");
        _macros[macro.Name] = _overrides?.Apply(macro, origin) ?? macro;
    }
}
