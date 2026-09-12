#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

/// <summary>
/// The C front-end: the lex → preprocess → (macro / dialect-keyword / typedef /
/// sizeof) rewriters → LALR parse → typed-IR bind pipeline, behind the
/// <see cref="IFrontend"/> seam. Owns no output-language knowledge; it produces the
/// neutral <see cref="Ir.IrBuilder"/> a backend then projects.
/// </summary>
internal sealed class CFrontend : IFrontend
{
    /// <summary>
    /// The target-neutral front half of the pipeline: lex, preprocess, parse and
    /// bind every translation unit to the typed IR, flushing the source-level
    /// diagnostics (IR errors/warnings and <c>-pedantic</c> dialect gating) along
    /// the way. The result is the backend-agnostic <see cref="Ir.IrBuilder"/> a
    /// backend then projects onto its own surface — <see cref="EmitCSharp"/> (C#)
    /// or the wat target. Splitting the pipeline here is what makes "one front-end,
    /// many targets" real rather than aspirational: every backend consumes the same
    /// IR, none re-runs the parse.
    /// </summary>
    public Ir.IrBuilder BuildIr(FrontendRequest req)
    {
        var inputPaths = req.InputPaths;
        var includeDirs = req.IncludeDirs;
        var defines = req.Defines;
        var dialect = req.Dialect;
        var names = req.Names;
        var warnings = req.Warnings;
        // -Wpedantic enables the dialect-conformance gate; -pedantic-errors
        // (Pedantic | AsErrors) escalates its diagnostics to collected errors.
        var pedantic = (warnings & WarningFlags.Pedantic) != 0;
        var pedanticErrors = (warnings & WarningFlags.PedanticErrors) == WarningFlags.PedanticErrors;

        var includeMap = Compiler.BuildIncludeMap(inputPaths, includeDirs);
        var lexerTable = C.BuildLexer();
        var activeDialect = dialect ?? CDialect.Default;
        var seededDefines = Compiler.SeedDialectDefines(activeDialect, defines);
        // C23 #embed: one byte side-table shared by every TU's preprocessor (which
        // populates it in OnEmbed) and the single IrBuilder (which resolves the
        // carrier tokens back in BuildEmbed). Keyed by content hash → cross-TU dedup.
        var embeds = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var overrides = req.Preprocessing is { } options ? new MacroOverrideSession(options, defines) : null;
        var macroBodies = new List<(string Name, IReadOnlyList<Item> Body)>();

        // Build the lexer → preprocessor → rewriter → parser pipeline for one
        // translation unit and parse it with the given (visitor-bound) parser.
        // Each unit is parsed once; one override session spans the invocation.
        Item ParseUnit(string unitPath, global::LALR.CC.Parser parser, bool quiet, DialectGate? gate = null)
        {
            var sourceMap = new PhysicalSourceMap(File.ReadAllText(unitPath), filename: Path.GetFileName(unitPath), identity: Path.GetFullPath(unitPath));
            var source = sourceMap.Text;
            // #embed search path: the TU's own directory first, then the -I dirs
            // (first-wins, mirroring #include). Resolved on the filesystem since
            // OnEmbed reads RAW bytes (distinct from the include text map).
            var embedDirs = new List<string>();
            if (Path.GetDirectoryName(unitPath) is { Length: > 0 } unitDir) { embedDirs.Add(unitDir); }
            if (includeDirs is not null) { embedDirs.AddRange(includeDirs); }
            var pre = new CPreprocessor(lexerTable, includeMap, seededDefines, quiet, gate, embedDirs, embeds, overrides);
            pre.SetActiveFilename(Path.GetFileName(unitPath));
            using var lexer = BytesLexer.FromString(source, lexerTable);
            using var mappedLexer = new SourceMappingLexer(lexer, sourceMap);
            using var preproc = pre.WrapStream(mappedLexer);
            // MacroExpander: function-like macro expansion. Needs lookahead
            // for the `(`, which the Rewrite hook can't do — so it lives as
            // its own RewritingTokenStream subclass after the preprocessor
            // populated the macro table.
            using var macroExp = new MacroExpander(preproc, pre);
            using var cTokens = new CTokenValidator(macroExp);
            // DialectKeywordRewriter: dialect-aware keyword promotion (rule 2
            // of the gating model). Promotes identifier-spelled keywords
            // (e.g. C23 `bool`) onto their grammar terminal only when the
            // active dialect makes them first-class. Sits AFTER macro
            // expansion (so an included header's `#define bool _Bool` wins and
            // the table simply doesn't fire) and BEFORE the typedef rewriter
            // (so e.g. `typedef bool MyBool;` under c23 sees `_Bool`).
            using var physicalTokens = new PhysicalPositionRewriter(cTokens);
            using var dialectRewriter = new DialectKeywordRewriter(physicalTokens, activeDialect);
            // TypeNameRewriter: the C lexer hack. Promotes ID → TYPE_NAME for
            // any name previously bound by a `typedef`. Sits AFTER macro
            // expansion (so expanded names can also trigger typedef
            // rewrites) and BEFORE the LA iterator. Seeded with the small
            // set of C#-side libc classes that user code reaches by name
            // through synthetic-header typedefs (e.g. <setjmp.h>'s
            // `typedef LongJmpToken jmp_buf;`).
            // (`const`/`volatile` used to be token-stripped here by a
            // QualifierStripper stage; they now parse as Type prefix/postfix
            // productions and carry a CType qualifier flag, so no stripping.)
            using var typeRewriter = new TypeNameRewriter(dialectRewriter, Compiler.PredefinedTypeNames);
            // Fold sizeof(T) → number so sizeof(int)*8 → 4*8, avoiding an LALR
            // conflict between ArrDims→[E] and Subscript→E[E] that drops
            // binary operators after sizeof in all contexts.
            using var sizeofFolder = new SizeofFolder(typeRewriter);
            using var tokens = new SyncLATokenIterator(sizeofFolder);

            Item result;
            try
            {
                result = parser.ParseInput(tokens, debugger: null, trimReductions: true);
            }
            catch (global::LALR.CC.ParseErrorException ex)
            {
                throw new CompileException($"parse failed in {SourceFileOrigin.Of(ex.OffendingToken)?.Name ?? Path.GetFileName(unitPath)}: {ex.Message}", ex);
            }
            catch (global::LALR.CC.LexicalGrammar.LexerException ex)
            {
                // A byte the lexer can't tokenize (e.g. a stray `\` that wasn't a
                // line continuation, an illegal control char). Surface it as the
                // same stable CompileException as a parse failure rather than
                // letting the raw LexerException escape unhandled.
                throw new CompileException($"lex failed in {Path.GetFileName(unitPath)}: {ex.Message}", ex);
            }
            if (result.IsError)
            {
                throw new CompileException($"parse failed in {Path.GetFileName(unitPath)}: {result}");
            }
            macroBodies.AddRange(pre.ConstantMacroBodies(defines));
            return result;
        }

        // ---- Typed-IR backend ------------------------------------------
        // dotcc compiles C → typed IR → low-level C#. The parse tree (yielded
        // raw by the identity visitor) is bound to a typed AST by IrBuilder, then
        // CSharpBackend prints it precedence-aware. The wat backend is the peer; the
        // retired bottom-up string emitter is gone — its shared identifier /
        // string-literal / export helpers live on in EmitHelpers, and the last
        // cross-cutting concern it owned (malloc→stack promotion) now runs as an
        // IR pass (IrBuilder.MallocPromote).
        //
        // -pedantic dialect gating: collect features that postdate -std= during
        // parse (preprocessor-era) and IR build (emit-pass), then flush as warnings
        // (-pedantic) or one collected error (-pedantic-errors). Off by default.
        var gate = (pedantic || pedanticErrors) ? new DialectGate(activeDialect) : null;
        var irBuilder = new Ir.IrBuilder(gate, names ?? new Backends.CSharpNameLegalizer(), embeds, warnings) { StableAnonymousNames = req.ObjectMode };
        var irParser = C.BuildSourceLocatedParser();
        foreach (var unitPath in inputPaths)
        {
            var root = ParseUnit(unitPath, irParser, quiet: false, gate);
            irBuilder.AddUnit(root, Path.GetFileName(unitPath));
        }
        overrides?.Complete();
        irBuilder.FinishAggregateTypes();
        foreach (var global in irBuilder.Globals)
            if (global.Init is { } init) Ir.IrBuilder.RequireNoRuntimeIntrinsic(init, "static initializer of " + global.Sym.Name);
        foreach (var intrinsic in irBuilder.RuntimeIntrinsicsUsed.OrderBy(x => x))
            overrides?.Event("intrinsic", ("name", intrinsic.ToString()), ("type", "_Bool"), ("evaluation", "runtime"));
        foreach (var macro in macroBodies)
        {
            irBuilder.MacroConstants.Add((macro.Name, CConstantMacros.TryLower(macro.Body)));
            if (macro.Body.Any(t => t.Content?.ToString() == RuntimeIntrinsicNames.IsLittleEndian))
                overrides?.Event("macro-export-skipped", ("name", macro.Name), ("reason", "runtime intrinsic is not a constant"));
        }
        var irErrors = irBuilder.Diagnostics.Where(d => d.Severity == Ir.Severity.Error).ToList();
        if (irErrors.Count > 0)
        {
            throw new CompileException(string.Join("\n", irErrors.Select(d => "error: " + d)));
        }
        foreach (var w in irBuilder.Diagnostics.Where(d => d.Severity == Ir.Severity.Warning))
        {
            Console.Error.WriteLine("dotcc: warning: " + w);
        }
        // Flush dialect-gate diagnostics: -pedantic-errors collects all into one
        // CompileException (collect-all, fail once); -pedantic warns and continues.
        if (gate is { HasAny: true })
        {
            if (pedanticErrors)
            {
                throw new CompileException(string.Join("\n", gate.Diagnostics.Select(d => "error: " + d)));
            }
            foreach (var d in gate.Diagnostics) { Console.Error.WriteLine("dotcc: warning: " + d); }
        }
        return irBuilder;
    }
}
