using System;
using System.Collections.Generic;
using System.Linq;
using LALR.CC.LexicalGrammar;
using DotCC.Ir;

namespace DotCC.Frontends;

/// <summary>Optional public macro metadata. Parse with the ordinary C grammar
/// and fold typed IR; never interpret a C expression as a C# expression.</summary>
internal static class CConstantMacros
{
    private static readonly HashSet<int> LiteralTokens = C.Definition.SymbolNames
        .Where(s => s.Name is "NUM" or "FLOAT" or "CHAR" or "STRING").Select(s => s.ID).ToHashSet();
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    { "int", "char", "short", "long", "unsigned", "signed", "float", "double", "const", "sizeof" };
    private static readonly HashSet<string> Operators = new(StringComparer.Ordinal)
    { "(", ")", "+", "-", "*", "/", "%", "~", "!", "<<", ">>", "&", "|", "^", "<", ">", "<=", ">=", "==", "!=", "&&", "||", "?", ":" };

    internal static CExpr? TryLower(IReadOnlyList<Item> body)
    {
        // An API constant cannot contain identifiers, calls, assignments,
        // statement fragments, contextual macros or pointer casts. Restrict
        // token kinds BEFORE parsing a synthetic declaration, preventing a
        // replacement list from injecting further declarations/directives.
        if (body.Count == 0 || body.Count > 256) return null;
        foreach (var token in body)
        {
            var text = token.Content?.ToString() ?? "";
            if (Keywords.Contains(text) || Operators.Contains(text)) continue;
            if (!LiteralTokens.Contains(token.ID)) return null;
        }
        try
        {
            using var lexer = BytesLexer.FromString("int __dotcc_macro = " + string.Join(" ", body.Select(t => t.Content)) + ";", C.BuildLexer());
            using var validator = new CTokenValidator(lexer);
            using var types = new TypeNameRewriter(validator);
            using var sizes = new SizeofFolder(types);
            using var tokens = new SyncLATokenIterator(sizes);
            var root = C.BuildSourceLocatedParser().ParseInput(tokens, debugger: null, trimReductions: true);
            if (root.IsError) return null;
            var ir = new IrBuilder(null, new Backends.CSharpNameLegalizer());
            ir.AddUnit(root, "<macro>");
            if (ir.Diagnostics.Any(d => d.Severity == Severity.Error)) return null;
            return ir.Globals.SingleOrDefault()?.Init is { } expression ? ir.FoldMacroConstant(expression) : null;
        }
        catch (LALR.CC.ParseErrorException) { return null; }
        catch (LALR.CC.LexicalGrammar.LexerException) { return null; }
        catch (IrUnsupportedException) { return null; }
        catch (OverflowException) { return null; }
        catch (DivideByZeroException) { return null; }
    }
}
