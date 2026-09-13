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
    { "(", ")", "+", "-", "*", "/", "%", "~", "!", "<<", ">>", "&", "|", "^", "<", ">", "<=", ">=", "==", "!=", "&&", "||", "?", ":", "[", "]", ",", ".", "->" };

    internal static CExpr? TryLower(IReadOnlyList<Item> body, IrBuilder context, CDialect? dialect = null)
    {
        // Fully expanded replacement lists bind in an isolated TU evaluator.
        // Reject declaration/statement injection (including aggregate bodies)
        // before parsing: discovery must never introduce types or declarations.
        if (body.Count == 0 || body.Count > 256) return null;
        try
        {
            var replacement = string.Join(" ", body.Select(t => t.Content));
            // ## can produce numeric tokens that still carry the preprocessor's
            // ID carrier. Classify the final spellings with the C lexer.
            using var bodyLexer = BytesLexer.FromString(replacement, C.BuildLexer());
            while (bodyLexer.MoveNext())
            {
                var token = bodyLexer.Current;
                var text = token.Content?.ToString() ?? "";
                if (Keywords.Contains(text) || Operators.Contains(text)) continue;
                if (CPreprocessingOptions.Identifier(text))
                {
                    if (text is "__LINE__" or "__FILE__" or "__func__" or RuntimeIntrinsicNames.IsLittleEndian) return null;
                    continue;
                }
                if (!LiteralTokens.Contains(token.ID)) return null;
            }
            using var lexer = BytesLexer.FromString("int __dotcc_macro = " + replacement + ";", C.BuildLexer());
            using var validator = new CTokenValidator(lexer);
            using var keywords = new DialectKeywordRewriter(validator, dialect ?? CDialect.Default);
            using var types = new TypeNameRewriter(keywords, context.MacroTypeNames);
            using var sizes = new SizeofFolder(types);
            using var tokens = new SyncLATokenIterator(sizes);
            var root = C.BuildSourceLocatedParser().ParseInput(tokens, debugger: null, trimReductions: true);
            if (root.IsError) return null;
            return context.LowerMacroInitializer(root);
        }
        catch (LALR.CC.ParseErrorException) { return null; }
        catch (LALR.CC.LexicalGrammar.LexerException) { return null; }
        catch (CompileException) { return null; }
        catch (OverflowException) { return null; }
        catch (DivideByZeroException) { return null; }
    }
}
