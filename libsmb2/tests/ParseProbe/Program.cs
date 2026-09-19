using System.Diagnostics;
using System.Text.Json;
using DotCC;
using LALR.CC;
using LALR.CC.LexicalGrammar;

// Drive CFrontend's actual token pipeline, stopping before IrBuilder.AddUnit.
// Preprocessing is exhausted separately so an early parse failure cannot hide a
// later lexical failure. Diagnostics are retained even when no exception occurs.
var request = JsonSerializer.Deserialize<Request>(File.ReadAllText(args[0]))!;
File.Delete(request.Output);
var lexerTable = C.BuildLexer();
var parser = C.BuildSourceLocatedParser();
var results = new List<object>();
foreach (var unit in request.Units)
{
    var stages = new List<object>();
    foreach (var stage in new[] { "preprocess_lex", "parse" })
    {
        var watch = Stopwatch.StartNew();
        using var diagnostics = new StringWriter();
        var previousError = Console.Error;
        string? failure = null;
        string? exceptionType = null;
        long tokenCount = 0;
        Console.SetError(diagnostics);
        try
        {
            var includeMap = Compiler.BuildIncludeMap(new[] { unit }, request.Includes);
            var sourceMap = new PhysicalSourceMap(File.ReadAllText(unit),
                filename: Path.GetFileName(unit), identity: Path.GetFullPath(unit));
            var pre = new CPreprocessor(lexerTable, includeMap,
                Compiler.SeedDialectDefines(CDialect.Parse("c17"), request.Defines));
            pre.SetActiveFilename(Path.GetFileName(unit), unit);
            using var lexer = BytesLexer.FromString(sourceMap.Text, lexerTable);
            using var mapped = new SourceMappingLexer(lexer, sourceMap);
            using var preproc = pre.WrapStream(mapped);
            using var expanded = new MacroExpander(preproc, pre);
            using var validated = new CTokenValidator(expanded);
            if (stage == "preprocess_lex")
            {
                while (validated.MoveNext()) tokenCount++;
            }
            else
            {
                using var positioned = new PhysicalPositionRewriter(validated);
                using var dialect = new DialectKeywordRewriter(positioned, CDialect.Parse("c17"));
                using var types = new TypeNameRewriter(dialect, Compiler.PredefinedTypeNames);
                using var sizes = new SizeofFolder(types);
                using var tokens = new SyncLATokenIterator(sizes);
                var root = parser.ParseInput(tokens, debugger: null, trimReductions: true);
                if (root.IsError) throw new Exception(root.ToString());
            }
        }
        catch (Exception ex)
        {
            failure = ex.Message;
            exceptionType = ex.GetType().FullName;
            if (ex is ParseErrorException parseError)
                failure = $"{SourceFileOrigin.Of(parseError.OffendingToken)?.Name}: {failure}";
        }
        finally { Console.SetError(previousError); }
        var messages = diagnostics.ToString();
        var clean = failure is null && string.IsNullOrWhiteSpace(messages);
        stages.Add(new { stage, clean, completed = failure is null, tokenCount,
            seconds = watch.Elapsed.TotalSeconds, failure, exceptionType, diagnostics = messages });
        Console.WriteLine($"{Path.GetFileName(unit)} {stage}: {(clean ? "PASS" : "BLOCKED")} {failure}");
    }
    results.Add(new { unit, stages });
    File.WriteAllText(request.Output, JsonSerializer.Serialize(results,
        new JsonSerializerOptions { WriteIndented = true }) + "\n");
}

record Request(string[] Units, string[] Includes, string[] Defines, string Output);
