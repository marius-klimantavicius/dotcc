// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored deterministic tests; no generated fixtures or native oracle dependency.
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using EdgSharp;

if (args.Length == 2 && args[0] == "--filesystem-special-child")
{
    Tests.FileSystemSpecialFileChild(args[1]);
    return 0;
}
if (args.Length == 2 && args[0] == "--analyze")
{
    using var session = new CompilationSession();
    AnalysisResult result = session.Analyze(File.ReadAllText(args[1]), args[1]);
    Console.WriteLine($"success={result.IsSuccess}; unsupported={result.ContainsUnsupportedFeatures}; declarations={result.Declarations.Count}");
    foreach (Declaration declaration in result.Declarations)
        Console.WriteLine($"declaration={declaration.Name}; type={declaration.Type}; range={declaration.Range.Start}:{declaration.Range.Length}");
    foreach (Diagnostic diagnostic in result.Diagnostics)
    {
        PresumedPosition location = diagnostic.PrimaryLocation;
        Console.WriteLine($"{diagnostic.Class} {diagnostic.Code} {location.FileName}:{location.Line}:{location.Column} {diagnostic.Message}");
        foreach (string note in diagnostic.ExpansionNotes) Console.WriteLine("note: " + note);
    }
    return result.IsSuccess ? 0 : 1;
}
if (args.Length != 0) { Console.Error.WriteLine("Usage: EdgSharp.Tests [--analyze file.cpp]"); return 2; }

var cases = new (string Name, Action Run)[]
{
    ("lex token spelling and source ranges", Tests.LexicalRanges),
    ("lex maximal punctuation and digraph exception", Tests.Punctuation),
    ("lex malformed comments and strings", Tests.MalformedLexicalInput),
    ("lex cursor checkpoint and retained replay", Tests.TokenReplay),
    ("M1 macro rescan inertness lazy arguments and authoritative boundaries", Tests.PreprocessingMacros),
    ("M1 variadic stringize paste and definition validation", Tests.PreprocessingVariadics),
    ("M1 controlling expressions use C++20 target arithmetic and short circuit", Tests.PreprocessingConditionals),
    ("M1 configured includes retain physical search and conditional boundaries", Tests.PreprocessingIncludes),
    ("M1 source maps retain spelling expansion and logical location", Tests.PreprocessingOrigins),
    ("M1 phase ordering raw literals and directive comments", Tests.PreprocessingPhases),
    ("M1 preprocessing integrates templates constexpr and retained graphs", Tests.PreprocessingSemantics),
    ("M1 preprocessing options limits and session state isolation", Tests.PreprocessingState),
    ("M1 retained upstream VAOPT preprocessing differential", Tests.PreprocessingUpstream),
    ("M1 origins retain paste stringize and repeated include activations", Tests.PreprocessingContributors),
    ("M1 independent preprocessing sessions run concurrently", Tests.PreprocessingConcurrency),
    ("M1 changed token stream preserves difficult M4 behavior", Tests.PreprocessingDifficultGate),
    ("M1 character literal graph uses target values and distinct types", Tests.CharacterValues),
    ("M1 character promotion overload and template identity", Tests.CharacterConversions),
    ("M1 character malformed unsupported and warning controls", Tests.CharacterDiagnostics),
    ("M1 character preprocessing operands and semantic gate", Tests.CharacterPreprocessing),
    ("M1 Unicode token identity preserves raw spelling and scalar offsets", Tests.UnicodeIdentifierTokens),
    ("M1 pinned Unicode17 XID start continue and exclusion boundaries", Tests.Unicode17Boundaries),
    ("M1 Unicode malformed UCN scalar and identifier restrictions", Tests.UnicodeIdentifierDiagnostics),
    ("M1 Unicode macro identity suppression parameters defined stringize paste", Tests.UnicodeMacroIdentity),
    ("M1 Unicode preprocessing inactive and unused diagnostic timing proposals", Tests.UnicodePreprocessingTiming),
    ("M1 Unicode character scalar encoding target units and strict count errors", Tests.UnicodeCharacterSemantics),
    ("M1 Unicode included aliases templates overload constexpr graph gate", Tests.UnicodeSemanticGate),
    ("M1 Unicode source metadata policy provider caches and retained queries", Tests.UnicodeSourceMetadata),
    ("M1 Unicode filesystem BOM encoding provenance and invalid bytes", Tests.UnicodeFileEncodingMetadata),
    ("M1 Unicode independent sessions retain separate identities and instances", Tests.UnicodeIndependentSessions),
    ("M1 string literal array graph preserves typed units raw spelling and null", Tests.StringGraph),
    ("M1 adjacent strings decode rescan and widen without repeated escapes", Tests.StringConcatenation),
    ("M1 sizeof suppresses evaluation decay and unneeded template bodies", Tests.StringSizeOf),
    ("M1 string array initialization infers bounds pads and retains storage", Tests.StringArrayVariables),
    ("M1 string pointer transport distinguishes one past from element loads", Tests.StringPointerStorage),
    ("M1 string element references and pointer overloads retain identities", Tests.StringElementReferences),
    ("M1 array inference rollback scoped cache ownership and resource limits", Tests.StringOwnershipAndRollback),
    ("M1 Unicode macro string template lookup and reference storage gate", Tests.StringStorageCombined),
    ("M1 standard predefined values are configured and deterministic", Tests.StandardPredefines),
    ("M1 include existence probes share search without activation", Tests.IncludeProbes),
    ("M1 structured include probe read failure and cache contracts", Tests.StructuredIncludes),
    ("M1 filesystem provider has explicit immutable configuration", Tests.FileSystemConfiguration),
    ("M1 filesystem requested paths and directory fallback", Tests.FileSystemRequestedPaths),
    ("M1 filesystem decoding recognizes explicit supported encodings", Tests.FileSystemEncodings),
    ("M1 filesystem failures remain distinct from missing files", Tests.FileSystemFailures),
    ("M1 filesystem inaccessible inputs have typed failures", Tests.FileSystemAccessFailures),
    ("M1 filesystem input limits bound bytes and decoded text", Tests.FileSystemLimits),
    ("M1 filesystem provider supports independent concurrent reads", Tests.FileSystemConcurrency),
    ("M1 filesystem session includes follow configured search", Tests.FileSystemSessionIncludes),
    ("M1 filesystem session probes do not activate inputs", Tests.FileSystemSessionProbes),
    ("M1 filesystem session diagnostics preserve input failure classes", Tests.FileSystemSessionDiagnostics),
    ("M1 filesystem session caches and retained queries isolate IO", Tests.FileSystemSessionCachingQueries),
    ("M1 filesystem special-file inputs finish in a bounded subprocess", Tests.FileSystemSpecialFilesBounded),
    ("semantic checkpoint restores all mutable state", Tests.SemanticCheckpoint),
    ("semantic checkpoint restores nested definitions and current functions", Tests.DefinitionCheckpoint),
    ("semantic checkpoint invalidates stale branches atomically", Tests.StaleCheckpoint),
    ("session independence lifecycle and canonical identity", Tests.Sessions),
    ("independent sessions support concurrent analysis", Tests.ConcurrentSessions),
    ("API rejects unsupported profile and null target", Tests.Options),
    ("public graph collections cannot mutate state", Tests.ReadOnlyGraph),
    ("M2 positive source fixture resolves complete graph", Tests.PositiveGraph),
    ("M2 negative fixtures and scalar branch implementation", Tests.FixtureBoundaries),
    ("alias redeclaration and hiding", Tests.Aliases),
    ("source ordered declaration insertion and lookup", Tests.SourceOrder),
    ("function identity and const parameter boundary", Tests.FunctionIdentity),
    ("M3 ordered overload bindings preserve entity identity", Tests.OverloadBindings),
    ("M3 scalar exact promotion and conversion selection", Tests.OverloadRanking),
    ("M3 indistinguishable and crossed conversions are ambiguous", Tests.OverloadAmbiguity),
    ("M3 declaration order does not rank candidates", Tests.OverloadDeclarationOrder),
    ("M3 candidate viability checks arity and argument types", Tests.OverloadViability),
    ("M3 calls snapshot source-visible overload candidates", Tests.OverloadSourceOrder),
    ("M3 nearest nonfunction bindings hide overloads", Tests.OverloadHiding),
    ("M3 overload binding insertion rolls back in order", Tests.OverloadCheckpoint),
    ("M3 source fixtures inspect selected callees and diagnostics", Tests.OverloadFixtures),
    ("M3 reference declarations preserve canonical entity identity", Tests.ReferenceIdentity),
    ("M3 scalar reference binding retains referents without loads", Tests.ReferenceBindings),
    ("M3 reference temporaries expose conversion and lifetime facts", Tests.ReferenceTemporaries),
    ("M3 invalid reference binding is separate from unsupported input", Tests.ReferenceInvalid),
    ("M3 lvalue and rvalue reference overloads select exact entities", Tests.ReferenceOverloads),
    ("M3 reference rank precedes cv and reference-kind tie breakers", Tests.ReferenceRanking),
    ("M3 crossed and by-value reference conversions remain ambiguous", Tests.ReferenceAmbiguity),
    ("M3 reference overload selection is independent of candidate order", Tests.ReferenceCandidateOrder),
    ("M3 reference calls preserve frozen lookup and lexical hiding", Tests.ReferenceLookup),
    ("M3 reference-returning calls expose lvalue and xvalue categories", Tests.ReferenceReturns),
    ("M3 lifetime hazards remain accepted with bounded facts and warnings", Tests.ReferenceLifetimeHazards),
    ("M3 reference graphs and candidates restore through owner replay", Tests.ReferenceRollback),
    ("M3 compound reference signatures release old translation units", Tests.ReferenceOwnership),
    ("M3 dependent references and compound declarator bounds stay unsupported", Tests.ReferenceBoundaries),
    ("M3 reference fixtures inspect selected types and categories", Tests.ReferenceFixtures),
    ("M3 character references integrate target promotion replay and ownership", Tests.CharacterReferences),
    ("M4 template dependent graph cache replay and instance reuse", Tests.TemplateGraph),
    ("M4 by-value deduction consistency and failed candidate viability", Tests.TemplateDeduction),
    ("M4 template ordinary and direct-pattern overload ordering", Tests.TemplateOverloads),
    ("M4 combined replay lookup overload and constexpr source gate", Tests.CombinedTemplateGate),
    ("M4 unused templates diagnose nondependent errors", Tests.TemplatePrototypeErrors),
    ("M4 static assertions defer template false conditions until instantiation", Tests.TemplateAssertions),
    ("M4 type and value dependency remain distinct", Tests.TemplateValueDependency),
    ("M4 constexpr interpreter executes calls locals and early returns", Tests.ConstexprEvaluation),
    ("M4 constant-required invalid operations have explicit outcomes", Tests.ConstexprFailures),
    ("M4 constant evaluation and instantiation limits are explicit", Tests.EvaluationLimits),
    ("M4 reference evaluation distinguishes locations binding and loads", Tests.ConstantReferenceStorage),
    ("M4 reference argument transport preserves frame and temporary identity", Tests.ConstantReferenceArguments),
    ("M4 required reference reads validate storage lifetimes", Tests.ConstantReferenceLifetimes),
    ("M4 optional initializer replay preserves original selected entities", Tests.ConstantReferenceOptionalReplay),
    ("M4 logical operands have contextual bool and short circuit evaluation", Tests.ConstantReferenceLogical),
    ("M4 storage execution cleans up failed and repeated evaluations", Tests.ConstantReferenceCleanup),
    ("M4 reference storage integrates macro characters template lookup and calls", Tests.ConstantReferenceCombined),
    ("M4 scalar assignment graph preserves lvalues and explicit loads", Tests.ScalarAssignmentGraph),
    ("M4 scalar compound assignment uses target arithmetic and conversion", Tests.ScalarAssignmentConversions),
    ("M4 scalar stores mutate local value parameters and aliases", Tests.ScalarAssignmentCells),
    ("M4 required scalar stores reject external unknown expired and unread cells", Tests.ScalarAssignmentFailures),
    ("M4 scalar stores evaluate RHS before the single destination", Tests.ScalarAssignmentOrder),
    ("M4 scalar sequencing distinguishes unsequenced accesses and function calls", Tests.ScalarAssignmentSequencing),
    ("M4 if branches are checked and only selected statements execute", Tests.ControlIfSelection),
    ("M4 if substatements retain lexical scopes and cleanup", Tests.ControlIfScopes),
    ("M4 while executes conditions mutations and fresh iteration storage", Tests.ControlWhile),
    ("M4 nested break continue resolve owners and unwind crossed blocks", Tests.ControlTransfers),
    ("M4 return unwinds loop block and full expression owners", Tests.ControlReturnCleanup),
    ("M4 do and ordinary for execute increments and cleanup", Tests.ControlDoFor),
    ("M4 invalid transfers and unimplemented control branches stay explicit", Tests.ControlBoundaries),
    ("M4 loop work and recursive calls obey limits and recover", Tests.ControlLimits),
    ("M4 repeated control analyses retain snapshots and release live state", Tests.ControlLifetime),
    ("M4 control flow couples macro template overload reference and storage", Tests.ControlCombined),
    ("M4 repeated template analyses isolate and release source graphs", Tests.TemplateLifetime),
    ("M4 owner checkpoint rolls back replay and instance mutations", Tests.TemplateRollback),
    ("M4 excluded template forms diagnose unsupported features", Tests.TemplateBoundaries),
    ("nested scopes permit local shadowing", Tests.Shadowing),
    ("LP64 integer literal type selection", Tests.LiteralTypes),
    ("explicit integer conversion chains", Tests.Conversions),
    ("usual arithmetic conversion rank and signedness", Tests.ArithmeticConversions),
    ("expression precedence associativity and value category", Tests.Precedence),
    ("target width constants wrap unsigned and withhold undefined folds", Tests.TargetConstants),
    ("void return expressions and call argument errors", Tests.ReturnsAndCalls),
    ("malformed source recovery retains following declarations", Tests.Recovery),
    ("unsupported constructs never report successful analysis", Tests.UnsupportedBoundaries),
    ("invalid scalar declarations remain errors", Tests.InvalidDeclarations)
};
int failures = 0;
foreach (var test in cases)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception error) { ++failures; Console.WriteLine($"FAIL {test.Name}: {error}"); }
}
Console.WriteLine($"Tests: {cases.Length}; passed: {cases.Length - failures}; failed: {failures}; skipped: 0");
return failures == 0 ? 0 : 1;

internal static partial class Tests
{
    private static void Check(bool condition, string message = "Assertion failed") { if (!condition) throw new Exception(message); }
    private static T As<T>(object? value) where T : class => value as T ?? throw new Exception($"Expected {typeof(T).Name}; got {value?.GetType().Name ?? "null"}.");
    private static void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected '{expected}', got '{actual}'."); }
    private static void Same(object expected, object? actual) => Check(ReferenceEquals(expected, actual), "Expected reference identity.");
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}.");
    }
    private static AnalysisResult Analyze(string text)
    {
        using var session = new CompilationSession();
        return session.Analyze(text);
    }
    private static AnalysisResult Success(string text)
    {
        AnalysisResult result = Analyze(text);
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => $"{d.Code}: {d.Message} at {d.Range.Start}")));
        return result;
    }
    private static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", name));
    private static Expression ReturnValue(FunctionDeclaration function) => As<ReturnStatement>(function.Body!.Statements.Last()).Expression!;
    private static FunctionDeclaration Function(AnalysisResult result, string name) => As<FunctionDeclaration>(result.GlobalScope.FindLocal(name)!.Definition);
    private static VariableDeclaration Variable(AnalysisResult result, string name) => As<VariableDeclaration>(result.GlobalScope.FindLocal(name)!.Declarations[0]);
    private static IntegerKind Kind(SemanticType type) => As<IntegerType>(type).IntegerKind;
    internal static void LexicalRanges()
    {
        using var session = new CompilationSession();
        var result = session.Lex("// comment\r\nint\tvalue = 0x2aU; /* done */\n", "ranges.cpp");
        Check(result.IsSuccess);
        var tokens = result.Tokens.Tokens;
        Equal(6, tokens.Count);
        Equal(TokenKind.Keyword, tokens[0].Kind);
        Equal("int", tokens[0].Spelling);
        Equal(2, tokens[0].Range.StartPosition.Line);
        Equal(1, tokens[0].Range.StartPosition.Column);
        Equal(TokenKind.Identifier, tokens[1].Kind);
        Equal(5, tokens[1].Range.StartPosition.Column);
        Equal("0x2aU", tokens[3].Spelling);
        Equal(TokenKind.IntegerLiteral, tokens[3].Kind);
        Equal(TokenKind.EndOfSource, tokens[^1].Kind);
        Equal(0, tokens[^1].Range.Length);
        Same(tokens[0].Range.File, result.Tokens.Source);
        Equal(result.Tokens.Source.Text.Length, tokens[^1].Range.Start);
    }
    internal static void Punctuation()
    {
        using var session = new CompilationSession();
        var tokens = session.Lex("<<= >>= <=> ->* ... %:%: <: :> <% %> %: <::x").Tokens.Tokens;
        TokenKind[] expected = [TokenKind.ShiftLeftEqual, TokenKind.ShiftRightEqual, TokenKind.Spaceship, TokenKind.ArrowStar, TokenKind.Ellipsis,
            TokenKind.HashHash, TokenKind.LeftBracket, TokenKind.RightBracket, TokenKind.LeftBrace, TokenKind.RightBrace, TokenKind.Hash,
            TokenKind.Less, TokenKind.Scope, TokenKind.Identifier, TokenKind.EndOfSource];
        Check(expected.SequenceEqual(tokens.Select(t => t.Kind)));
    }
    internal static void MalformedLexicalInput()
    {
        using var session = new CompilationSession();
        foreach (string text in new[] { "/* unterminated", "\"bad\n", "'bad", "@" }) Check(!session.Lex(text).IsSuccess, text);
        Check(session.Lex("// line\n/* block */").IsSuccess);
        Check(session.Lex("// splice\\\nint x;").IsSuccess);
        Check(!session.Lex("int \\u0061;").IsSuccess);
        Equal(TokenKind.EndOfSource, session.Lex("").Tokens.Tokens.Single().Kind);
    }
    internal static void TokenReplay()
    {
        using var session = new CompilationSession();
        TokenBuffer buffer = session.Lex("int item = 2;").Tokens;
        TokenCursor cursor = buffer.CreateCursor();
        TokenCheckpoint start = cursor.Checkpoint();
        cursor.Advance(); cursor.Advance();
        Equal("=", cursor.Current.Spelling);
        cursor.Restore(start);
        Equal("int", cursor.Current.Spelling);
        TokenBuffer retained = buffer.Retain(1, 3);
        Equal("item", retained.CreateCursor().Current.Spelling);
        Equal("2", retained.Tokens[^2].Spelling);
        Equal(TokenKind.EndOfSource, retained.Tokens[^1].Kind);
        Throws<ArgumentException>(() => retained.CreateCursor().Restore(start));
        while (cursor.Current.Kind != TokenKind.EndOfSource) cursor.Advance();
        int index = cursor.Index;
        cursor.Advance();
        Equal(index, cursor.Index);
        Equal(TokenKind.EndOfSource, cursor.Peek(int.MaxValue).Kind);
    }
    private static ParseState NewState()
    {
        using var session = new CompilationSession();
        return new ParseState(session.Lex("int x;").Tokens.CreateCursor(), new DiagnosticBag());
    }
    internal static void SemanticCheckpoint()
    {
        ParseState state = NewState();
        var table = new TypeTable(TargetInfo.Lp64);
        state.PendingReplays.Add(new CompilationSession().Lex("int old;").Tokens);
        var outer = state.Checkpoint();
        Scope scope = state.PushScope(ScopeKind.Block);
        var symbol = new Symbol("name", SymbolKind.Variable, table.Integer(IntegerKind.Int), scope);
        state.Enter(symbol);
        var declaration = new VariableDeclaration(symbol, symbol.Type, state.Tokens.Current.Range, state.Tokens.Current.Range);
        state.AddDeclaration(declaration);
        var inner = state.Checkpoint();
        state.Tokens.Advance();
        state.Diagnostics.Add("TEST", "speculative", state.Tokens.Current.Range);
        state.PendingReplays.Clear();
        state.Restore(inner);
        Same(scope, state.CurrentScope);
        Same(symbol, scope.FindLocal("name"));
        Equal(0, state.Diagnostics.Count);
        Equal(1, state.PendingReplays.Count);
        Equal("int", state.Tokens.Current.Spelling);
        state.Restore(inner); // Repeated restore on an unchanged valid branch.
        state.Restore(outer);
        Same(state.GlobalScope, state.CurrentScope);
        Equal(0, scope.Declarations.Count);
        Equal(0, symbol.Declarations.Count);
        Check(scope.FindLocal("name") == null);
        Equal(0, state.GlobalScope.Children.Count);
        Throws<ArgumentException>(() => NewState().Restore(outer));
    }
    internal static void DefinitionCheckpoint()
    {
        ParseState state = NewState();
        var table = new TypeTable(TargetInfo.Lp64);
        FunctionType type = table.Function(table.Integer(IntegerKind.Int), []);
        var function = new Symbol("f", SymbolKind.Function, type, state.GlobalScope);
        var initiallyUndefined = new Symbol("g", SymbolKind.Function, type, state.GlobalScope);
        state.Enter(function);
        state.Enter(initiallyUndefined);
        FunctionDeclaration Definition(Symbol symbol)
        {
            Scope parameters = state.PushScope(ScopeKind.Function);
            state.PopScope();
            SourceRange range = state.Tokens.Current.Range;
            var declaration = new FunctionDeclaration(symbol, type, range, range, [], parameters);
            declaration.Body = new BlockStatement(parameters, [], range);
            state.AddDeclaration(declaration);
            return declaration;
        }
        FunctionDeclaration original = Definition(function);
        FunctionDeclaration middle = Definition(function);
        FunctionDeclaration latest = Definition(function);
        FunctionDeclaration gMiddle = Definition(initiallyUndefined);
        FunctionDeclaration gLatest = Definition(initiallyUndefined);
        state.SetDefinition(function, original);
        var baseline = state.Checkpoint();
        state.CurrentFunction = original;
        var outer = state.Checkpoint();
        state.SetDefinition(function, middle);
        state.SetDefinition(initiallyUndefined, gMiddle);
        state.CurrentFunction = middle;
        var inner = state.Checkpoint();
        state.SetDefinition(function, latest);
        state.SetDefinition(initiallyUndefined, gLatest);
        state.CurrentFunction = latest;
        state.Restore(inner);
        Same(middle, function.Definition);
        Same(gMiddle, initiallyUndefined.Definition);
        Same(middle, state.CurrentFunction);
        state.SetDefinition(function, latest);
        state.CurrentFunction = latest;
        state.Restore(inner);
        Same(middle, function.Definition);
        Same(middle, state.CurrentFunction);
        state.Restore(outer);
        Same(original, function.Definition);
        Check(initiallyUndefined.Definition == null);
        Same(original, state.CurrentFunction);
        Same(function, state.GlobalScope.FindLocal("f"));
        Throws<ArgumentException>(() => state.Restore(inner));
        Same(original, function.Definition);
        Same(original, state.CurrentFunction);
        state.Restore(baseline);
        Same(original, function.Definition);
        Check(state.CurrentFunction == null);
    }
    internal static void StaleCheckpoint()
    {
        ParseState state = NewState();
        var outer = state.Checkpoint();
        state.PushScope(ScopeKind.Block);
        state.Diagnostics.Add("TEST", "branch A", state.Tokens.Current.Range);
        var branchA = state.Checkpoint();
        state.Restore(outer);
        Scope branchB = state.PushScope(ScopeKind.Block);
        state.Diagnostics.Add("TEST", "branch B", state.Tokens.Current.Range);
        Throws<ArgumentException>(() => state.Restore(branchA));
        Same(branchB, state.CurrentScope);
        Same(branchB, state.GlobalScope.Children.Single());
        Equal(1, state.Diagnostics.Count);
        state.Restore(outer);
        state.Diagnostics.Add("TEST", "diagnostic only", state.Tokens.Current.Range);
        var diagnosticBranch = state.Checkpoint();
        state.Restore(outer);
        Throws<ArgumentException>(() => state.Restore(diagnosticBranch));
        Equal(0, state.Diagnostics.Count);
        Same(state.GlobalScope, state.CurrentScope);
    }
    internal static void Sessions()
    {
        var session = new CompilationSession();
        AnalysisResult first = session.Analyze("int value;");
        AnalysisResult second = session.Analyze("int value;");
        Same(first.Declarations[0].Type, second.Declarations[0].Type);
        Check(!ReferenceEquals(first.Declarations[0].Symbol, second.Declarations[0].Symbol));
        using var independent = new CompilationSession();
        AnalysisResult other = independent.Analyze("int value;");
        Check(!ReferenceEquals(first.Declarations[0].Type, other.Declarations[0].Type));
        Check(!ReferenceEquals(first.Source, other.Source));
        session.Dispose(); session.Dispose();
        Equal("value", first.Declarations[0].Name);
        Throws<ObjectDisposedException>(() => session.Analyze(""));
        Throws<ObjectDisposedException>(() => session.Lex(""));
    }
    internal static void ConcurrentSessions()
    {
        var tasks = Enumerable.Range(0, 8).Select(i => Task.Run(() => Success($"int f{i}() {{ return {i}; }}"))).ToArray();
        Task.WaitAll(tasks);
        for (int i = 0; i < tasks.Length; ++i) Equal($"f{i}", tasks[i].Result.Declarations.Single().Name);
    }
    internal static void Options()
    {
        Throws<ArgumentOutOfRangeException>(() => new CompilationSession(new AnalysisOptions { Language = (LanguageProfile)99 }));
        Throws<ArgumentNullException>(() => new CompilationSession(new AnalysisOptions { Target = null! }));
    }
    internal static void ReadOnlyGraph()
    {
        AnalysisResult result = Success("int f(int a) { return a; }");
        Throws<NotSupportedException>(() => ((IList)result.Declarations).Clear());
        Throws<NotSupportedException>(() => ((IList)result.Tokens.Tokens).Clear());
        FunctionDeclaration function = Function(result, "f");
        Throws<NotSupportedException>(() => ((IList)function.Parameters).Clear());
        Throws<NotSupportedException>(() => ((IList)function.Type.ParameterTypes).Clear());
        Throws<NotSupportedException>(() => ((IList)function.Body!.Statements).Clear());
    }
    internal static void PositiveGraph()
    {
        AnalysisResult result = Success(Fixture("m2-positive.cpp"));
        Equal(4, result.Declarations.Count);
        var alias = As<AliasDeclaration>(result.Declarations[0]);
        var prototype = As<FunctionDeclaration>(result.Declarations[1]);
        var definition = As<FunctionDeclaration>(result.Declarations[2]);
        Same(prototype.Symbol, definition.Symbol);
        Same(prototype.Type, definition.Type);
        Same(definition, prototype.Symbol.Definition);
        Same(alias.AliasedType, definition.Type.ReturnType);
        Same(definition.ParameterScope, definition.Body!.Scope);
        var local = As<DeclarationStatement>(definition.Body.Statements[0]).Declaration;
        var conversion = As<ConversionExpression>(As<VariableDeclaration>(local).Initializer);
        Equal(ConversionKind.IntegralConversion, conversion.Conversion);
        Same(definition.Parameters[0].Symbol, As<ReferenceExpression>(As<ConversionExpression>(conversion.Operand).Operand).ReferencedSymbol);
        var inner = As<BlockStatement>(definition.Body.Statements[1]);
        Check(!ReferenceEquals(inner.Scope.FindLocal("result"), definition.Body.Scope.FindLocal("result")));
        var call = As<CallExpression>(ReturnValue(Function(result, "entry")));
        Same(definition.Symbol, call.SelectedFunction);
        Same(alias.AliasedType, call.Type);
        Equal(ValueCategory.PRValue, call.ValueCategory);
        Equal("widen(value)", call.Range.Spelling);
    }
    internal static void FixtureBoundaries()
    {
        foreach (string file in new[] { "m2-unresolved.cpp", "m2-conflicting.cpp", "m2-parameter-redeclaration.cpp" })
        {
            AnalysisResult result = Analyze(Fixture(file));
            Check(!result.IsSuccess, file);
            Check(result.Diagnostics.Any(d => d.Class == DiagnosticClass.InvalidSource), file);
        }
        AnalysisResult branch = Success(Fixture("m2-unsupported-statement.cpp"));
        IfStatement conditional = As<IfStatement>(Function(branch, "entry").Body!.Statements[1]);
        Equal(ConversionKind.BooleanConversion, As<ConversionExpression>(conditional.Condition).Conversion);
        Equal(new BigInteger(2), As<ReturnStatement>(conditional.Then.Statements.Single()).Expression!.ConstantValue!.Value);
    }
    internal static void Aliases()
    {
        AnalysisResult result = Success("using I = int; using I = int; typedef I J; J n = 2; int f() { using I = long; I n = 3; return n; }");
        Same(result.Declarations[0].Symbol, result.Declarations[1].Symbol);
        Same(result.Declarations[0].Type, result.Declarations[2].Type);
        Equal(IntegerKind.Int, Kind(Variable(result, "n").Type));
        Check(!Analyze("using I=int; using I=long;").IsSuccess);
        Check(!Analyze("int I; I value;").IsSuccess);
    }
    internal static void SourceOrder()
    {
        Check(!Analyze("int f() { return later(); } int later() { return 1; }").IsSuccess);
        AnalysisResult result = Success("int later(); int f() { return later(); } int later() { return 1; }");
        Same(result.Declarations[0].Symbol, As<CallExpression>(ReturnValue(Function(result, "f"))).SelectedFunction);
        Check(!Analyze("int f() { int first = second; int second = 1; return first; }").IsSuccess);
        AnalysisResult own = Success("int f() { int value = value; return value; }");
        VariableDeclaration variable = As<VariableDeclaration>(As<DeclarationStatement>(Function(own, "f").Body!.Statements[0]).Declaration);
        Same(variable.Symbol, As<ReferenceExpression>(As<ConversionExpression>(variable.Initializer).Operand).ReferencedSymbol);
    }
    internal static void FunctionIdentity()
    {
        AnalysisResult result = Success("int f(int); int f(const int value) { return value; }");
        var definition = Function(result, "f");
        Same(result.Declarations[0].Symbol, definition.Symbol);
        Equal(TypeQualifiers.None, definition.Type.ParameterTypes[0].Qualifiers);
        Equal(TypeQualifiers.Const, definition.Parameters[0].Type.Qualifiers);
        AnalysisResult cv = Success("const int source(); int consumer(){return source();}");
        Equal(TypeQualifiers.Const, As<FunctionType>(cv.GlobalScope.FindLocal("source")!.Type).ReturnType.Qualifiers);
        Equal(TypeQualifiers.None, ReturnValue(Function(cv, "consumer")).Type.Qualifiers);
        Equal(TypeQualifiers.None, As<CallExpression>(ReturnValue(Function(cv, "consumer"))).Type.Qualifiers);
        Check(!Analyze("int f(){return 1;} int f(){return 2;}").IsSuccess);
        Check(Success("int f(int); int f(long);").GlobalScope.FindLocalBinding("f")!.Symbols.Count == 2);
        AnalysisResult aliasVoid = Success("using V=void; int f(V); int f(){return 1;}");
        Same(aliasVoid.Declarations[1].Symbol, aliasVoid.Declarations[2].Symbol);
        Equal(0, As<FunctionDeclaration>(aliasVoid.Declarations[1]).Parameters.Count);
        Equal(0, As<FunctionDeclaration>(aliasVoid.Declarations[1]).Type.ParameterTypes.Count);
        Equal("const unsigned long", As<FunctionDeclaration>(Success("int unnamed(const unsigned long);").Declarations[0]).Parameters[0].Range.Spelling);
        foreach (string invalidVoid in new[] { "using V=void; int f(V named);", "using V=void; int f(V, int);", "using V=void; int f(int, V);" })
            Check(!Analyze(invalidVoid).IsSuccess, invalidVoid);
    }
    private static CallExpression ReturnedCall(AnalysisResult result, string function)
    {
        Expression expression = ReturnValue(Function(result, function));
        while (expression is ConversionExpression conversion) expression = conversion.Operand;
        return As<CallExpression>(expression);
    }
    private static IntegerKind SelectedParameter(CallExpression call, int index = 0)
        => Kind(As<FunctionType>(call.SelectedFunction.Type).ParameterTypes[index]);
    internal static void OverloadBindings()
    {
        AnalysisResult result = Success("int pick(int); long pick(long); int pick(const int x){return x;} long pick(long x){return x;}");
        NameBinding binding = result.GlobalScope.FindLocalBinding("pick")!;
        Equal(2, binding.Symbols.Count);
        Check(binding.SingleSymbol == null);
        Check(result.GlobalScope.FindLocal("pick") == null);
        Check(result.GlobalScope.Lookup("pick") == null);
        Same(result.Declarations[0].Symbol, binding.Symbols[0]);
        Same(result.Declarations[1].Symbol, binding.Symbols[1]);
        Same(result.Declarations[0].Symbol, result.Declarations[2].Symbol);
        Same(result.Declarations[1].Symbol, result.Declarations[3].Symbol);
        Same(result.Declarations[2], binding.Symbols[0].Definition);
        Equal(2, binding.Symbols[0].Declarations.Count);
        Throws<NotSupportedException>(() => ((IList)binding.Symbols).Clear());
        AnalysisResult alias = Success("using I=int; int pick(I); int pick(long); int pick(const I x){return x;}");
        Same(alias.Declarations[1].Symbol, alias.Declarations[3].Symbol);
        Equal(2, alias.GlobalScope.FindLocalBinding("pick")!.Symbols.Count);
        AnalysisResult returnConflict = Analyze("int pick(int); long pick(long); long pick(int);");
        Check(!returnConflict.IsSuccess && !returnConflict.ContainsUnsupportedFeatures);
        Check(returnConflict.Diagnostics.Any(d => d.Code == "SEM009"));
        Equal(2, returnConflict.GlobalScope.FindLocalBinding("pick")!.Symbols.Count);
        foreach (string source in new[] { "int pick(int); int pick(long); int pick;", "int pick; int pick(int);", "using pick=int; int pick(int);" })
        {
            AnalysisResult conflict = Analyze(source);
            Check(!conflict.IsSuccess && conflict.Diagnostics.Any(d => d.Code == "SEM003"), source);
        }
    }
    internal static void OverloadRanking()
    {
        AnalysisResult result = Success("int choose(long); int choose(int); int exact(int x){return choose(x);} int promoted(short x){return choose(x);} int narrow(unsigned char x){return choose(x);} int unsignedShort(unsigned short x){return choose(x);} int flag(bool x){return choose(x);}");
        foreach (string name in new[] { "exact", "promoted", "narrow", "unsignedShort", "flag" })
        {
            CallExpression call = ReturnedCall(result, name);
            Equal(IntegerKind.Int, SelectedParameter(call));
            Equal(2, call.Candidates.Count);
            Check(call.Candidates.All(c => c.IsViable));
            Same(call.SelectedFunction, call.SelectedCandidate.Function);
            Equal(name == "exact" ? StandardConversionRank.ExactMatch : StandardConversionRank.Promotion,
                call.SelectedCandidate.ArgumentConversions[0].Rank);
            Check(call.SelectedCandidate.ArgumentConversions[0].RequiresLValueToRValue);
            Equal(StandardConversionRank.Conversion, call.Candidates[0].ArgumentConversions[0].Rank);
            var converted = As<ConversionExpression>(call.Arguments[0]);
            Equal(name == "exact" ? ConversionKind.LValueToRValue : ConversionKind.IntegralPromotion, converted.Conversion);
            if (name != "exact") Equal(ConversionKind.LValueToRValue, As<ConversionExpression>(converted.Operand).Conversion);
            Throws<NotSupportedException>(() => ((IList)call.Candidates).Clear());
            Throws<NotSupportedException>(() => ((IList)call.SelectedCandidate.ArgumentConversions).Clear());
            Throws<NotSupportedException>(() => ((IList)As<OverloadSetExpression>(call.Callee).Functions).Clear());
        }
        AnalysisResult cv = Success("int pick(int); int pick(long); int run(const int x){return (pick)(x);}");
        CallExpression exact = ReturnedCall(cv, "run");
        Equal(IntegerKind.Int, SelectedParameter(exact));
        Equal(TypeQualifiers.Const, exact.SelectedCandidate.ArgumentConversions[0].SourceType.Qualifiers);
        Equal(TypeQualifiers.None, exact.SelectedCandidate.ArgumentConversions[0].TargetType.Qualifiers);
        Equal(StandardConversionRank.ExactMatch, exact.SelectedCandidate.ArgumentConversions[0].Rank);
        AnalysisResult boolean = Success("int one(bool); int run(){return one(256);}");
        CallExpression toBool = ReturnedCall(boolean, "run");
        Equal(StandardConversionRank.Conversion, toBool.SelectedCandidate.ArgumentConversions[0].Rank);
        Equal(ConversionKind.BooleanConversion, As<ConversionExpression>(toBool.Arguments[0]).Conversion);
        Equal(BigInteger.One, toBool.Arguments[0].ConstantValue!.Value);
    }
    internal static void OverloadAmbiguity()
    {
        foreach (string source in new[]
        {
            "int pick(long); int pick(long long); int run(){return pick(1);}",
            "int pick(short); int pick(long); int run(){return pick(1);}",
            "int pick(bool); int pick(long); int run(){return pick(1);}",
            "int pick(long); int pick(unsigned long); int run(unsigned int x){return pick(x);}",
            "int pick(int,long); int pick(long,int); int run(){return pick(1,1);}",
            "int pick(int,long,long); int pick(long,int,int); int run(){return pick(1,1,1);}",
            "int pick(int,long); int pick(long,int); int pick(long,long); int run(){return pick(1,1);}"
        })
        {
            AnalysisResult result = Analyze(source);
            Check(!result.IsSuccess && !result.ContainsUnsupportedFeatures, source);
            Equal(1, result.Diagnostics.Count(d => d.Code == "SEM026"));
            Check(ReturnValue(Function(result, "run")) is ErrorExpression);
        }
        AnalysisResult dominated = Success("int pick(long,long); int pick(int,long); int pick(int,int); int run(){return pick(1,1);}");
        CallExpression selected = ReturnedCall(dominated, "run");
        Equal(IntegerKind.Int, SelectedParameter(selected));
        Equal(IntegerKind.Int, SelectedParameter(selected, 1));
    }
    internal static void OverloadDeclarationOrder()
    {
        foreach (string declarations in new[] { "int pick(long); int pick(int);", "int pick(int); int pick(long);" })
        {
            AnalysisResult result = Success(declarations + "int run(short x){return pick(x);}");
            CallExpression call = ReturnedCall(result, "run");
            Equal(IntegerKind.Int, SelectedParameter(call));
            Equal(StandardConversionRank.Promotion, call.SelectedCandidate.ArgumentConversions[0].Rank);
        }
        foreach (string declarations in new[] { "int pick(long); long pick(long long);", "long pick(long long); int pick(long);" })
        {
            AnalysisResult result = Analyze(declarations + "int run(){return pick(1);}");
            Check(result.Diagnostics.Any(d => d.Code == "SEM026"));
        }
        string[] signatures = ["int pick(long,long);", "int pick(int,long);", "int pick(int,int);"];
        int[][] permutations = [[0, 1, 2], [0, 2, 1], [1, 0, 2], [1, 2, 0], [2, 0, 1], [2, 1, 0]];
        foreach (int[] order in permutations)
        {
            AnalysisResult result = Success(string.Concat(order.Select(index => signatures[index])) + "int run(){return pick(1,1);}");
            CallExpression call = ReturnedCall(result, "run");
            Equal(IntegerKind.Int, SelectedParameter(call));
            Equal(IntegerKind.Int, SelectedParameter(call, 1));
        }
    }
    internal static void OverloadViability()
    {
        AnalysisResult result = Success("int pick(); int pick(int); int pick(int,int); int run(){return pick(1);}");
        CallExpression call = ReturnedCall(result, "run");
        Equal(3, call.Candidates.Count);
        Equal(CandidateRejectionReason.ArityMismatch, call.Candidates[0].RejectionReason);
        Equal(CandidateRejectionReason.None, call.Candidates[1].RejectionReason);
        Equal(CandidateRejectionReason.ArityMismatch, call.Candidates[2].RejectionReason);
        Equal(0, call.Candidates[0].ArgumentConversions.Count);
        Same(call.Candidates[1], call.SelectedCandidate);
        foreach (string source in new[]
        {
            "int pick(int); int pick(int,int); int run(){return pick();}",
            "int pick(); int pick(int); int run(){return pick(1,2);}",
            "void nothing(); int pick(int); int pick(long); int run(){return pick(nothing());}"
        })
        {
            AnalysisResult invalid = Analyze(source);
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, source);
            Check(invalid.Diagnostics.Any(d => d.Code == "SEM023"));
        }
        AnalysisResult functionArgument = Analyze("int source(); int pick(int); int pick(long); int run(){return pick(source);}");
        Check(!functionArgument.IsSuccess && functionArgument.ContainsUnsupportedFeatures);
        AnalysisResult unresolved = Analyze("int pick(int); int pick(long); int run(){return pick(missing);}");
        Equal(1, unresolved.Diagnostics.Count);
        Equal("SEM021", unresolved.Diagnostics[0].Code);
    }
    internal static void OverloadSourceOrder()
    {
        AnalysisResult result = Success("int pick(long); int early(){return pick(1);} int pick(int); int late(){return pick(1);} int pick(const int x){return x;}");
        CallExpression early = ReturnedCall(result, "early");
        CallExpression late = ReturnedCall(result, "late");
        Equal(IntegerKind.Long, SelectedParameter(early));
        Equal(IntegerKind.Int, SelectedParameter(late));
        Equal(1, early.Candidates.Count);
        Equal(2, late.Candidates.Count);
        Same(result.Declarations[0].Symbol, early.SelectedFunction);
        Same(result.Declarations[2].Symbol, late.SelectedFunction);
        Same(result.Declarations[4], late.SelectedFunction.Definition);
        AnalysisResult three = Success("int pick(long); int pick(unsigned long); int early(){return pick(1L);} int pick(int); int late(){return pick(1);}");
        CallExpression beforeThird = ReturnedCall(three, "early");
        Equal(2, beforeThird.Candidates.Count);
        Equal(2, As<OverloadSetExpression>(beforeThird.Callee).Functions.Count);
        Equal(3, three.GlobalScope.FindLocalBinding("pick")!.Symbols.Count);
        Equal(IntegerKind.Long, SelectedParameter(beforeThird));
        Check(!Analyze("int early(){return pick(1);} int pick(int);").IsSuccess);
        AnalysisResult noRetroactiveMatch = Analyze("int pick(int,int); int early(){return pick(1);} int pick(int);");
        Check(noRetroactiveMatch.Diagnostics.Any(d => d.Code == "SEM023"));
    }
    internal static void OverloadHiding()
    {
        foreach (string source in new[]
        {
            "int pick(int); int pick(long); int run(int pick){return pick(1);}",
            "int pick(int); int pick(long); int run(){int pick=0; return pick(1);}",
            "int pick(int); int pick(long); int run(){{int pick=0; return pick(1);} return 0;}"
        })
        {
            AnalysisResult result = Analyze(source);
            Check(!result.IsSuccess && !result.ContainsUnsupportedFeatures, source);
            Check(result.Diagnostics.Any(d => d.Code == "SEM022"));
        }
        AnalysisResult restored = Success("int pick(int); int pick(long); int run(){{int pick=0; pick+1;} return pick(1);}");
        Equal(IntegerKind.Int, SelectedParameter(ReturnedCall(restored, "run")));
        foreach (string source in new[]
        {
            "int pick(int); int pick(long); int run(){return pick;}",
            "int pick(int); int pick(long); int run(){pick; return 0;}",
            "int pick(int); int pick(long); int run(){return !pick;}",
            "int pick(int); int pick(long); int run(){return pick+1;}"
        }) Check(Analyze(source).ContainsUnsupportedFeatures, source);
    }
    internal static void OverloadCheckpoint()
    {
        ParseState state = NewState();
        var table = new TypeTable(TargetInfo.Lp64);
        IntegerType integer = table.Integer(IntegerKind.Int);
        var first = new Symbol("pick", SymbolKind.Function, table.Function(integer, [integer]), state.GlobalScope);
        state.Enter(first);
        NameBinding binding = state.GlobalScope.FindLocalBinding("pick")!;
        var outer = state.Checkpoint();
        var second = new Symbol("pick", SymbolKind.Function, table.Function(integer, [table.Integer(IntegerKind.Long)]), state.GlobalScope);
        state.Enter(second);
        var inner = state.Checkpoint();
        var third = new Symbol("pick", SymbolKind.Function, table.Function(integer, []), state.GlobalScope);
        state.Enter(third);
        Equal(3, binding.Symbols.Count);
        state.Restore(inner);
        Same(binding, state.GlobalScope.FindLocalBinding("pick"));
        Equal(2, binding.Symbols.Count);
        Same(second, binding.Symbols[1]);
        state.Restore(outer);
        Same(binding, state.GlobalScope.FindLocalBinding("pick"));
        Same(first, state.GlobalScope.FindLocal("pick"));
        Equal(1, binding.Symbols.Count);
        Scope child = state.PushScope(ScopeKind.Block);
        var nonfunction = new Symbol("pick", SymbolKind.Variable, integer, child);
        state.Enter(nonfunction);
        Same(nonfunction, child.Lookup("pick"));
        Same(child.FindLocalBinding("pick")!, child.LookupBinding("pick"));
        state.Restore(outer);
        Equal(0, state.GlobalScope.Children.Count);
        Scope overloadScope = state.PushScope(ScopeKind.Block);
        state.Enter(new Symbol("pick", SymbolKind.Function, table.Function(integer, [table.Integer(IntegerKind.Long)]), overloadScope));
        state.Enter(new Symbol("pick", SymbolKind.Function, table.Function(integer, []), overloadScope));
        Check(overloadScope.Lookup("pick") == null);
        Same(overloadScope.FindLocalBinding("pick")!, overloadScope.LookupBinding("pick"));
        state.Restore(outer);
        Same(first, state.GlobalScope.FindLocal("pick"));
    }
    internal static void OverloadFixtures()
    {
        AnalysisResult positive = Success(Fixture("m3-positive.cpp"));
        Equal(IntegerKind.Int, SelectedParameter(ReturnedCall(positive, "exact")));
        Equal(IntegerKind.Int, SelectedParameter(ReturnedCall(positive, "promotion")));
        Equal(StandardConversionRank.Promotion, ReturnedCall(positive, "promotion").SelectedCandidate.ArgumentConversions[0].Rank);
        Same(positive.Declarations[0].Symbol, positive.Declarations[2].Symbol);
        AnalysisResult order = Success(Fixture("m3-order.cpp"));
        Equal(IntegerKind.Long, SelectedParameter(ReturnedCall(order, "before")));
        Equal(IntegerKind.Int, SelectedParameter(ReturnedCall(order, "after")));
        foreach (var (fixture, code) in new[] { ("m3-ambiguous.cpp", "SEM026"), ("m3-crossed.cpp", "SEM026"), ("m3-no-viable.cpp", "SEM023"), ("m3-hiding.cpp", "SEM022") })
        {
            AnalysisResult invalid = Analyze(Fixture(fixture));
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, fixture);
            Check(invalid.Diagnostics.Any(d => d.Code == code), fixture);
        }
    }
    private static FunctionTemplateDeclaration Template(AnalysisResult result, string name)
        => result.GlobalScope.FindLocal(name)!.TemplateDefinition!;
    internal static void TemplateGraph()
    {
        AnalysisResult result = Success("template<class T> T id(const T value){T local=value; return local;} int one(){return id(1);} long two(){return id(2L);} int again(){return id(3);}");
        FunctionTemplateDeclaration template = Template(result, "id");
        Equal(SymbolKind.FunctionTemplate, template.Symbol.Kind);
        Equal(TypeKind.TemplateParameter, template.Type.ReturnType.Kind);
        Same(template.TypeParameters[0].Type, template.Type.ReturnType);
        Same(template.TemplateScope, template.TypeParameters[0].Type.ParameterScope);
        Equal(TypeQualifiers.Const, template.Pattern.Parameters[0].Type.Qualifiers);
        Equal(TypeQualifiers.None, template.Type.ParameterTypes[0].Qualifiers);
        Check(template.Pattern.Body != null && template.IsValid);
        Check(ReturnValue(template.Pattern) is DependentConversionExpression);
        Same(result.Source, template.BodyTokens.Source);
        Equal(TokenKind.LeftBrace, template.BodyTokens.Tokens[0].Kind);
        Equal(TokenKind.RightBrace, template.BodyTokens.Tokens[^2].Kind);
        Equal(TokenKind.EndOfSource, template.BodyTokens.Tokens[^1].Kind);
        Equal(2, template.Instances.Count);
        CallExpression first = ReturnedCall(result, "one"), second = ReturnedCall(result, "two"), again = ReturnedCall(result, "again");
        Same(first.TemplateInstance!, again.TemplateInstance);
        Check(!ReferenceEquals(first.TemplateInstance, second.TemplateInstance));
        Same(first.SelectedFunction, first.TemplateInstance!.Symbol);
        Same(template, first.SelectedCandidate.Template);
        Equal(IntegerKind.Int, Kind(first.SelectedCandidate.TemplateArguments[0]));
        foreach (FunctionTemplateInstance instance in template.Instances)
        {
            Equal(TemplateInstanceState.Complete, instance.State);
            Equal(1, instance.BodyReplayCount);
            Same(instance.Declaration!, instance.Symbol.Definition);
            Same(instance, instance.Declaration!.TemplateInstance);
            Same(template.TemplateScope, instance.Declaration.ParameterScope.Parent);
            Equal(TypeQualifiers.Const, instance.Declaration.Parameters[0].Type.Qualifiers);
            Equal(TypeQualifiers.None, instance.Declaration.Type.ParameterTypes[0].Qualifiers);
            Check(!instance.Declaration.Type.ParameterTypes[0].DisplayName.Contains('T'));
            Throws<NotSupportedException>(() => ((IList)instance.TypeArguments).Clear());
        }
        Throws<NotSupportedException>(() => ((IList)template.Instances).Clear());
        Throws<NotSupportedException>(() => ((IList)template.TypeParameters).Clear());
        Throws<NotSupportedException>(() => ((IDictionary)template.DefinitionContext.Bindings).Clear());
    }
    internal static void TemplateDeduction()
    {
        AnalysisResult result = Success("int same(long,long); template<class T> T same(T a,T b){return a;} int run(){return same(1,2L);}");
        CallExpression call = ReturnedCall(result, "run");
        Check(call.TemplateInstance == null);
        Equal(CandidateRejectionReason.DeductionFailure, call.Candidates[1].RejectionReason);
        Equal(0, call.Candidates[1].Template!.Instances.Count);
        AnalysisResult consistent = Success("template<class T> T same(const T a,T b){return a;} int run(const int x){return same(x,2);}");
        Equal(IntegerKind.Int, Kind(ReturnedCall(consistent, "run").TemplateInstance!.TypeArguments[0]));
        AnalysisResult shortArgument = Success("template<class T> T id(T value){return value;} short run(short value){return id(value);}");
        CallExpression shortCall = ReturnedCall(shortArgument, "run");
        Equal(IntegerKind.Short, Kind(shortCall.TemplateInstance!.TypeArguments[0]));
        Equal(IntegerKind.Short, SelectedParameter(shortCall));
        Equal(StandardConversionRank.ExactMatch, shortCall.SelectedCandidate.ArgumentConversions[0].Rank);
        Equal(ConversionKind.LValueToRValue, As<ConversionExpression>(shortCall.Arguments[0]).Conversion);
        Equal(IntegerKind.Short, Kind(shortCall.Arguments[0].Type));
        AnalysisResult pair = Success("template<class A,typename B> A first(A a,B b){return a;} int run(){return first(1,2L);}");
        Equal(2, Template(pair, "first").Instances[0].TypeArguments.Count);
        foreach (string source in new[]
        {
            "template<class T> T same(T a,T b){return a;} int run(){return same(1,2L);}",
            "template<class T> T same(T a,T b){return a;} int run(short value){return same(value,1);}",
            "template<class T> T id(T a){return a;} int run(){return id();}",
            "template<class T> T unknown(int a){return a;} int run(){return unknown(1);}"
        })
        {
            AnalysisResult invalid = Analyze(source);
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, source);
            Check(invalid.Diagnostics.Any(d => d.Code == "SEM023"));
        }
    }
    internal static void TemplateOverloads()
    {
        foreach (string source in new[]
        {
            "int pick(int); template<class T> T pick(T x){return x;} int run(){return pick(1);}",
            "template<class T> T pick(T x){return x;} int pick(int); int run(){return pick(1);}"
        })
        {
            AnalysisResult result = Success(source);
            Check(ReturnedCall(result, "run").TemplateInstance == null);
            Equal(SymbolKind.Function, ReturnedCall(result, "run").SelectedFunction.Kind);
            Equal(0, result.GlobalScope.FindLocalBinding("pick")!.Symbols.Single(s => s.Kind == SymbolKind.FunctionTemplate).TemplateDefinition!.Instances.Count);
        }
        AnalysisResult better = Success("int pick(long); template<class T> T pick(T x){return x;} int run(){return pick(1);}");
        Check(ReturnedCall(better, "run").TemplateInstance != null);
        AnalysisResult crossed = Analyze(Fixture("m4-crossed.cpp"));
        Check(!crossed.IsSuccess && crossed.Diagnostics.Any(d => d.Code == "SEM026"));
        AnalysisResult losing = Success(Fixture("m4-losing-template.cpp"));
        Check(ReturnedCall(losing, "entry").TemplateInstance == null);
        FunctionTemplateDeclaration unused = losing.GlobalScope.FindLocalBinding("pick")!.Symbols.Single(s => s.Kind == SymbolKind.FunctionTemplate).TemplateDefinition!;
        Equal(0, unused.Instances.Count);
        AnalysisResult selectedBad = Analyze("void sink(int); template<class T> T pick(T x){return sink(x);} int run(){return pick(1);}");
        Check(!selectedBad.IsSuccess && !selectedBad.ContainsUnsupportedFeatures);
        Equal(TemplateInstanceState.Failed, Template(selectedBad, "pick").Instances[0].State);
        AnalysisResult ordered = Success("template<class T,class U> int pair(T a,U b){return 1;} template<class V> int pair(V a,V b){return 2;} int run(){return pair(1,1);}");
        Equal(1, ReturnedCall(ordered, "run").SelectedCandidate.Template!.TypeParameters.Count);
        AnalysisResult unordered = Analyze("template<class T> int pair(T a,int b){return 1;} template<class U> int pair(int a,U b){return 2;} int run(){return pair(1,1);}");
        Check(unordered.Diagnostics.Any(d => d.Code == "SEM026"));
        AnalysisResult order = Success("template<class T> T pick(T x){return x;} int before(){return pick(1);} int pick(int); int after(){return pick(1);}");
        Check(ReturnedCall(order, "before").TemplateInstance != null);
        Check(ReturnedCall(order, "after").TemplateInstance == null);
        Equal(1, ReturnedCall(order, "before").Candidates.Count);
    }
    internal static void CombinedTemplateGate()
    {
        AnalysisResult result = Success(Fixture("m4-combined-gate.cpp"));
        Equal(3, result.StaticAssertions.Count);
        foreach (StaticAssertion assertion in result.StaticAssertions)
        {
            Equal(ConstantEvaluationStatus.Success, assertion.Evaluation.Status);
            Equal(BigInteger.One, assertion.Evaluation.Value!.Value);
            Check(assertion.Evaluation.Steps > 10 && assertion.Evaluation.MaximumCallDepth >= 2);
            Check(!assertion.IsDeferred);
        }
        FunctionTemplateDeclaration combine = Template(result, "combine");
        FunctionTemplateDeclaration identity = Template(result, "identity");
        Equal(2, combine.Instances.Count);
        Equal(2, identity.Instances.Count);
        CallExpression fromRun = ReturnedCall(result, "run");
        Same(combine.Instances[0], fromRun.TemplateInstance);
        var third = As<BinaryExpression>(result.StaticAssertions[2].Expression);
        Same(fromRun.TemplateInstance!, As<CallExpression>(third.Left).TemplateInstance);
        var prototypeReturn = As<DependentConversionExpression>(ReturnValue(combine.Pattern));
        var prototypeBinary = As<DependentBinaryExpression>(prototypeReturn.Operand);
        CallExpression nondependent = As<CallExpression>(prototypeBinary.Left);
        Equal(IntegerKind.Long, SelectedParameter(nondependent));
        Equal(1, nondependent.Candidates.Count);
        Equal(1, combine.DefinitionContext.Bindings["mark"].Count);
        Equal(2, result.GlobalScope.FindLocalBinding("mark")!.Symbols.Count);
        foreach (FunctionTemplateInstance instance in combine.Instances)
        {
            Equal(1, instance.BodyReplayCount);
            var binary = As<BinaryExpression>(ReturnValue(instance.Declaration!));
            CallExpression mark = As<CallExpression>(binary.Left);
            Same(nondependent.SelectedFunction, mark.SelectedFunction);
            Equal(IntegerKind.Long, SelectedParameter(mark));
            Equal(1, mark.Candidates.Count);
            var copy = As<VariableDeclaration>(As<DeclarationStatement>(instance.Declaration!.Body!.Statements[0]).Declaration);
            Check(As<CallExpression>(copy.Initializer).TemplateInstance != null);
        }
        Throws<NotSupportedException>(() => ((IList)result.StaticAssertions).Clear());
        AnalysisResult globals = Success("template<class T> constexpr T id(T x){return x;} constexpr long answer=id(6L); static_assert(answer==6);");
        Equal(ConstantEvaluationStatus.Success, Variable(globals, "answer").ConstantEvaluation!.Status);
        Equal(new BigInteger(6), Variable(globals, "answer").ConstantEvaluation!.Value!.Value);
    }
    internal static void TemplatePrototypeErrors()
    {
        foreach (var (source, code) in new[]
        {
            (Fixture("m4-unused-name.cpp"), "SEM021"),
            (Fixture("m4-unused-call.cpp"), "SEM023"),
            ("template<class T> int unused(T value){return missing();}", "SEM021"),
            ("template<class T> int unused(int T){return T;}", "TMP001"),
            ("template<class T> T unused(T value){int T=0; return value;}", "TMP001"),
            ("template<class T> T unused(T value){{int T=0;} return value;}", "TMP001"),
            ("template<class T> int T(int value){return value;}", "TMP001")
        })
        {
            AnalysisResult result = Analyze(source);
            Check(!result.IsSuccess && !result.ContainsUnsupportedFeatures, source);
            Check(result.Diagnostics.Any(d => d.Code == code), source);
        }
        AnalysisResult noLater = Analyze("template<class T> int unused(T x){return later(0);} int later(int);");
        Check(noLater.Diagnostics.Any(d => d.Code == "SEM021"));
    }
    internal static void TemplateAssertions()
    {
        AnalysisResult unused = Success(Fixture("m4-false-unused.cpp"));
        var deferred = As<StaticAssertStatement>(Template(unused, "assertion").Pattern.Body!.Statements[0]).Assertion;
        Check(deferred.IsDeferred);
        Equal(ConstantEvaluationStatus.Success, deferred.Evaluation.Status);
        Equal(BigInteger.Zero, deferred.Evaluation.Value!.Value);
        AnalysisResult selected = Analyze(Fixture("m4-false-selected.cpp"));
        Check(!selected.IsSuccess && selected.Diagnostics.Any(d => d.Code == "CEX002"));
        AnalysisResult nonconstant = Analyze("int value; template<class T> int assertion(T x){static_assert(value); return 1;}");
        Check(!nonconstant.IsSuccess && nonconstant.Diagnostics.Any(d => d.Code == "CEX001"));
        AnalysisResult malformed = Analyze("template<class T> int assertion(T x){static_assert(1+); return 1;}");
        Check(!malformed.IsSuccess);
    }
    internal static void TemplateValueDependency()
    {
        AnalysisResult unused = Success("template<class T> bool dependent(T x){constexpr bool b=!x; static_assert(!x); return b;}");
        FunctionTemplateDeclaration template = Template(unused, "dependent");
        var local = As<VariableDeclaration>(As<DeclarationStatement>(template.Pattern.Body!.Statements[0]).Declaration);
        Check(!local.Initializer!.IsTypeDependent && local.Initializer.IsValueDependent);
        Equal(ConstantEvaluationStatus.Dependent, local.ConstantEvaluation!.Status);
        var assertion = As<StaticAssertStatement>(template.Pattern.Body.Statements[1]).Assertion;
        Check(assertion.IsDeferred && assertion.Expression.IsValueDependent);
        Equal(ConstantEvaluationStatus.Dependent, assertion.Evaluation.Status);
        AnalysisResult instantiated = Analyze("template<class T> bool dependent(T x){constexpr bool b=!x; return b;} bool run(){return dependent(1);}");
        Check(!instantiated.IsSuccess && instantiated.Diagnostics.Any(d => d.Code == "CEX001"));
        AnalysisResult knownArity = Analyze("int need(int); template<class T> int bad(T x){const int n=x; need(n,n); return 1;}");
        Check(!knownArity.IsSuccess && knownArity.Diagnostics.Any(d => d.Code == "SEM023"));
        foreach (string qualifier in new[] { "constexpr", "const" })
        {
            AnalysisResult required = Success("template<class T> constexpr T id(T x){return x;} template<class U> int outer(U x){" + qualifier + " int n=id(1); static_assert(n==1); return n;}");
            FunctionTemplateDeclaration outer = Template(required, "outer");
            Equal(0, outer.Instances.Count);
            FunctionTemplateInstance identity = Template(required, "id").Instances.Single();
            Equal(TemplateInstanceState.Complete, identity.State);
            Equal(1, identity.BodyReplayCount);
            var initialized = As<VariableDeclaration>(As<DeclarationStatement>(outer.Pattern.Body!.Statements[0]).Declaration);
            Equal(ConstantEvaluationStatus.Success, initialized.ConstantEvaluation!.Status);
        }
        const string optionalRuntime = "void sink(int); template<class T> T runtime(T value){return sink(value);} template<class U> int unused(U x){const int n=runtime(1); return n;}";
        AnalysisResult optional = Success(optionalRuntime);
        FunctionTemplateInstance deferredRuntime = Template(optional, "runtime").Instances.Single();
        Equal(TemplateInstanceState.NotInstantiated, deferredRuntime.State);
        Equal(0, deferredRuntime.BodyReplayCount);
        Check(!Analyze(optionalRuntime + "int run(){return unused(1);}").IsSuccess);
    }
    internal static void ConstexprEvaluation()
    {
        AnalysisResult result = Success("constexpr int add(int a,int b){int local=a+b; {int local=9; local+1;} return local;} constexpr int twice(int x){int local=add(x,x); return add(local,1);} constexpr int early(){return 7; int never=1/0;} static_assert(twice(3)==7); static_assert(early()==7); static_assert(4294967295u+1u==0u); static_assert(-7%3==-1);");
        Equal(4, result.StaticAssertions.Count);
        Check(result.StaticAssertions.All(a => a.Evaluation.Status == ConstantEvaluationStatus.Success));
        Equal(2, result.StaticAssertions[0].Evaluation.MaximumCallDepth);
        AnalysisResult constLocal = Success("constexpr int f(int x){const int local=x+1; return local;} static_assert(f(4)==5); const int n=3; static_assert(n==3);");
        Check(constLocal.StaticAssertions.All(a => a.Evaluation.Value == 1));
        AnalysisResult declared = Success("constexpr int f(); constexpr int f(){return 2;} static_assert(f()==2);");
        Same(declared.Declarations[0].Symbol, declared.Declarations[1].Symbol);
        AnalysisResult discarded = Success("int h(); constexpr int f(){int x; x; (x); h; return 1;} static_assert(f()==1);");
        Equal(ConstantEvaluationStatus.Success, discarded.StaticAssertions[0].Evaluation.Status);
        var interpreter = new ConstantInterpreter(new AnalysisOptions());
        Expression first = result.StaticAssertions[0].Expression;
        Equal(ConstantEvaluationStatus.Success, interpreter.Evaluate(first).Status);
        Equal(ConstantEvaluationStatus.Success, interpreter.Evaluate(discarded.StaticAssertions[0].Expression).Status);
        AnalysisResult failure = Analyze("constexpr int bad(int x){int y; return y;} constexpr int outer(int x){return bad(x);} static_assert(outer(1));");
        Equal(ConstantEvaluationStatus.UninitializedRead, interpreter.Evaluate(failure.StaticAssertions[0].Expression).Status);
        ConstantEvaluationResult reused = interpreter.Evaluate(first);
        Equal(ConstantEvaluationStatus.Success, reused.Status);
        Equal(result.StaticAssertions[0].Evaluation.Steps, reused.Steps);
    }
    internal static void ConstexprFailures()
    {
        foreach (var (source, status) in new[]
        {
            (Fixture("m4-mutable-global.cpp"), ConstantEvaluationStatus.NotConstant),
            ("constexpr int f(){int value; return value;} static_assert(f()==0);", ConstantEvaluationStatus.UninitializedRead),
            ("constexpr int f(){int value=value; return value;} static_assert(f()==0);", ConstantEvaluationStatus.UninitializedRead),
            ("int f(){return 1;} static_assert(f()==1);", ConstantEvaluationStatus.NotConstant),
            ("constexpr int f(); static_assert(f()==1); constexpr int f(){return 1;}", ConstantEvaluationStatus.NotConstant),
            ("static_assert(2147483647+1==0);", ConstantEvaluationStatus.UndefinedBehavior),
            ("static_assert((-2147483647-1)/-1==0);", ConstantEvaluationStatus.UndefinedBehavior),
            ("static_assert((-2147483647-1)%-1==0);", ConstantEvaluationStatus.UndefinedBehavior),
            ("static_assert(-(-2147483647-1)==0);", ConstantEvaluationStatus.UndefinedBehavior),
            ("static_assert(1/0==0);", ConstantEvaluationStatus.UndefinedBehavior)
        })
        {
            AnalysisResult invalid = Analyze(source);
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, source);
            Equal(status, invalid.StaticAssertions[0].Evaluation.Status);
            Check(invalid.Diagnostics.Any(d => d.Code == "CEX001"));
        }
        Check(Analyze("static_assert(false);").Diagnostics.Any(d => d.Code == "CEX002"));
        Check(Analyze("constexpr int value=1/0;").Diagnostics.Any(d => d.Code == "CEX001"));
        Check(Analyze("constexpr int f(); int f(){return 1;}").Diagnostics.Any(d => d.Code == "CEX003"));
        foreach (string invalid in new[] { "constexpr using I=int;", "constexpr typedef int I;" })
            Check(Analyze(invalid).Diagnostics.Any(d => d.Code == "CEX004"));
        AnalysisResult lateConst = Success("constexpr int f(); const int value=f(); constexpr int f(){return 1;} static_assert(value==1);");
        Equal(ConstantEvaluationStatus.NotConstant, Variable(lateConst, "value").ConstantEvaluation!.Status);
        Equal(ConstantEvaluationStatus.Success, lateConst.StaticAssertions.Single().Evaluation.Status);
    }
    internal static void EvaluationLimits()
    {
        using var shallow = new CompilationSession(new AnalysisOptions { ConstantEvaluationCallLimit = 8 });
        AnalysisResult recursive = shallow.Analyze("template<class T> constexpr T recur(T x){return recur(x+1);} static_assert(recur(0)==0);");
        Check(!recursive.IsSuccess && !recursive.ContainsUnsupportedFeatures);
        Equal(ConstantEvaluationStatus.ResourceLimit, recursive.StaticAssertions[0].Evaluation.Status);
        Equal(8, recursive.StaticAssertions[0].Evaluation.MaximumCallDepth);
        Equal(1, Template(recursive, "recur").Instances.Count);
        Equal(1, Template(recursive, "recur").Instances[0].BodyReplayCount);
        using var steps = new CompilationSession(new AnalysisOptions { ConstantEvaluationStepLimit = 3 });
        AnalysisResult costly = steps.Analyze("static_assert(1+2+3==6);");
        Equal(ConstantEvaluationStatus.ResourceLimit, costly.StaticAssertions[0].Evaluation.Status);
        using var instantiation = new CompilationSession(new AnalysisOptions { TemplateInstantiationDepthLimit = 1 });
        AnalysisResult deep = instantiation.Analyze("template<class T> T id(T x){return x;} template<class T> T wrap(T x){return id(x);} int run(){return wrap(1);}");
        Check(deep.Diagnostics.Any(d => d.Code == "TMP003"));
        Throws<ArgumentOutOfRangeException>(() => new CompilationSession(new AnalysisOptions { ConstantEvaluationStepLimit = 0 }));
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Scope> DiscardTemplateResult(CompilationSession session)
    {
        AnalysisResult result = session.Analyze("template<class T> T id(T x){return x;}");
        Check(result.IsSuccess);
        return new WeakReference<Scope>(result.GlobalScope);
    }
    internal static void TemplateLifetime()
    {
        using var session = new CompilationSession();
        var weak = DiscardTemplateResult(session);
        for (int i = 0; i < 3; ++i) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
        Check(!weak.TryGetTarget(out _), "Session canonical types retained a dropped template translation unit.");
        AnalysisResult first = session.Analyze("template<class T> T id(T x){return x;} int run(){return id(1);}");
        AnalysisResult second = session.Analyze("template<class T> T id(T x){return x;} int run(){return id(1);}");
        Check(first.IsSuccess && second.IsSuccess);
        Check(!ReferenceEquals(Template(first, "id").Symbol, Template(second, "id").Symbol));
        Check(!ReferenceEquals(Template(first, "id").TypeParameters[0].Type, Template(second, "id").TypeParameters[0].Type));
        Check(!ReferenceEquals(Template(first, "id").Instances[0].Symbol, Template(second, "id").Instances[0].Symbol));
        Same(Template(first, "id").Instances[0].Symbol.Type, Template(second, "id").Instances[0].Symbol.Type);
        GC.KeepAlive(session);
    }
    internal static void TemplateRollback()
    {
        using var session = new CompilationSession();
        var table = new TypeTable(TargetInfo.Lp64);
        var diagnostics = new DiagnosticBag();
        var owner = new ParseState(session.Lex("template<class T> T id(T x){return x;}").Tokens.CreateCursor(), diagnostics);
        new Parser(owner, table).TranslationUnit();
        FunctionTemplateDeclaration template = owner.GlobalScope.FindLocal("id")!.TemplateDefinition!;
        int retainedChildren = template.TemplateScope.Children.Count;
        var checkpoint = owner.Checkpoint();
        var replay = new ParseState(session.Lex("int run(){return id(1);}").Tokens.CreateCursor(), diagnostics, owner.GlobalScope, owner);
        Throws<InvalidOperationException>(() => replay.Checkpoint());
        Throws<InvalidOperationException>(() => replay.Restore(checkpoint));
        new Parser(replay, table).TranslationUnit();
        Equal(0, diagnostics.Count);
        FunctionTemplateInstance instance = template.Instances.Single();
        Equal(TemplateInstanceState.Complete, instance.State);
        owner.Restore(checkpoint);
        Equal(0, template.Instances.Count);
        Equal(TemplateInstanceState.NotInstantiated, instance.State);
        Check(instance.Declaration == null && instance.Symbol.Definition == null);
        Equal(0, instance.BodyReplayCount);
        Equal(0, instance.Symbol.Declarations.Count);
        Equal(retainedChildren, template.TemplateScope.Children.Count);
        Check(owner.GlobalScope.FindLocal("run") == null);
        Same(template, owner.GlobalScope.FindLocal("id")!.TemplateDefinition);
    }
    internal static void TemplateBoundaries()
    {
        foreach (string source in new[]
        {
            "template<class T> T id(T);", "template<class T=int> T id(T x){return x;}",
            "template<int N> int value(){return N;}", "template<class... T> int count(){return 0;}",
            "template<class> int f(int x){return x;}",
            "template<class T> T* id(T* x){return x;}", "template<class T> T id(T& x){return x;}",
            "template<class T> T id(T x){return x;} int run(){return id<int>(1);}",
            "template<class T> T id(T x){return missing(x);}",
            "template<class T> T id(T x){return x;} template<class U> U id(U x){return x;}",
            "static_assert(true, \"message\");", "constexpr void empty(){return;}",
            "template<class T> constexpr void noop(T x){return;} constexpr int f(){noop(1);return 1;} static_assert(f()==1);",
            "int f(); static_assert(f);", "template<class T> T id(T x){return x;} int f(){id; return 0;}"
        })
        {
            AnalysisResult result = Analyze(source);
            Check(!result.IsSuccess && result.ContainsUnsupportedFeatures, source);
        }
    }
    internal static void Shadowing()
    {
        AnalysisResult result = Success("int f(int x) { { long x = 1; x + 2; } return x; }");
        FunctionDeclaration f = Function(result, "f");
        var nested = As<BlockStatement>(f.Body!.Statements[0]);
        Same(f.Parameters[0].Symbol, As<ReferenceExpression>(As<ConversionExpression>(ReturnValue(f)).Operand).ReferencedSymbol);
        Equal(IntegerKind.Long, Kind(nested.Scope.FindLocal("x")!.Type));
        Check(!Analyze("int f(int x) { int x; return x; }").IsSuccess);
        Check(!Analyze("int f(int x, int x) { return x; }").IsSuccess);
    }
    internal static void LiteralTypes()
    {
        AnalysisResult result = Success("long a=2147483648; unsigned int b=0xffffffff; unsigned long c=0xffffffffffffffff; long long d=1LL; unsigned long long e=1uLL; int f=0b1010'0011; int g=077;");
        Equal(IntegerKind.Long, Kind(Variable(result, "a").Initializer!.Type));
        Equal(IntegerKind.UnsignedInt, Kind(Variable(result, "b").Initializer!.Type));
        Equal(IntegerKind.UnsignedLong, Kind(Variable(result, "c").Initializer!.Type));
        Equal(IntegerKind.LongLong, Kind(Variable(result, "d").Initializer!.Type));
        Equal(new BigInteger(163), Variable(result, "f").Initializer!.ConstantValue!.Value);
        Equal(new BigInteger(63), Variable(result, "g").Initializer!.ConstantValue!.Value);
        foreach (string literal in new[] { "0x", "09", "0b2", "12lul", "1lL", "1''2", "0x'1" })
            Check(!Analyze($"int value={literal};").IsSuccess, literal);
    }
    internal static void Conversions()
    {
        AnalysisResult result = Success("long f(short x) { return x + 1; } bool b=256; unsigned char c=258; int s=4294967295u;");
        var conversion = As<ConversionExpression>(ReturnValue(Function(result, "f")));
        Equal(ConversionKind.IntegralConversion, conversion.Conversion);
        var binary = As<BinaryExpression>(conversion.Operand);
        Equal(ConversionKind.IntegralPromotion, As<ConversionExpression>(binary.Left).Conversion);
        Equal(ConversionKind.LValueToRValue, As<ConversionExpression>(As<ConversionExpression>(binary.Left).Operand).Conversion);
        Equal(BigInteger.One, Variable(result, "b").Initializer!.ConstantValue!.Value);
        Equal(new BigInteger(2), Variable(result, "c").Initializer!.ConstantValue!.Value);
        Equal(new BigInteger(-1), Variable(result, "s").Initializer!.ConstantValue!.Value);
    }
    internal static void ArithmeticConversions()
    {
        AnalysisResult result = Success("long a=1L+1u; unsigned long long b=1LL+1UL; long long c=1LL+1L; int d=true+false; bool e=1u<2L;");
        Equal(IntegerKind.Long, Kind(Variable(result, "a").Initializer!.Type));
        Equal(IntegerKind.UnsignedLongLong, Kind(Variable(result, "b").Initializer!.Type));
        Equal(IntegerKind.LongLong, Kind(Variable(result, "c").Initializer!.Type));
        Equal(IntegerKind.Int, Kind(Variable(result, "d").Initializer!.Type));
        Equal(IntegerKind.Long, Kind(As<BinaryExpression>(Variable(result, "e").Initializer).Left.Type));
        var table = new TypeTable(TargetInfo.Lp64);
        Check(!ReferenceEquals(table.Integer(IntegerKind.Long), table.Integer(IntegerKind.LongLong)));
        AnalysisResult chains = Success("long fromShort(short x){return x+1L;} long fromBool(bool x){return x+1L;}");
        foreach (string name in new[] { "fromShort", "fromBool" })
        {
            var binary = As<BinaryExpression>(ReturnValue(Function(chains, name)));
            var common = As<ConversionExpression>(binary.Left);
            Equal(ConversionKind.IntegralConversion, common.Conversion);
            Equal(IntegerKind.Long, Kind(common.Type));
            var promotion = As<ConversionExpression>(common.Operand);
            Equal(ConversionKind.IntegralPromotion, promotion.Conversion);
            Equal(IntegerKind.Int, Kind(promotion.Type));
            Equal(ConversionKind.LValueToRValue, As<ConversionExpression>(promotion.Operand).Conversion);
        }
    }
    internal static void Precedence()
    {
        AnalysisResult result = Success("int a=2+3*4; int b=9-3-1; bool c=2+3==5; int f(int x){return (x);}");
        Equal(new BigInteger(14), Variable(result, "a").Initializer!.ConstantValue!.Value);
        Equal(new BigInteger(5), Variable(result, "b").Initializer!.ConstantValue!.Value);
        Equal(BigInteger.One, Variable(result, "c").Initializer!.ConstantValue!.Value);
        var returned = As<ConversionExpression>(ReturnValue(Function(result, "f")));
        Equal(ValueCategory.LValue, As<ParenthesizedExpression>(returned.Operand).ValueCategory);
        AnalysisResult assignment = Success("constexpr int f(){int x; (x=1)=2; return x;} static_assert(f()==2);");
        AssignmentExpression outer = As<AssignmentExpression>(As<ExpressionStatement>(Function(assignment, "f").Body!.Statements[1]).Expression);
        Equal(ValueCategory.LValue, outer.ValueCategory);
        Equal(ValueCategory.LValue, As<ParenthesizedExpression>(outer.Target).Operand.ValueCategory);
        Equal(ConstantEvaluationStatus.Success, assignment.StaticAssertions.Single().Evaluation.Status);
    }
    internal static void TargetConstants()
    {
        AnalysisResult result = Success("unsigned int a=4294967295u+1u; unsigned int b=-1u; int c=2147483647+1; int d=1/0; long e=-9223372036854775807L-1L; int f=-7%3;");
        Equal(BigInteger.Zero, Variable(result, "a").Initializer!.ConstantValue!.Value);
        Equal(BigInteger.Parse("4294967295"), Variable(result, "b").Initializer!.ConstantValue!.Value);
        Check(Variable(result, "c").Initializer!.ConstantValue == null);
        Check(Variable(result, "d").Initializer!.ConstantValue == null);
        Equal(BigInteger.Parse("-9223372036854775808"), Variable(result, "e").Initializer!.ConstantValue!.Value);
        Equal(new BigInteger(-1), Variable(result, "f").Initializer!.ConstantValue!.Value);
    }
    internal static void ReturnsAndCalls()
    {
        AnalysisResult result = Success("void empty() { return; } void forward() { return empty(); } int id(int x) { return x; } int f(){return (id)(3);}");
        Equal(TypeKind.Void, ReturnValue(Function(result, "forward")).Type.Kind);
        Equal("id", As<CallExpression>(ReturnValue(Function(result, "f"))).SelectedFunction.Name);
        foreach (string text in new[] { "void f(){return 1;}", "int f(){return;}", "int f(int); int g(){return f();}",
            "int f(int); int g(){return f(1,2);}", "int g(){int x=1; return x();}", "void f(); int g(){return f();}" })
            Check(!Analyze(text).IsSuccess, text);
    }
    internal static void Recovery()
    {
        AnalysisResult result = Analyze("int f(){return 1+; int next=2; return next;} int after(){return 3;}");
        Check(!result.IsSuccess);
        Check(Function(result, "f").Body!.Statements[0] is ErrorStatement);
        Check(result.GlobalScope.FindLocal("after") != null);
        Check(!Analyze("int f(){return 1;").IsSuccess);
        Check(!Analyze("int f( { return 1; }").IsSuccess);
    }
    internal static void UnsupportedBoundaries()
    {
        foreach (string source in new[]
        {
            "struct S {int x;};", "float f;",
            "int **p;", "int a[2][3];", "int f(){return sizeof(float);}", "int f(){return 1.0;}",
            "int f(){int a=1; a<<=2; return a;}", "int f(){int a=1; a++; return a;}",
            "int f(){return 1<<2;}", "int f(){return true?1:2;}",
            "int f(){return (1,2);}", "int f(){return (int)1;}", "int f(){return int(1);}", "int a{1};", "const void f();",
            "void main(){}", "bool main(){return true;}", "int main(){return main();}", "int main;", "int main=1;",
            "int f(); bool g(){return f;}", "int f(); bool g(){return !f;}", "int g(){return 1<=>2;}", "int n=1_custom;"
            , "int f(){ label: return 1; }", "int f(){return ::f();}", "int value(1);"
        })
        {
            AnalysisResult result = Analyze(source);
            Check(!result.IsSuccess && result.ContainsUnsupportedFeatures, source);
        }
        Equal(BigInteger.Zero, Variable(Success("int n=1 bitand 2;"), "n").Initializer!.ConstantValue!.Value);
        AnalysisResult logical = Success("constexpr bool g(){return 1 and 2 or false;} static_assert(g());");
        BinaryExpression disjunction = As<BinaryExpression>(ReturnValue(Function(logical, "g")));
        Equal(TokenKind.OrOr, disjunction.Operator);
        Equal(TokenKind.AndAnd, As<BinaryExpression>(disjunction.Left).Operator);
        Equal(IntegerKind.Bool, Kind(disjunction.Type));
        Equal(BigInteger.One, logical.StaticAssertions.Single().Evaluation.Value!.Value);
        Check(Success("int f(){int main=1; return main;}").IsSuccess);
    }
    internal static void InvalidDeclarations()
    {
        foreach (string source in new[] { "int value; int value;", "using I=int; int I;", "void value;", "const int value;",
            "unsigned signed int value;", "long short value;", "bool int value;", "int int value;", "int f(void x);" })
            Check(!Analyze(source).IsSuccess, source);
    }
}
