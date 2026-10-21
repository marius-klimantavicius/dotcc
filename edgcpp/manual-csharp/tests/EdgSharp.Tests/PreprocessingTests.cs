// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored preprocessing/semantic integration controls; no native dependency.
using System.Numerics;
using EdgSharp;

internal static partial class Tests
{
    private static PreprocessingResult Preprocessed(string source, PreprocessingOptions? options = null, string name = "input.cpp")
    {
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options ?? new() });
        PreprocessingResult result = session.Preprocess(source, name);
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        return result;
    }
    private static string PpText(PreprocessingResult result)
    {
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        return string.Join(" ", result.Tokens.Tokens.Where(t => t.Kind != TokenKind.EndOfSource).Select(t => t.Spelling));
    }
    private static string Pp(string source) => PpText(Preprocessed(source));
    internal static void PreprocessingMacros()
    {
        Equal("7 8 9 F ( 7 )", Pp("#define F(x) x\n#define A F\n#define ID(x) x\nF(F(7)) A(8) ID(F)(9) F(F)(7)"));
        Equal("A B", Pp("#define A B\n#define B A\nA B"));
        Equal("+ + < < alpha beta 1 2", Pp("#define PLUS +\n#define LEFT <\n#define I alpha\n#define N 1\nPLUS+ LEFT< I beta N 2"));
        Equal("3", Pp("#define F(x) x\nF\n(3)"));
        Equal("( x ) x ( 7 )", Pp("#define F/**/(x) x\nF(7)"));
        Equal("7", Pp("#define F\\\n(x) x\nF(7)"));
        Equal("long item ; 1", Pp("#define int long\n#define and 1\nint item; and"));
        Equal("object \"a+x\" function \"a+x\" multi \"a+x+y\" spaced \"a+ x\" empty_before \"a +b\" empty_after \"a+b\" empty_adjacent \"a++b\"", Pp(Fixture("m1-rescan-whitespace.cpp")));
        using var session = new CompilationSession();
        Check(!session.Preprocess("#define F(x) x\nF({1,2})").IsSuccess); // Only parentheses shield commas.
        PreprocessingResult boundaries = Preprocessed("#define P +\nP+ < < foo bar 1 2");
        Check(boundaries.Tokens.Source.Kind == SourceFileKind.PreprocessedPresentation);
        Check(boundaries.Tokens.Tokens.Take(4).Select(t => t.Kind).SequenceEqual(new[] { TokenKind.Plus, TokenKind.Plus, TokenKind.Less, TokenKind.Less }));
    }
    internal static void PreprocessingVariadics()
    {
        Equal("unused 7 stringized \"BAD(1)\" vaopt \"\" \"a\"", Pp(Fixture("m1-lazy-arguments.cpp")));
        Equal("1 1 , 2 , 3", Pp("#define EMPTY\n#define V(...) 1 __VA_OPT__(, __VA_ARGS__)\nV(EMPTY) V(2,3)"));
        Equal("ab a b", Pp("#define CAT(a,b) a##b\nCAT(a,b) CAT(a,) CAT(,b) CAT(,)"));
        Equal("\"a b\" \"a+b\"", Pp("#define S(x) #x\nS(  a /**/ b  ) S(a+b)"));
        Equal("\"\" \"hello\"", Pp("#define V(...) #__VA_OPT__(hello)\nV() V(1)"));
        Equal("\"\" X \"\" \"\" X \"\"", Pp("#define V(...) #__VA_OPT__() X##__VA_OPT__() #__VA_OPT__( )\nV() V(1)"));
        Equal("\"##\" \"a##b\"", Pp("#define HH # ## #\n#define S(x) #x\n#define E(x) S(x)\nE(HH) S(a##b)"));
        using var session = new CompilationSession();
        foreach (string invalid in new[] { "#define F(x) x\n#define F(y) y", "#define X+1", "#define V(...) __VA_OPT__(## a)", "#define V(...) __VA_OPT__(a ##)", "#define F(x) #7", "#define C(a,b) a##b\nC(+,*)" })
            Check(!session.Preprocess(invalid).IsSuccess, invalid);
        PreprocessingResult repeated = Preprocessed("#define F(x) x\n#define F(x) x\nF(1)");
        Equal(1, repeated.Macros["F"].NameOrigin.Presumed.Line);
    }
    internal static void PreprocessingConditionals()
    {
        Equal("yes", Pp("#define X 9\n#if defined(X) && !defined(MISSING) && true && MISSING == 0\nyes\n#else\nno\n#endif"));
        Equal("yes", Pp("#if (1 || 1/0) && (0 ? 1/0 : 3) == 3 && (0 && 1/0) == 0\nyes\n#endif"));
        Equal("yes", Pp("#if (1LL<<63) == (-9223372036854775807LL-1) && (-1LL<<1)==-2 && (0xffffffffffffffffU+1)==0\nyes\n#endif"));
        Equal("yes", Pp("#if not false and (6 bitand 3)==2 and (4 bitor 1)==5 and (6 xor 3)==5 and (compl 0)==-1 and 1 not_eq 2\nyes\n#endif"));
        Equal("chosen", Pp("#if 0\n#if BAD(\nwrong\n#endif\n#elif 1\nchosen\n#else\nwrong\n#endif"));
        using var session = new CompilationSession();
        Check(!session.Preprocess("#if 1/0\nx\n#endif").IsSuccess);
        Check(!session.Preprocess("#if 1 || (\nx\n#endif").IsSuccess); // Dead operands must still have valid syntax.
        foreach (string source in new[] { "#if 'a'\nx\n#endif", "#if __has_include(\"x\")\nx\n#endif" })
            Check(session.Preprocess(source).Diagnostics.Any(d => d.Class == DiagnosticClass.UnsupportedFeature));
        Check(!session.Preprocess("#warning invalid in strict20").IsSuccess);
    }
    internal static void PreprocessingIncludes()
    {
        var files = new Dictionary<string, string>
        {
            ["/inc/value.h"] = "#ifndef VALUE_H\n#define VALUE_H\n#include \"value.h\"\n#define VALUE 7\n#endif\n",
            ["/quotes/other.h"] = "quote\n", ["/system/other.h"] = "system\n",
            ["/inc/cross.h"] = "#endif\n", ["/inc/open.h"] = "#if 1\n",
            ["/inc/end.h"] = "F\n", ["/inc/args.h"] = "(7)\n", ["/header.h"] = "root\n"
        };
        var options = new PreprocessingOptions(new InMemoryIncludeProvider(files), ["/quotes"], ["/system"]);
        Equal("quote system 7", PpText(Preprocessed("#line 80 \"renamed/virtual.cpp\"\n#include \"value.h\"\n#include \"other.h\"\n#include <other.h>\nVALUE", options, "/inc/root.cpp")));
        Equal("F ( 7 )", PpText(Preprocessed("#define F(x) x\n#include \"end.h\"\n(7)", options, "/inc/root.cpp")));
        Equal("F ( 7 )", PpText(Preprocessed("#define F(x) x\nF\n#include \"args.h\"", options, "/inc/root.cpp")));
        Equal("root", PpText(Preprocessed("#include \"header.h\"", options, "/root.cpp")));
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options });
        foreach (string header in new[] { "cross.h", "open.h" })
            Check(!session.Preprocess("#if 1\n#include \"" + header + "\"\n#endif", "/inc/root.cpp").IsSuccess);
        PreprocessingResult retained = Preprocessed("#include \"value.h\"\nVALUE", options, "/inc/root.cpp");
        Equal(2, retained.PhysicalSources.Count);
        Equal(2, retained.Includes.Count); // Guarded re-entry is permitted.
        Check(retained.Tokens.Tokens[0].Origin.ExpansionNotes.Any(n => n.Contains("included from", StringComparison.Ordinal)));
    }
    internal static void PreprocessingOrigins()
    {
        PreprocessingResult expanded = Preprocessed("#define G 7\n#define ID(x) x\n#define F(x) ID(x)\nF(G)", name: "origin.cpp");
        Token token = expanded.Tokens.Tokens[0];
        Check(token.Origin.ExpansionHistory.Select(f => f.Name).Distinct().Order().SequenceEqual(new[] { "F", "G", "ID" }));
        Check(token.Origin.ExpansionHistory.All(f => f.Definition.File.Kind == SourceFileKind.Physical));
        Equal(4, token.Origin.PrimaryPhysicalRange.StartPosition.Line);
        Equal(1, token.Origin.SpellingRanges[0].StartPosition.Line);
        PreprocessingResult line = Preprocessed("#line 20 \"a\\x62.cpp\"\n__LINE__ __FILE__");
        Equal("20 \"ab.cpp\"", PpText(line));
        Equal("direct 6 wrapped 9 nested 7 logical 82", Pp(Fixture("m1-multiline-origin.cpp")));
        SourceFile physical = new("splice.cpp", "ab\\\nc");
        SourceFile logical = LogicalInput.Normalize(physical);
        TokenOrigin origin = logical.OriginForRange(logical.Range(1, 1));
        Equal(1, origin.SpellingRanges.Count); Equal("b", origin.SpellingRanges[0].Spelling); Equal(2, origin.Presumed.Column);
        SourceFile empty = new("empty.cpp", "");
        TokenOrigin emptyOrigin = empty.OriginForRange(empty.Range(0, 0));
        Equal(0, emptyOrigin.SpellingRanges[0].Length); Equal(1, emptyOrigin.Presumed.Line); Equal(1, emptyOrigin.Presumed.Column);
        TokenOrigin eof = logical.OriginForRange(logical.Range(logical.Text.Length, 0));
        Equal(physical.Text.Length, eof.SpellingRanges[^1].Start); Equal(2, eof.SpellingRanges[^1].StartPosition.Line);
        TokenOrigin boundary = logical.OriginForRange(logical.Range(2, 0));
        Equal(1, boundary.SpellingRanges.Count); Equal(4, boundary.SpellingRanges[0].Start);
        Equal(0, boundary.SpellingRanges[0].Length); Equal(2, boundary.Presumed.Line); Equal(1, boundary.Presumed.Column);
        PreprocessingResult emptyOutput = Preprocessed("#define EMPTY\nEMPTY");
        Token emptyEof = emptyOutput.Tokens.Tokens.Single();
        Equal(TokenKind.EndOfSource, emptyEof.Kind); Equal(SourceFileKind.Physical, emptyEof.Origin.SpellingRanges[0].File.Kind);
        Equal(emptyOutput.Source.Text.Length, emptyEof.Origin.SpellingRanges[0].Start);
        Token adjacent = Preprocessed("#define A 1\n#define B 2\nA B").Tokens.Tokens[0];
        TokenOrigin exact = adjacent.Range.File.OriginForRange(adjacent.Range);
        Check(exact.ExpansionHistory.Select(f => f.Name).SequenceEqual(new[] { "A" }));
        PreprocessingResult builtin = Preprocessed("#define A __LI\n#define B NE__\n#define CAT(x,y) x##y\n#define E(x,y) CAT(x,y)\nE(A,B)");
        Equal("5", PpText(builtin));
        Check(new[] { "A", "B", "E", "CAT" }.All(name => builtin.Tokens.Tokens[0].Origin.ExpansionHistory.Any(f => f.Name == name)));
        AnalysisResult graph = Success("#define A 1\n#define B 2\nint f(){return A+B;}");
        TokenOrigin binary = ReturnValue(Function(graph, "f")).Range.File.OriginForRange(ReturnValue(Function(graph, "f")).Range);
        Check(binary.ExpansionHistory.Select(f => f.Name).Distinct().Order().SequenceEqual(new[] { "A", "B" }));
        AnalysisResult error = Analyze("#define BAD unknown\n#line 90 \"logical.cpp\"\nint f(){return BAD;}");
        Diagnostic diagnostic = error.Diagnostics.First(d => d.Message.Contains("unknown", StringComparison.Ordinal));
        Equal("logical.cpp", diagnostic.PrimaryLocation.FileName); Equal(90, diagnostic.PrimaryLocation.Line);
        Check(diagnostic.ExpansionNotes.Any(note => note.Contains("BAD", StringComparison.Ordinal)));
        Throws<InvalidOperationException>(() => SourceRange.Cover(physical.Range(0, 1), graph.Source.Range(0, 1)));
    }
    internal static void PreprocessingPhases()
    {
        Equal("value 7", Pp("#define X /* spanning\n comment */ 7\nvalue X"));
        Equal("value 28 + 28", Pp("#line 27\nvalue __LINE__ \\\n+ __LINE__"));
        Equal("int x = 1 ;", Pp("#define UNUSED L\"x\"\n#if 0\nL\"inactive\"\nR\"tag(first\nsecond\\\nthird)tag\"\n#endif\nint x=1;"));
        Equal("R\"tag(first\nsecond\\\nthird)tag\"", Pp("R\"tag(first\nsecond\\\nthird)tag\""));
        Equal("value R\"(a\nb)\" + 7", Pp("#define X R\"(a\nb)\" + 7\nvalue X"));
        Equal("int x ;", Pp("// comment\\\n#define WRONG\nint x;"));
        Equal("# define X 7 X", Pp("#define HASH #\nHASH define X 7\nX")); // Expansion cannot form a directive.
        Equal("int value = 1 ;", Pp("/\\\n/ R\"(\\\n)\"\nint value=1;"));
        using var session = new CompilationSession();
        Check(!session.Preprocess("#define UNUSED R\"(foo\"").IsSuccess);
        Check(session.Preprocess("#define UNUSED R\"(foo\"\n").Diagnostics.Any(d => d.Class == DiagnosticClass.UnsupportedFeature));
        foreach (string raw in new[] { "R\"(unterminated\"", "R\"tag(text)wrong\"", "R\"bad delimiter(text)bad delimiter\"" })
            Check(session.Preprocess(raw).Diagnostics.Any(d => d.Code == "LEX004"), raw);
        Equal("R\"tag(embedded \" quote /* comment */ \\\n newline)tag\"", Pp("R\"tag(embedded \" quote /* comment */ \\\n newline)tag\""));
        Equal("\"a\\n\\t.cpp\"", Pp("#line 9 \"a\\n\\t.cpp\"\n__FILE__"));
        foreach (string number in new[] { "1.5", "1.0_x", "1_x", "1.0_xe" })
            Check(Analyze("int value=" + number + ";").ContainsUnsupportedFeatures, number);
        AnalysisResult invalid = Analyze("int value=1e+;");
        Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures && invalid.Diagnostics.Any(d => d.Code == "LEX011"));
        Check(!session.Lex("1e+").IsSuccess);
        Equal("int value = 1 ;", Pp("#define UNUSED 1e+\n#if 0\n1e+\n#endif\nint value=1;"));
        Equal("1e+", Pp("1e+")); // A pp-number need not already be a valid language literal.
    }
    internal static void PreprocessingSemantics()
    {
        var options = new PreprocessingOptions(new InMemoryIncludeProvider(new Dictionary<string, string> { ["m1-integrated.h"] = Fixture("m1-integrated.h") }));
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options });
        AnalysisResult graph = session.Analyze(Fixture("m1-integrated.cpp"), "m1-integrated.cpp");
        Check(graph.IsSuccess, string.Join("; ", graph.Diagnostics.Select(d => d.Message)));
        Same(graph.Source, graph.Preprocessing.Source);
        Check(graph.Source.Kind == SourceFileKind.Physical && graph.Tokens.Source.Kind == SourceFileKind.PreprocessedPresentation);
        Equal(1, graph.StaticAssertions.Count);
        Equal(ConstantEvaluationStatus.Success, graph.StaticAssertions[0].Evaluation.Status);
        FunctionTemplateDeclaration template = Template(graph, "id");
        Equal(1, template.Instances.Count);
        Same(graph.Tokens.Source, template.BodyTokens.Source);
        Check(template.BodyTokens.Tokens.Any(t => t.Origin.ExpansionHistory.Any(f => f.Name == "BODY")));
        Check(Success("int\n#define V 1\nx; int f(int\n#define W 2\ny){return y+V+W;}").IsSuccess);
    }
    internal static void PreprocessingState()
    {
        var macros = new Dictionary<string, string> { ["CONFIG"] = "4" };
        var roots = new List<string> { "/first" };
        var input = new Dictionary<string, string> { ["/first/x.h"] = "CONFIG\n" };
        var options = new PreprocessingOptions(new InMemoryIncludeProvider(input), roots, predefinedMacros: macros);
        macros["CONFIG"] = "9"; roots[0] = "/other"; input["/first/x.h"] = "bad";
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options });
        Equal("4", PpText(session.Preprocess("#include \"x.h\"")));
        Equal("7", PpText(session.Preprocess("#define TEMP 7\nTEMP")));
        Equal("TEMP 4", PpText(session.Preprocess("TEMP CONFIG")));
        Throws<ArgumentOutOfRangeException>(() => new PreprocessingOptions(workLimit: 0));
        Throws<ArgumentNullException>(() => new CompilationSession(new AnalysisOptions { Preprocessing = null! }));
        var recursive = new PreprocessingOptions(new InMemoryIncludeProvider(new Dictionary<string, string> { ["self.h"] = "#include \"self.h\"" }), includeDepthLimit: 2);
        using var bounded = new CompilationSession(new AnalysisOptions { Preprocessing = recursive });
        Check(bounded.Preprocess("#include \"self.h\"").Diagnostics.Any(d => d.Class == DiagnosticClass.ResourceLimit));
        using var workBounded = new CompilationSession(new AnalysisOptions { Preprocessing = new(workLimit: 3) });
        Check(workBounded.Preprocess("a b c d e").Diagnostics.Any(d => d.Class == DiagnosticClass.ResourceLimit));
        PreprocessingResult retained = session.Preprocess("#define X 7\nX"); session.Dispose();
        Equal("7", PpText(retained)); Equal("7", retained.Tokens.Tokens[0].Origin.SpellingRanges[0].Spelling);
        Throws<ObjectDisposedException>(() => session.Preprocess("int x;"));
    }
    internal static void PreprocessingUpstream()
    {
        // Exact retained upstream C++20 preprocess-only input. Expected token
        // stream is authored from the pinned EDG receipt; string contents and
        // token boundaries remain significant while layout between tokens does not.
        string expected = """
            1: F, (, 'a', 'b', );
            2: 'a' + 'b';
            3: f(0 , a, b, c)
            4: f(0 )
            5: f(0, a , b, c)
            6: f(0, a )
            7: f(0, a )
            7.1: f(0, a , ,)
            8: HT_
            9: TONG C ( ) B ( ) "A()"
            10: ""
            11: "A1 expandedA1 expandedA0 B ( )"
            12.0:
            12:
            13: BB
            14: "" X ""
            15: "" X ""
            16: S foo ;
            17: S bar = { 1, 2 };
            18: B ( ) "" B ( )
            19: B ( ) "" B ( )
            20: B ( ) "A3 expandedA3 expandedA2 expandedA1 expandedA0 A3C A3" B ( )
            21: B ( ) "A3 B ( ),expandedA0 A3A(),A0A3C A3" B ( )
            22: B ( ) "" B ( )
            23: B ( ) "" B ( )
            24: B ( ) "A3 expandedA0 A0C A3" expandedA0 expandedA0 A0C expandedA0 B ( )
            25: B ( ) "A3 B ( ),expandedA0 A0A(),A0A0C A3" expandedA0 expandedA0 C ( ),expandedA0 A0A(),A0A0C expandedA0 B ( )
            26: B 1
            26_1: B 1
            27: B 11
            27_1: BexpandedA0 11
            28: B 11
            """;
        using var session = new CompilationSession();
        TokenBuffer expectedTokens = session.Lex(expected).Tokens;
        string expectedText = string.Join(" ", expectedTokens.Tokens.Where(t => t.Kind != TokenKind.EndOfSource).Select(t => t.Spelling));
        Equal(expectedText, Pp(Fixture("m1-upstream-vaopt.cpp")));
    }
    internal static void PreprocessingContributors()
    {
        PreprocessingResult result = Preprocessed("#define A 1\n#define CAT(x,y) x##y\n#define STR(x) #x\n#define E(x) STR(x)\nCAT(a,b) E(A)");
        Token paste = result.Tokens.Tokens[0], text = result.Tokens.Tokens[1];
        Equal(TokenOriginKind.TokenPaste, paste.Origin.Kind);
        Check(new[] { "a", "b", "##" }.All(spelling => paste.Origin.SpellingRanges.Any(r => r.Spelling == spelling)));
        Check(paste.Origin.ExpansionHistory.Any(f => f.Name == "CAT"));
        Equal(TokenOriginKind.Stringization, text.Origin.Kind);
        Check(text.Origin.SpellingRanges.Any(r => r.Spelling == "#"));
        Check(text.Origin.SpellingRanges.Any(r => r.Spelling == "1"));
        Check(new[] { "A", "E", "STR" }.All(name => text.Origin.ExpansionHistory.Any(f => f.Name == name)));
        var provider = new InMemoryIncludeProvider(new Dictionary<string, string> { ["/h.h"] = "#line 50 \"header-virtual.h\"\n__LINE__\n" });
        PreprocessingResult included = Preprocessed("#line 10 \"first.cpp\"\n#include \"h.h\"\n#line 30 \"second.cpp\"\n#include \"h.h\"\n__LINE__", new(provider), "/root.cpp");
        Equal("50 50 31", PpText(included));
        Equal(2, included.Includes.Count);
        Check(!ReferenceEquals(included.Includes[0], included.Includes[1]));
        Same(included.Includes[0].File, included.Includes[1].File);
        Equal("first.cpp", included.Includes[0].Directive.Presumed.FileName);
        Equal("second.cpp", included.Includes[1].Directive.Presumed.FileName);
        Token first = included.Tokens.Tokens[0], second = included.Tokens.Tokens[1];
        Same(included.Includes[0], first.Origin.Include);
        Same(included.Includes[1], second.Origin.Include);
        TokenOrigin aggregate = first.Range.File.OriginForRange(SourceRange.Cover(first.Range, second.Range));
        Check(aggregate.ExpansionNotes.Any(n => n.Contains("first.cpp:10:", StringComparison.Ordinal)));
        Check(aggregate.ExpansionNotes.Any(n => n.Contains("second.cpp:30:", StringComparison.Ordinal)));
        Equal("header-virtual.h", first.Origin.Presumed.FileName);
        Equal("second.cpp", included.Tokens.Tokens[2].Origin.Presumed.FileName);
    }
    internal static void PreprocessingConcurrency()
    {
        Task<string>[] analyses = Enumerable.Range(1, 8).Select(value => Task.Run(() =>
        {
            var options = new PreprocessingOptions(new InMemoryIncludeProvider(new Dictionary<string, string> { ["local.h"] = "#define LOCAL " + value + "\nLOCAL CONFIG\n" }),
                predefinedMacros: new Dictionary<string, string> { ["CONFIG"] = (value + 10).ToString(System.Globalization.CultureInfo.InvariantCulture) });
            using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options });
            return PpText(session.Preprocess("#include \"local.h\""));
        })).ToArray();
        Task.WaitAll(analyses);
        for (int i = 0; i < analyses.Length; ++i) Equal($"{i + 1} {i + 11}", analyses[i].Result);
    }
    internal static void PreprocessingDifficultGate()
    {
        var provider = new InMemoryIncludeProvider(new Dictionary<string, string> { ["m1-difficult.h"] = Fixture("m1-difficult.h") });
        AnalysisResult result;
        using (var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(provider) }))
            result = session.Analyze(Fixture("m1-difficult.cpp"), "m1-difficult.cpp");
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        Equal(3, result.StaticAssertions.Count);
        Check(result.StaticAssertions.All(a => a.Evaluation.Status == ConstantEvaluationStatus.Success));
        FunctionTemplateDeclaration combine = Template(result, "combine"), id = Template(result, "id");
        Equal(2, combine.Instances.Count); Equal(2, id.Instances.Count);
        Check(combine.Instances.All(instance => instance.State == TemplateInstanceState.Complete));
        Check(combine.BodyTokens.Tokens.Any(t => t.Origin.ExpansionHistory.Any(f => f.Name == "BODY")));
        Check(combine.BodyTokens.Tokens.Any(t => t.Origin.ExpansionNotes.Any(n => n.Contains("included from", StringComparison.Ordinal))));
        CallExpression call = ReturnedCall(result, "run");
        Same(combine.Instances.Single(instance => Kind(instance.TypeArguments[0]) == IntegerKind.Int).Symbol, call.SelectedFunction);
        Equal(1, combine.DefinitionContext.Bindings["mark"].Count);
        Equal(2, result.GlobalScope.FindLocalBinding("mark")!.Symbols.Count);
        foreach (FunctionTemplateInstance instance in combine.Instances)
        {
            Equal(1, instance.BodyReplayCount);
            var binary = As<BinaryExpression>(ReturnValue(instance.Declaration!));
            CallExpression mark = As<CallExpression>(binary.Left);
            Equal(IntegerKind.Long, SelectedParameter(mark)); Equal(1, mark.Candidates.Count);
            Same(combine.DefinitionContext.Bindings["mark"][0], mark.SelectedFunction);
            Check(binary.Range.File.OriginForRange(binary.Range).ExpansionHistory.Any(f => f.Name == "BODY"));
            var copy = As<VariableDeclaration>(As<DeclarationStatement>(instance.Declaration!.Body!.Statements[0]).Declaration);
            Check(As<CallExpression>(copy.Initializer).TemplateInstance != null);
        }
        FunctionTemplateDeclaration losing = result.GlobalScope.FindLocalBinding("pick")!.Symbols.Single(s => s.Kind == SymbolKind.FunctionTemplate).TemplateDefinition!;
        Equal(0, losing.Instances.Count);
        Equal(IntegerKind.Int, SelectedParameter(ReturnedCall(result, "ordinary")));
    }
}
