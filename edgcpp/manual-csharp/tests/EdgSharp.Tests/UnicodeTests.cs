// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored Unicode17/source-policy controls and native witness proposals.
// Execution, native comparison and acceptance belong to the supervisor.
using System.Numerics;
using System.Text;
using EdgSharp;

internal static partial class Tests
{
    private static string IdentifierOutput(PreprocessingResult result)
    {
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        return string.Join(" ", result.Tokens.Tokens.Where(t => t.Kind != TokenKind.EndOfSource).Select(t => t.IdentifierText));
    }
    internal static void UnicodeIdentifierTokens()
    {
        using var session = new CompilationSession();
        foreach (var (raw, canonical) in new[]
        {
            ("é", "é"), ("\\u00E9", "é"), ("\\U000000e9", "é"),
            ("𐐀", "𐐀"), ("\\U00010400", "𐐀"), ("a\\u0301", "á"), ("á", "á"),
            ("_a9", "_a9"), ("a\\u200C", "a‌")
        })
        {
            LexicalResult lexed = session.Lex(raw);
            Check(lexed.IsSuccess, raw);
            Token token = lexed.Tokens.Tokens[0];
            Equal(2, lexed.Tokens.Tokens.Count); Equal(TokenKind.Identifier, token.Kind);
            Equal(raw, token.Spelling); Equal(canonical, token.IdentifierText);
            Equal(raw, token.Origin.SpellingRanges.Single().Spelling);
        }
        TokenBuffer supplementary = session.Lex("𐐀 x").Tokens;
        Equal(2, supplementary.Tokens[0].Range.Length); Equal(3, supplementary.Tokens[1].Range.Start);
        Equal(4, supplementary.Tokens[1].Range.StartPosition.Column);
        Token[] distinct = session.Lex("é é").Tokens.Tokens.ToArray();
        Check(distinct[0].IdentifierText != distinct[1].IdentifierText, "Identifier identity must not compose or decompose Unicode.");
        string literals = "\"\\u00E9\" R\"(\\u00E9)\" U'\\U00010400'";
        Equal(literals, string.Join(" ", session.Preprocess(literals).Tokens.Tokens.Where(t => t.Kind != TokenKind.EndOfSource).Select(t => t.IdentifierText)));
        TokenCursor replay = session.Lex("\\u00e9 é").Tokens.CreateCursor();
        TokenCheckpoint checkpoint = replay.Checkpoint();
        replay.Advance(); replay.Restore(checkpoint);
        Equal("\\u00e9", replay.Current.Spelling); Equal("é", replay.Current.IdentifierText);
        Token spliced = session.Lex("\\u00\\\ne9").Tokens.Tokens[0];
        Equal("\\u00e9", spliced.Spelling); Equal("é", spliced.IdentifierText);
        Equal(2, spliced.Origin.SpellingRanges.Count);
        Equal("\\u00", spliced.Origin.SpellingRanges[0].Spelling); Equal("e9", spliced.Origin.SpellingRanges[1].Spelling);
    }
    internal static void Unicode17Boundaries()
    {
        Equal("17.0.0", UnicodeIdentifiers.UnicodeVersion);
        using var session = new CompilationSession();
        foreach (int scalar in new[] { 0x00aa, 0x00b5, 0x037f, 0x10400, 0x11db0, 0x11ddb, 0x1e6c0, 0x1e6de, 0x33479 })
        {
            Check(UnicodeIdentifiers.IsStart(scalar) && UnicodeIdentifiers.IsContinue(scalar));
            Check(session.Lex(char.ConvertFromUtf32(scalar)).IsSuccess, $"U+{scalar:X}");
            AnalysisResult result = Analyze("int " + char.ConvertFromUtf32(scalar) + "=1;");
            Check(result.IsSuccess, $"Unicode17 start U+{scalar:X}");
        }
        foreach (int scalar in new[] { 0x00b7, 0x0300, 0x036f, 0x0660, 0x200c, 0x200d, 0xfe0f, 0x104a0, 0x11de9 })
        {
            Check(!UnicodeIdentifiers.IsStart(scalar) && UnicodeIdentifiers.IsContinue(scalar), $"Continuation U+{scalar:X}");
            string character = char.ConvertFromUtf32(scalar);
            Check(!session.Lex(character).IsSuccess);
            Check(session.Lex("a" + character).IsSuccess);
        }
        foreach (int scalar in new[] { 0x00a9, 0x00b4, 0x037e, 0x0380, 0x1e6bf, 0x1e6df, 0x3347a, 0xd800, 0x10ffff, 0x110000 })
            Check(!UnicodeIdentifiers.IsStart(scalar) && !UnicodeIdentifiers.IsContinue(scalar), $"Excluded U+{scalar:X}");
        // CLR category tests would wrongly admit some characters whose XID
        // closure excludes them, and cannot pin newly introduced Unicode17 data.
        foreach (int scalar in new[] { 0x037a, 0x0e33, 0x0eb3 }) Check(!UnicodeIdentifiers.IsStart(scalar));
    }
    internal static void UnicodeIdentifierDiagnostics()
    {
        using var session = new CompilationSession();
        foreach (string spelling in new[] { "\\u", "\\u123", "\\U0001040", "\\u00G1", "\\U0000XY00", "\\uD800", "\\uDfff", "\\U00110000", "\\UFFFFFFFF", "\\u0041", "a\\u0030", "\\u005f", "\\u0301", "\\u0660" })
        {
            LexicalResult lexical = session.Lex(spelling);
            Check(!lexical.IsSuccess && lexical.Diagnostics.All(d => d.Class == DiagnosticClass.InvalidSource), spelling);
            AnalysisResult invalid = Analyze("int " + spelling + "=1;");
            Check(!invalid.IsSuccess && invalid.Diagnostics.Any(d => d.Code is "LEX030" or "LEX031" or "LEX032" or "LEX033"), spelling);
        }
        foreach (string source in new[] { "int \ud800;", "// \udfff\nint okay;", "int x=U'\ud800';" })
        {
            AnalysisResult invalid = Analyze(source);
            Check(!invalid.IsSuccess && invalid.Diagnostics.Any(d => d.Code == "SRC001"));
            Equal(OriginalSourceEncoding.ManagedUtf16, invalid.Source.Encoding.OriginalEncoding);
        }
        Check(session.Lex("\\u{e9}").Diagnostics.Any(d => d.Class == DiagnosticClass.UnsupportedFeature));
        Check(!session.Preprocess("#define DUP(é,\\u00e9) 0").IsSuccess);
        Check(!session.Preprocess("#define é 1\n#define \\u00e9 2").IsSuccess);
        Check(!Analyze("int é; long \\u00e9;").IsSuccess);
        Check(!Analyze("template<class é> int \\u00e9(int x){return x;}").IsSuccess);
        Check(!Analyze("template<class é> é f(é x){int \\u00e9=1; return x;}").IsSuccess);
    }
    internal static void UnicodeMacroIdentity()
    {
        Equal("7 7 9", Pp("#define é 7\né \\u00E9\n#undef \\U000000e9\n#define \\u00e9 9\né"));
        Equal("7 7", PpText(Preprocessed("é \\u00e9", new(predefinedMacros: new Dictionary<string, string> { ["é"] = "7", ["\\u00e9"] = "7" }))));
        PreprocessingResult macros = Preprocessed("#define é(α) \\u03b1\n#define \\u00e9(\\u03b1) α\né(3) \\u00e9(4)");
        Equal("3 4", PpText(macros)); Equal("é", macros.Macros["é"].Name);
        Equal("α", macros.Macros["é"].Parameters.Single());
        Equal("yes", Pp("#define é 1\n#if defined(\\u00e9) && defined é\n#ifdef \\U000000e9\nyes\n#endif\n#endif"));
        Equal("yes", Pp("#define \\u03b1 1\n#ifndef α\nno\n#else\nyes\n#endif"));
        Equal("é é", IdentifierOutput(Preprocessed("#define é \\u00E9\né \\u00e9")));
        Equal("α β", IdentifierOutput(Preprocessed("#define α β\n#define \\u03b2 \\u03b1\nα β")));
        Equal("φ ( 7 )", IdentifierOutput(Preprocessed("#define φ(x) x\n\\u03c6(φ)(7)")));
        Equal("\"\\u03b1\"", Pp("#define S(é) #\\u00e9\nS(\\u03b1)"));
        Equal("\"\\\"\\\\u03b1\\\"\"", Pp("#define S(x) #x\nS(\"\\u03b1\")"));
        PreprocessingResult pasted = Preprocessed("#define P(a,b) a##b\nP(\\u00e9,_tail)");
        Token token = pasted.Tokens.Tokens[0];
        Equal("\\u00e9_tail", token.Spelling); Equal("é_tail", token.IdentifierText);
        Equal(TokenOriginKind.TokenPaste, token.Origin.Kind); Check(token.Origin.RelatedOrigins.Count >= 2);
        Equal("int value = 8 ;", Pp("#define P(a,b) a##b\n#define é_tail 8\nint value=P(\\u00e9,_tail);"));
        using var session = new CompilationSession();
        Check(!session.Preprocess("#define P(a,b) a##b\nP(a,\\u0041)").IsSuccess);
        using var configured = new CompilationSession(new AnalysisOptions { Preprocessing = new(predefinedMacros: new Dictionary<string, string> { ["é"] = "7", ["\\u00e9"] = "8" }) });
        Check(configured.Preprocess("é").Diagnostics.Any(d => d.Code == "PP044"));
        // Neither literal contents nor quoted header names are identifier keys.
        Equal("\"\\u00e9\" R\"(\\u00e9)\" U'\\u00e9'", Pp("#define é 5\n\"\\u00e9\" R\"(\\u00e9)\" U'\\u00e9'"));
        const string definitions = "#define A é\n#define B \\u00E9\n#define S(x) #x\n#define E(x) S(x)\n#define ID(x) x\n#define EE(x) E(x)\n";
        PreprocessingResult contexts = Preprocessed(definitions +
            "S(é) S(\\u00E9) E(é) E(\\u00E9) E(A) E(B) S(ID(é)) E(ID(é)) EE(E(A))");
        string[] expected = ["\"é\"", "\"\\u00E9\"", "\"é\"", "\"\\u00E9\"", "\"\\u00e9\"", "\"\\u00e9\"", "\"ID(é)\"", "\"é\"", "\"\\\"\\\\u00e9\\\"\""];
        Token[] strings = contexts.Tokens.Tokens.Where(t => t.Kind != TokenKind.EndOfSource).ToArray();
        Check(expected.SequenceEqual(strings.Select(t => t.Spelling)), PpText(contexts));
        Equal("é", contexts.Macros["A"].RawReplacementSpelling); Equal("\\u00e9", contexts.Macros["A"].ReplacementSpelling);
        Equal("\\u00E9", contexts.Macros["B"].RawReplacementSpelling); Equal("\\u00e9", contexts.Macros["B"].ReplacementSpelling);
        SourceRange rawA = contexts.Macros["A"].Replacement.Single().Range;
        SourceRange rawB = contexts.Macros["B"].Replacement.Single().Range;
        Equal("é", rawA.Spelling); Equal("\\u00E9", rawB.Spelling);
        Check(strings[4].Origin.SpellingRanges.Contains(rawA)); Check(strings[5].Origin.SpellingRanges.Contains(rawB));
        Check(strings[8].Origin.SpellingRanges.Contains(rawA));
        Check(strings[8].Origin.ExpansionHistory.Any(f => f.Name == "A"));
        SourceRange directArgument = contexts.Source.Range(contexts.Source.Text.IndexOf("S(\\u00E9)", StringComparison.Ordinal) + 2, 6);
        Check(strings[1].Origin.SpellingRanges.Contains(directArgument));
        // A formal parameter is a substitution section, not literal identifier
        // replacement text. Its actual argument spelling must remain effective.
        Equal("\"é\" \"\\u00E9\" \"\\u00e9\"", Pp(definitions +
            "#define PARAM(é) \\u00E9\n#define FUNC(x) é\nE(PARAM(é)) E(PARAM(\\u00E9)) E(FUNC(0))"));
        Equal("\"\\u00e9\" \"\\U00010400\"", Pp(definitions +
            "#define SAME é\n#define SAME \\u00E9\n#define SUP 𐐀\nE(SAME) E(SUP)"));
    }
    internal static void UnicodePreprocessingTiming()
    {
        // Supervisor native observations distinguish initial active identifier
        // scanning from skipped groups and deferred character value conversion.
        Equal("int x = 1 ;", Pp("#if 0\n\\uD800 \\u00G0 \\u0041\n#if \\UFFFFFFFF\n#endif\n#endif\nint x=1;"));
        Equal("int x = 1 ;", Pp("#define UNUSED U'\\U00110000'\n#define DROP(x) 1\nint x=DROP(U'\\U00110000');"));
        using var session = new CompilationSession();
        foreach (string source in new[] { "#define DROP(x) 1\nDROP(\\uD800)", "#define STR(x) #x\nSTR(\\uD800)", "#if 1\nyes\n#elif \\uD800\nno\n#endif" })
        {
            PreprocessingResult invalid = session.Preprocess(source);
            Check(!invalid.IsSuccess && invalid.Diagnostics.Any(d => d.Code == "LEX031"));
        }
        // Native UTF8 controls reject malformed scalar UCNs encountered by
        // scan_number/check_for_ud_suffix before DROP or STR can discard them.
        foreach (var (spelling, code) in new[]
        {
            ("1\\uD800", "LEX031"), ("U'a'_\\uD800", "LEX031"), ("\"x\"_\\uD800", "LEX031"),
            ("1\\u12G0", "LEX030"), ("U'a'_\\u12G0", "LEX030"), ("\"x\"_\\u12G0", "LEX030"),
            ("1\\U00110000", "LEX031"), ("U'a'_\\u12", "LEX030"), ("R\"(x)\"_\\uD800", "LEX031")
        })
        {
            foreach (string macro in new[] { "#define DROP(x) 1\nDROP(", "#define STR(x) #x\nSTR(" })
            {
                PreprocessingResult invalid = session.Preprocess(macro + spelling + ")");
                Check(!invalid.IsSuccess && invalid.Diagnostics.Any(d => d.Code == code && d.Class == DiagnosticClass.InvalidSource), spelling);
            }
            Equal("int x = 1 ;", Pp("#if 0\n" + spelling + "\n#endif\nint x=1;"));
        }
        foreach (string spelling in new[] { "1\\uD800", "U'a'_\\uD800", "\"x\"_\\uD800" })
            Check(session.Lex(spelling).Diagnostics.Any(d => d.Code == "LEX031"), spelling);
        // Only the suffix is scanned as identifier text: malformed character
        // contents, string escapes and raw r-chars still have lazy value semantics.
        Equal("1 1 1 1", Pp("#define DROP(x) 1\nDROP(U'\\U00110000'_tag) DROP(\"\\uD800\"_tag) DROP(R\"(\\uD800)\"_tag) DROP(U'a'_tag)"));
        Equal("\"U'a'_tag\"", Pp("#define STR(x) #x\nSTR(U'a'_tag)"));
        Check(!session.Preprocess("\\uD800").IsSuccess);
        Check(!session.Preprocess("#if U'\\U00110000'\nno\n#endif").IsSuccess);
    }
    internal static void UnicodeCharacterSemantics()
    {
        AnalysisResult result = Success("char ascii='\\u0041'; char zero='\\u0000'; char8_t byte=u8'\\xff'; char16_t direct=u'é'; char16_t escaped=u'\\u00e9'; char32_t scalar=U'𐐀'; char32_t ucn=U'\\U00010400'; wchar_t wide=L'𐐀'; char16_t numeric=u'\\xd800';");
        foreach (var (name, value, kind, encoding) in new[]
        {
            ("ascii", 65, IntegerKind.Char, LiteralEncoding.Ordinary), ("zero", 0, IntegerKind.Char, LiteralEncoding.Ordinary),
            ("byte", 255, IntegerKind.Char8, LiteralEncoding.Utf8), ("direct", 233, IntegerKind.Char16, LiteralEncoding.Utf16),
            ("escaped", 233, IntegerKind.Char16, LiteralEncoding.Utf16), ("scalar", 0x10400, IntegerKind.Char32, LiteralEncoding.Utf32),
            ("ucn", 0x10400, IntegerKind.Char32, LiteralEncoding.Utf32), ("wide", 0x10400, IntegerKind.WChar, LiteralEncoding.Wide),
            ("numeric", 0xd800, IntegerKind.Char16, LiteralEncoding.Utf16)
        })
        {
            CharacterLiteralExpression literal = Character(result, name);
            Equal(new BigInteger(value), literal.ConstantValue!.Value); Equal((uint)value, literal.CodeUnits.Single());
            Equal(kind, Kind(literal.Type)); Equal(encoding, literal.Encoding); Check(!literal.IsMulticharacter);
        }
        foreach (string spelling in new[] { "'é'", "'\\u00e9'", "u8'é'", "u8'\\u00e9'", "u'𐐀'", "u'\\U00010400'", "L'ab'", "U'ab'", "u'\\x110000'" })
        {
            AnalysisResult invalid = Analyze("int value=" + spelling + ";");
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures && invalid.Diagnostics.Any(d => d.Code == "LEX020"), spelling);
        }
        foreach (string spelling in new[] { "U'\\uD800'", "U'\\U00110000'", "u'\\u12G0'", "'\\U123'" })
        {
            AnalysisResult invalid = Analyze("int value=" + spelling + ";");
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures && invalid.Diagnostics.Any(d => d.Code is "LEX030" or "LEX031"), spelling);
        }
        foreach (string source in new[] { "int value=U'\\é';", "int value=U'\\N{LATIN CAPITAL LETTER A}';", "int value=u'é'_custom;", "int value=1.5;", "int value=\"é\";" })
            Check(Analyze(source).ContainsUnsupportedFeatures, source);
        Equal("yes", Pp("#if '\\u0041'==65 && u'é'==233 && U'\\U00010400'==66560 && L'𐐀'==66560 && u8'\\xff'==255\nyes\n#endif"));
    }
    internal static void UnicodeSemanticGate()
    {
        UnicodeReferenceStorageGate();
        AnalysisResult redeclared = Success("using é=int; using \\u00E9=int; \\u00e9 value=3; int φ(int); int \\u03c6(int x){return x;} int run(){return φ(value);}");
        Equal(2, redeclared.GlobalScope.FindLocal("é")!.Declarations.Count);
        Equal(2, redeclared.GlobalScope.FindLocal("φ")!.Declarations.Count);
        Same(redeclared.GlobalScope.FindLocal("φ")!, ReturnedCall(redeclared, "run").SelectedFunction);
        Equal("\\u00E9", redeclared.GlobalScope.FindLocal("é")!.Declarations[1].NameRange.Spelling);
        var provider = new InMemoryIncludeProvider(new Dictionary<string, string> { ["m1-unicode-gate.h"] = Fixture("m1-unicode-gate.h") });
        AnalysisResult gate;
        using (var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(includeProvider: provider, sourceInput: new()) }))
            gate = session.Analyze(Fixture("m1-unicode-gate.cpp"), "m1-unicode-gate.cpp");
        Check(gate.IsSuccess, string.Join("; ", gate.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        Equal(4, gate.StaticAssertions.Count); Check(gate.StaticAssertions.All(a => a.Evaluation.Status == ConstantEvaluationStatus.Success));
        FunctionTemplateDeclaration template = Template(gate, "ίδιο");
        Same(template.Symbol, gate.GlobalScope.FindLocal("\\u03afδιο"));
        Equal(2, template.Instances.Count);
        Check(template.Instances.Select(i => Kind(i.TypeArguments.Single())).SequenceEqual(new[] { IntegerKind.Char16, IntegerKind.Char32 }));
        CallExpression call = ReturnedCall(gate, "τρέξε");
        Equal(IntegerKind.Int, SelectedParameter(call)); Equal(StandardConversionRank.Promotion, call.SelectedCandidate.ArgumentConversions[0].Rank);
        Equal(2, gate.PhysicalSources.Count); Equal(1, gate.Preprocessing.Includes.Count);
        Check(template.BodyTokens.Tokens.Any(t => t.Origin.ExpansionNotes.Any(n => n.Contains("included from", StringComparison.Ordinal))));
        Check(template.BodyTokens.Tokens.Any(t => t.Origin.ExpansionHistory.Any(f => f.Name == "επιστροφή")));
        Symbol symbol = gate.GlobalScope.FindLocal("αριθμός")!;
        Same(symbol, gate.GlobalScope.FindLocal("\\u03b1ριθμός"));
        Check(gate.Declarations.OfType<AliasDeclaration>().Any(d => d.Name == "γράμμα"));
        CharacterLiteralExpression retained = Character(gate, "αριθμός");
        Equal((uint)233, retained.CodeUnits.Single());
        Equal(OriginalSourceEncoding.ManagedUtf16, retained.Range.File.OriginForRange(retained.Range).PrimaryPhysicalRange.File.Encoding.OriginalEncoding);
    }
    private sealed class UnicodeMetadataProvider(SourceEncodingInfo encoding) : IStructuredIncludeProvider
    {
        internal int Probes, Reads;
        public IncludeProbeResult Probe(string path) { ++Probes; return IncludeProbeResult.Found(path); }
        public IncludeReadResult Read(string path)
        {
            ++Reads;
            return IncludeReadResult.Found(path, "#pragma once\nconstexpr char32_t κόσμος=U'𐐀';", encoding);
        }
        public bool TryRead(string path, out string? text) => throw new Exception("Structured provider compatibility path must not be called.");
    }
    internal static void UnicodeSourceMetadata()
    {
        var metadata = new SourceEncodingInfo(OriginalSourceEncoding.Utf16BigEndian, hasByteOrderMark: true);
        var provider = new UnicodeMetadataProvider(metadata);
        var input = new SourceInputOptions();
        AnalysisResult result;
        using (var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(includeProvider: provider, sourceInput: input) }))
        {
            Same(input, session.Options.Preprocessing.SourceInput);
            result = session.Analyze("#if __has_include(\"unicode.h\") && __has_include(\"unicode.h\")\n#include \"unicode.h\"\n#include \"unicode.h\"\nstatic_assert(κόσμος==66560);\n#endif", "root.cpp");
        }
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Message)));
        Equal(1, provider.Probes); Equal(1, provider.Reads); Equal(2, result.PhysicalSources.Count);
        SourceFile included = result.PhysicalSources.Single(s => s.Name == "unicode.h");
        Same(metadata, included.Encoding); Equal(OriginalSourceEncoding.Utf16BigEndian, included.Encoding.OriginalEncoding);
        Check(included.Encoding.HasByteOrderMark); Equal(UnicodeSourcePolicy.Unicode, included.Encoding.UnicodePolicy);
        Equal(ExecutionCharacterEncoding.Utf8, included.Encoding.ExecutionEncoding);
        Equal(OriginalSourceEncoding.ManagedUtf16, result.Source.Encoding.OriginalEncoding); Check(!result.Source.Encoding.HasByteOrderMark);
        CharacterLiteralExpression retained = Character(result, "κόσμος");
        TokenOrigin origin = retained.Range.File.OriginForRange(retained.Range);
        Same(included, origin.PrimaryPhysicalRange.File); Equal((uint)0x10400, retained.CodeUnits.Single());
        _ = result.GlobalScope.Lookup("\\u03baόσμος"); _ = origin.ExpansionNotes; _ = result.Preprocessing.Macros.Count;
        Equal(1, provider.Probes); Equal(1, provider.Reads);
        foreach (SourceInputOptions unsupported in new[] { new SourceInputOptions(UnicodeSourcePolicy.LegacyNonUnicode), new SourceInputOptions(UnicodeSourcePolicy.NativeMultibyte), new SourceInputOptions(executionEncoding: ExecutionCharacterEncoding.LegacyNative) })
        {
            using var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(sourceInput: unsupported) });
            Check(session.Lex("int x;").Diagnostics.Any(d => d.Code == "UNS030"));
            AnalysisResult bounded = session.Analyze("int x;");
            Check(!bounded.IsSuccess && bounded.ContainsUnsupportedFeatures && bounded.Declarations.Count == 0);
        }
        var badProvider = new UnicodeMetadataProvider(new SourceEncodingInfo(unicodePolicy: UnicodeSourcePolicy.LegacyNonUnicode));
        using (var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(includeProvider: badProvider) }))
            Check(session.Analyze("#include \"legacy.h\"").ContainsUnsupportedFeatures);
        Check(IncludeReadResult.Missing("missing").Encoding == null);
        Equal(OriginalSourceEncoding.ManagedUtf16, IncludeReadResult.Found("legacy-provider", "int x;").Encoding!.OriginalEncoding);
        Throws<ArgumentException>(() => new SourceEncodingInfo(hasByteOrderMark: true));
        Throws<ArgumentOutOfRangeException>(() => new SourceInputOptions((UnicodeSourcePolicy)100));
        Throws<ArgumentOutOfRangeException>(() => new SourceEncodingInfo((OriginalSourceEncoding)100));
    }
    internal static void UnicodeFileEncodingMetadata()
    {
        using var files = new IncludeTestDirectory();
        FileSystemIncludeProvider provider = files.Provider();
        var cases = new[]
        {
            ("utf8.h", (Encoding)new UTF8Encoding(false, true), OriginalSourceEncoding.Utf8, false),
            ("utf8bom.h", (Encoding)new UTF8Encoding(true, true), OriginalSourceEncoding.Utf8, true),
            ("utf16le.h", (Encoding)new UnicodeEncoding(false, true, true), OriginalSourceEncoding.Utf16LittleEndian, true),
            ("utf16be.h", (Encoding)new UnicodeEncoding(true, true, true), OriginalSourceEncoding.Utf16BigEndian, true)
        };
        foreach (var (name, encoding, original, bom) in cases)
        {
            string path = files.Bytes(name, encoding.GetPreamble().Concat(encoding.GetBytes("constexpr char32_t 値=U'𐐀';")).ToArray());
            IncludeReadResult read = provider.Read(path); _ = IncludeFound(read);
            Equal(original, read.Encoding!.OriginalEncoding); Equal(bom, read.Encoding.HasByteOrderMark);
            AnalysisResult result;
            using (var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(includeProvider: provider) }))
                result = session.Analyze("#include \"" + name + "\"\nstatic_assert(値==66560);", files.PathOf("root.cpp"));
            Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Message)));
            SourceFile included = result.PhysicalSources.Single(s => s.Name == path);
            Equal(original, included.Encoding.OriginalEncoding); Equal(bom, included.Encoding.HasByteOrderMark);
            Equal(ExecutionCharacterEncoding.Utf8, included.Encoding.ExecutionEncoding);
        }
        string malformed = files.Bytes("bad.h", [0xc3, 0x28]);
        IncludeProbeFound(provider.Probe(malformed)); IncludeFailed(provider.Read(malformed), IncludeFailureKind.InvalidEncoding);
        string invalid16 = files.Bytes("bad16.h", [0xff, 0xfe, 0x00, 0xd8]);
        IncludeProbeFound(provider.Probe(invalid16)); IncludeFailed(provider.Read(invalid16), IncludeFailureKind.InvalidEncoding);
    }
    internal static void UnicodeIndependentSessions()
    {
        var source = "using γράμμα=char16_t; template<class Τ> constexpr Τ ίδιο(Τ χ){return χ;} constexpr γράμμα αξία=u'é'; static_assert(\\u03afδιο(αξία)==233);";
        AnalysisResult[] results = new AnalysisResult[4];
        Parallel.For(0, results.Length, index =>
        {
            using var session = new CompilationSession(new AnalysisOptions { Preprocessing = new(sourceInput: new()) });
            results[index] = session.Analyze(source, "independent.cpp");
        });
        foreach (AnalysisResult result in results)
        {
            Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Message)));
            Equal(1, Template(result, "ίδιο").Instances.Count);
        }
        Check(!ReferenceEquals(results[0].GlobalScope.FindLocal("αξία"), results[1].GlobalScope.FindLocal("αξία")));
        Check(!ReferenceEquals(Template(results[0], "ίδιο").Instances.Single().TypeArguments[0], Template(results[1], "ίδιο").Instances.Single().TypeArguments[0]));
    }
}
