// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored target-character/predefined/include-probe integration controls.
using System.Numerics;
using EdgSharp;

internal static partial class Tests
{
    private static CharacterLiteralExpression Character(AnalysisResult result, string name)
    {
        Expression expression = Variable(result, name).Initializer!;
        while (expression is ConversionExpression conversion) expression = conversion.Operand;
        return As<CharacterLiteralExpression>(expression);
    }
    internal static void CharacterValues()
    {
        AnalysisResult result = Success("char a='a'; char b='\\n'; char c='\\101'; char d='\\x41'; char negative='\\xff'; int multi='ab'; wchar_t wide=L'A'; char8_t utf8=u8'A'; char16_t utf16=u'A'; char32_t utf32=U'A';");
        foreach (string name in new[] { "a", "c", "d" }) Equal(new BigInteger(name == "a" ? 97 : 65), Character(result, name).ConstantValue!.Value);
        Equal(new BigInteger(10), Character(result, "b").ConstantValue!.Value);
        Equal(new BigInteger(-1), Character(result, "negative").ConstantValue!.Value);
        Equal(new BigInteger(0x6162), Character(result, "multi").ConstantValue!.Value);
        Equal(IntegerKind.Int, Kind(Character(result, "multi").Type)); Check(Character(result, "multi").IsMulticharacter);
        foreach (var (name, kind, encoding) in new[] { ("wide", IntegerKind.WChar, LiteralEncoding.Wide), ("utf8", IntegerKind.Char8, LiteralEncoding.Utf8), ("utf16", IntegerKind.Char16, LiteralEncoding.Utf16), ("utf32", IntegerKind.Char32, LiteralEncoding.Utf32) })
        {
            CharacterLiteralExpression character = Character(result, name);
            Equal(kind, Kind(character.Type)); Same(Variable(result, name).Type, character.Type);
            Equal(encoding, character.Encoding); Equal((uint)65, character.CodeUnits.Single());
            Equal(new BigInteger(65), character.ConstantValue!.Value);
        }
        Check(Success("static_assert('a'==97); static_assert('\\xff'==-1); static_assert('ab'==24930); static_assert(u'\\xffff'==65535); static_assert(U'\\xffffffff'==4294967295U);").StaticAssertions.All(a => a.Evaluation.Status == ConstantEvaluationStatus.Success));
        AnalysisResult packing = Success("int first='\\377a'; int last='a\\377'; int both='\\377\\377';");
        Equal(new BigInteger(-159), Character(packing, "first").ConstantValue!.Value);
        Equal(new BigInteger(25087), Character(packing, "last").ConstantValue!.Value);
        Equal(new BigInteger(-1), Character(packing, "both").ConstantValue!.Value);
    }
    internal static void CharacterConversions()
    {
        AnalysisResult result = Success("int pick(int); int pick(long); int one(){return pick(u'a');} int two(){return pick(L'a');} int three(){return pick(u8'a');} int exact(char32_t); int exact(unsigned int); int four(){return exact(U'a');} int promote(unsigned int); int promote(long); int five(){return promote(U'a');} template<class T> constexpr T id(T x){return x;} static_assert(id('a')==97); static_assert(id(u'a')==97); static_assert(id(U'a')==97);");
        foreach (string name in new[] { "one", "two", "three" })
        { Equal(IntegerKind.Int, SelectedParameter(ReturnedCall(result, name))); Equal(StandardConversionRank.Promotion, ReturnedCall(result, name).SelectedCandidate.ArgumentConversions[0].Rank); }
        Equal(IntegerKind.Char32, SelectedParameter(ReturnedCall(result, "four")));
        Equal(IntegerKind.UnsignedInt, SelectedParameter(ReturnedCall(result, "five")));
        Equal(StandardConversionRank.Promotion, ReturnedCall(result, "five").SelectedCandidate.ArgumentConversions[0].Rank);
        Check(Template(result, "id").Instances.Select(i => Kind(i.TypeArguments[0])).SequenceEqual(new[] { IntegerKind.Char, IntegerKind.Char16, IntegerKind.Char32 }));
        foreach (string invalid in new[] { "unsigned wchar_t x;", "long char16_t x;", "char8_t int x;", "char32_t char32_t x;" }) Check(!Analyze(invalid).IsSuccess);
    }
    internal static void CharacterDiagnostics()
    {
        foreach (string spelling in new[] { "''", "'\\x'", "'\\x100'", "'\\400'", "'abcde'", "u8'ab'", "u'ab'", "U'ab'", "L'ab'" })
        {
            AnalysisResult invalid = Analyze("int value=" + spelling + ";");
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures && invalid.Diagnostics.Any(d => d.Code == "LEX020"), spelling);
        }
        AnalysisResult warning = Success("char value='\\q'; int multi='ab';");
        Equal(new BigInteger(113), Character(warning, "value").ConstantValue!.Value);
        Check(warning.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning) == 2);
        Equal(new BigInteger(65), Character(Success("char value='\\u0041';"), "value").ConstantValue!.Value);
        AnalysisResult multibyte = Analyze("char value='é';");
        Check(!multibyte.IsSuccess && !multibyte.ContainsUnsupportedFeatures && multibyte.Diagnostics.Any(d => d.Code == "LEX020"));
        foreach (string source in new[] { "int value='a'_custom;", "char32_t value=U'\\é';" })
        { AnalysisResult bounded = Analyze(source); Check(!bounded.IsSuccess && bounded.ContainsUnsupportedFeatures, source); }
        Equal("'a'_tag", Pp("#define _tag 7\n#define E(x) x\nE('a'_tag)"));
        AnalysisResult template = Success("template<class T> constexpr int warning(T x){return '\\q'+x;} static_assert(warning(1)==114);");
        Check(Template(template, "warning").IsValid);
        Equal(TemplateInstanceState.Complete, Template(template, "warning").Instances.Single().State);
    }
    internal static void CharacterPreprocessing()
    {
        Equal("yes", Pp("#if 'a'==97 && '\\n'==10 && '\\xff'==-1 && 'ab'==24930 && u'\\xffff'==65535 && U'\\xffffffff'==4294967295U\nyes\n#endif"));
        Equal("yes", Pp("#if 0 && 1/0\nno\n#elif (u8'A'==65) && (L'A'==65)\nyes\n#endif"));
        Equal("yes", Pp("#if u8'a'>-1 && u'a'>-1 && U'a'>-1 && (U'\\xffffffff'+U'\\1')==0\nyes\n#endif"));
        Equal("yes", Pp("#if '\\377a'==-159 && 'a\\377'==25087 && '\\377\\377'==-1\nyes\n#endif"));
        using var session = new CompilationSession();
        Check(!session.Preprocess("#if '\\x100'\nno\n#endif").IsSuccess);
        Check(!session.Preprocess("#if 'a'<<32\nno\n#endif").IsSuccess);
        Check(!session.Preprocess("#if true<<32\nno\n#endif").IsSuccess);
        Check(!session.Preprocess("#if (1==1)<<32\nno\n#endif").IsSuccess);
        Equal("int value = 1 ;", Pp("#define UNUSED '\\x100'\n#if 0\n'\\x100'\n#endif\nint value=1;"));
        var includeOptions = new PreprocessingOptions(new InMemoryIncludeProvider(new Dictionary<string, string> { ["m1-character-gate.h"] = Fixture("m1-character-gate.h") }));
        AnalysisResult gate;
        using (var configured = new CompilationSession(new AnalysisOptions { Preprocessing = includeOptions }))
            gate = configured.Analyze(Fixture("m1-character-gate.cpp"), "m1-character-gate.cpp");
        Check(gate.IsSuccess, string.Join("; ", gate.Diagnostics.Select(d => d.Message)));
        Equal(3, gate.StaticAssertions.Count); Equal(2, Template(gate, "id").Instances.Count);
        Check(Template(gate, "id").Instances.Select(instance => Kind(instance.TypeArguments[0])).SequenceEqual(new[] { IntegerKind.Char, IntegerKind.Char16 }));
        CallExpression selected = ReturnedCall(gate, "run");
        Equal(IntegerKind.Int, SelectedParameter(selected));
        Equal(StandardConversionRank.Promotion, selected.SelectedCandidate.ArgumentConversions[0].Rank);
        Expression run = As<BinaryExpression>(gate.StaticAssertions[0].Expression).Left;
        ConstantEvaluationResult evaluated = new ConstantInterpreter(new AnalysisOptions()).Evaluate(run);
        Equal(ConstantEvaluationStatus.Success, evaluated.Status); Equal(new BigInteger(66), evaluated.Value!.Value);
        Check(Template(gate, "id").BodyTokens.Tokens.Any(t => t.Origin.ExpansionHistory.Any(f => f.Name == "RETURN")));
        Equal(2, gate.PhysicalSources.Count); Equal(1, gate.Preprocessing.Includes.Count);
        Check(Template(gate, "id").BodyTokens.Tokens.Any(t => t.Origin.ExpansionNotes.Any(n => n.Contains("included from", StringComparison.Ordinal))));
    }
    internal static void StandardPredefines()
    {
        Equal("\"Jan  1 1970\" \"00:00:00\" 1 202002L", PpText(Preprocessed("__DATE__ __TIME__ __STDC_HOSTED__ __cplusplus", new(translationTimestamp: DateTimeOffset.UnixEpoch))));
        DateTimeOffset before = DateTimeOffset.UtcNow;
        using (var current = new CompilationSession())
        {
            DateTimeOffset captured = current.Options.Preprocessing.TranslationTimestamp!.Value;
            Check(captured >= before && captured <= DateTimeOffset.UtcNow);
            Equal(PpText(current.Preprocess("__DATE__ __TIME__")), PpText(current.Preprocess("__DATE__ __TIME__")));
            Equal(captured, current.Preprocess("").TranslationTimestamp);
            Equal(captured, current.Options.Preprocessing.TranslationTimestamp.Value);
        }
        var options = new PreprocessingOptions(translationTimestamp: new DateTimeOffset(2024, 2, 9, 2, 3, 4, TimeSpan.FromHours(2)), hosted: false);
        Equal("\"Feb  9 2024\" \"00:03:04\" 0", PpText(Preprocessed("__DATE__ __TIME__ __STDC_HOSTED__", options)));
        Equal("yes", PpText(Preprocessed("#if defined(__DATE__) && defined(__TIME__) && defined(__STDC_HOSTED__) && __STDC_HOSTED__==0\nyes\n#endif", options)));
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options });
        foreach (string name in new[] { "__DATE__", "__TIME__", "__STDC_HOSTED__", "__has_include" })
        {
            Check(!session.Preprocess("#define " + name + " 9\n").IsSuccess);
            Check(!session.Preprocess("#undef " + name + "\n").IsSuccess);
            Throws<ArgumentException>(() => new PreprocessingOptions(predefinedMacros: new Dictionary<string, string> { [name] = "9" }));
        }
        PreprocessingResult origin = Preprocessed("#define DATE __DATE__\nDATE", options);
        Check(origin.Tokens.Tokens[0].Origin.ExpansionHistory.Any(f => f.Name == "DATE"));
        Task<string>[] independent = Enumerable.Range(1, 4).Select(day => Task.Run(() => PpText(Preprocessed("__DATE__", new(translationTimestamp: new DateTimeOffset(2000, 3, day, 0, 0, 0, TimeSpan.Zero)))))).ToArray();
        Task.WaitAll(independent);
        for (int index = 0; index < independent.Length; ++index) Equal($"\"Mar  {index + 1} 2000\"", independent[index].Result);
    }
    private sealed class ObservedIncludes(Dictionary<string, string> files) : IIncludeProvider
    {
        internal List<string> Reads { get; } = [];
        public bool TryRead(string path, out string? text) { Reads.Add(path); return files.TryGetValue(path, out text); }
    }
    internal static void IncludeProbes()
    {
        var provider = new ObservedIncludes(new Dictionary<string, string>
        {
            ["/physical/h.h"] = "#pragma once\n#define ACTIVE 7\n#line 90 \"header.cpp\"\nint headerValue=ACTIVE;\n",
            ["/quote/h.h"] = "quote\n", ["/system/h.h"] = "system\n", ["/h.h"] = "root\n",
            ["/system/outer/*comment*/inner.h"] = "literal\n", ["/system/macro name.h"] = "macro\n",
            ["/system/outer/*unterminated.h"] = "opaque\n", ["/system/outer/slash.h"] = "slashes\n"
        });
        var options = new PreprocessingOptions(provider, ["/quote"], ["/system"]);
        PreprocessingResult probe = Preprocessed("#line 10 \"renamed/other.cpp\"\n#if defined(__has_include) && __has_include(\"h.h\") && __has_include(<h.h>) && !__has_include(\"missing.h\")\nyes\n#endif\n#ifdef ACTIVE\nwrong\n#endif\n__LINE__", options, "/physical/root.cpp");
        Equal("yes 16", PpText(probe)); Equal(1, probe.PhysicalSources.Count); Equal(0, probe.Includes.Count);
        Check(provider.Reads.Take(2).SequenceEqual(new[] { "/physical/h.h", "/system/h.h" }));
        PreprocessingResult real = Preprocessed("#if __has_include(\"h.h\") && __has_include(\"h.h\")\n#include \"h.h\"\n#endif\nACTIVE __LINE__", options, "/physical/root.cpp");
        Equal("int headerValue = 7 ; 7 4", PpText(real)); Equal(1, real.Includes.Count); Equal(2, real.PhysicalSources.Count);
        Equal(2, provider.Reads.Count(path => path == "/physical/h.h")); // One read per separate analysis, repeated probe/include reuses content.
        Equal("yes", PpText(Preprocessed("#define Q \"h.h\"\n#define A <h.h>\n#if __has_include(Q) && __has_include(A)\nyes\n#endif", options, "/physical/root.cpp")));
        Equal("yes", PpText(Preprocessed("#define HI __has_include\n#define h wrong\n#if HI(<h.h>)\nyes\n#endif", options, "/physical/root.cpp")));
        Equal("", PpText(Preprocessed("#define HI __has_include\n#define H <h.h>\n#define h wrong\n#if HI(H)\nyes\n#endif", options, "/physical/root.cpp")));
        Equal("yes literal", PpText(Preprocessed("#define HI __has_include\n#if __has_include(<outer/*comment*/inner.h>) && HI(<outer/*comment*/inner.h>)\nyes\n#include <outer/*comment*/inner.h>\n#endif", options, "/physical/root.cpp")));
        Equal("macro", PpText(Preprocessed("#define P __has_include(<macro/*comment*/name.h>)\n#if P\nmacro\n#endif", options, "/physical/root.cpp")));
        Equal("yes opaque slashes", PpText(Preprocessed("#define HI __has_include\n#if HI(<outer/*unterminated.h>) && HI(<outer//slash.h>)\nyes\n#include <outer/*unterminated.h>\n#include <outer//slash.h>\n#endif", options, "/physical/root.cpp")));
        Equal("yes", PpText(Preprocessed("#define CAT(a,b) a##b\n#if CAT(__has_,include)(<outer//slash.h>)\nyes\n#endif", options, "/physical/root.cpp")));
        Equal("\"<outer inner.h>\"", Pp("#define S(x) #x\nS(<outer/*comment*/inner.h>)"));
        string rawSpelling = "R\"(one\\\ntwo)\"";
        PreprocessingResult afterHeader = Preprocessed("#define HI __has_include\n#if HI(<outer/*unterminated.h>)\nyes\n#endif\n" + rawSpelling, options, "/physical/root.cpp");
        Equal("yes " + rawSpelling, PpText(afterHeader));
        Token raw = afterHeader.Tokens.Tokens.Single(token => token.Kind == TokenKind.StringLiteral);
        Check(raw.Origin.SpellingRanges.Any(range => range.Spelling.Contains("\\\n", StringComparison.Ordinal)));
        Same(afterHeader.Tokens.Source, raw.Range.File);
        Equal("yes", PpText(Preprocessed("#if __has_include(\"h.h\")\nyes\n#endif", options, "/root.cpp")));
        int reads = provider.Reads.Count;
        Equal("yes", PpText(Preprocessed("#if 0\n#if __has_include(\"never.h\")\nwrong\n#endif\n#endif\nyes", options, "/physical/root.cpp")));
        Equal(reads, provider.Reads.Count);
        Equal("yes", PpText(Preprocessed("#if 1 || __has_include(\"missing.h\")\nyes\n#endif", options, "/physical/root.cpp")));
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options });
        foreach (string source in new[] { "__has_include(\"h.h\")", "#if __has_include(\"\")\nx\n#endif", "#if __has_include()\nx\n#endif", "#if __has_include(\"h.h\"\nx\n#endif", "#define HI __has_include\n#if HI(<a>b>)\nx\n#endif" }) Check(!session.Preprocess(source).IsSuccess, source);
        Check(!session.Preprocess("#define HI __has_include\n#if HI(<outer/*comment\n*/inner.h>)\n#endif", "/physical/root.cpp").IsSuccess);
        Check(!session.Preprocess("ordinary /*unterminated").IsSuccess);
        Equal("one two", Pp("one // ordinary comment\ntwo"));
        TokenBuffer lexedRaw = session.Lex(rawSpelling).Tokens;
        Equal(rawSpelling, lexedRaw.Tokens[0].Spelling); Same(lexedRaw.Source, lexedRaw.Tokens[0].Range.File);
        Equal(rawSpelling, lexedRaw.Retain(0, 1).Tokens[0].Spelling);
    }
    private sealed class StructuredInputs : IStructuredIncludeProvider
    {
        internal Dictionary<string, IncludeProbeResult> Probes { get; } = [];
        internal Dictionary<string, IncludeReadResult> Contents { get; } = [];
        internal List<string> ProbeCalls { get; } = [];
        internal List<string> ReadCalls { get; } = [];
        public IncludeProbeResult Probe(string path) { ProbeCalls.Add(path); return Probes.TryGetValue(path, out var result) ? result : IncludeProbeResult.Missing(path); }
        public IncludeReadResult Read(string path) { ReadCalls.Add(path); return Contents.TryGetValue(path, out var result) ? result : IncludeReadResult.Missing(path); }
        public bool TryRead(string path, out string? text) => throw new InvalidOperationException("Structured input must use separate probe/read entry points.");
    }
    internal static void StructuredIncludes()
    {
        var provider = new StructuredInputs();
        provider.Probes["/p/ok.h"] = IncludeProbeResult.Found("/canonical/ok.h");
        provider.Contents["/p/ok.h"] = IncludeReadResult.Found("/canonical/ok.h", "#pragma once\n#define ACTIVE 7\nint value=ACTIVE;\n");
        provider.Contents["/alias/ok.h"] = provider.Contents["/p/ok.h"];
        var options = new PreprocessingOptions(provider, ["/q"], ["/s"]);
        PreprocessingResult result = Preprocessed("#if __has_include(\"ok.h\") && __has_include(\"ok.h\")\nyes\n#endif\n#ifdef ACTIVE\nwrong\n#endif\n#include \"ok.h\"\n#include \"/alias/ok.h\"\n#include \"/canonical/ok.h\"\nACTIVE", options, "/p/main.cpp");
        Equal("yes int value = 7 ; 7", PpText(result));
        Check(provider.ProbeCalls.SequenceEqual(new[] { "/p/ok.h" }));
        Check(provider.ReadCalls.SequenceEqual(new[] { "/p/ok.h", "/alias/ok.h" }));
        Equal(2, result.PhysicalSources.Count); Equal("/canonical/ok.h", result.PhysicalSources[1].Name); Equal(1, result.Includes.Count);
        provider.Probes["/p/fail.h"] = IncludeProbeResult.Failed("/p/fail.h", new(IncludeFailureKind.Access, "denied"));
        provider.Contents["/p/bad.h"] = IncludeReadResult.Failed("/p/bad.h", new(IncludeFailureKind.InvalidEncoding, "malformed UTF input"));
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options });
        PreprocessingResult failed = session.Preprocess("#if __has_include(\"fail.h\")\nx\n#endif\n#if __has_include(\"fail.h\")\nx\n#endif", "/p/main.cpp");
        Check(!failed.IsSuccess && failed.Diagnostics.All(d => d.Class == DiagnosticClass.InputFailure));
        Equal(1, provider.ProbeCalls.Count(path => path == "/p/fail.h"));
        Check(!provider.ProbeCalls.Contains("/q/fail.h") && !provider.ProbeCalls.Contains("/s/fail.h"));
        PreprocessingResult invalid = session.Preprocess("#include \"bad.h\"", "/p/main.cpp");
        Check(!invalid.IsSuccess && invalid.Diagnostics.Single().Class == DiagnosticClass.InvalidSource);
        Check(!invalid.Diagnostics.Any(d => d.Code == "PP028"));
        Check(!provider.ReadCalls.Contains("/q/bad.h"));
        PreprocessingResult missing = session.Preprocess("#if __has_include(\"missing.h\") || __has_include(\"missing.h\")\nx\n#endif", "/p/main.cpp");
        Check(missing.IsSuccess); Equal(0, missing.Includes.Count); Equal(1, missing.PhysicalSources.Count);
        foreach (string path in new[] { "/p/missing.h", "/q/missing.h", "/s/missing.h" }) Equal(1, provider.ProbeCalls.Count(call => call == path));
        Check(session.Preprocess("#if __has_include(\"missing.h\")\nx\n#endif", "/p/main.cpp").IsSuccess);
        Equal(2, provider.ProbeCalls.Count(call => call == "/p/missing.h"));
    }
}
