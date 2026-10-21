// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored deterministic tests; no generated fixtures or native oracle dependency.
using System.Collections;
using System.Numerics;
using EdgSharp;

if (args.Length == 2 && args[0] == "--analyze")
{
    using var session = new CompilationSession();
    AnalysisResult result = session.Analyze(File.ReadAllText(args[1]), args[1]);
    Console.WriteLine($"success={result.IsSuccess}; unsupported={result.ContainsUnsupportedFeatures}; declarations={result.Declarations.Count}");
    foreach (Declaration declaration in result.Declarations)
        Console.WriteLine($"declaration={declaration.Name}; type={declaration.Type}; range={declaration.Range.Start}:{declaration.Range.Length}");
    foreach (Diagnostic diagnostic in result.Diagnostics)
        Console.WriteLine($"{diagnostic.Class} {diagnostic.Code} {diagnostic.Range.StartPosition.Line}:{diagnostic.Range.StartPosition.Column} {diagnostic.Message}");
    return result.IsSuccess ? 0 : 1;
}
if (args.Length != 0) { Console.Error.WriteLine("Usage: EdgSharp.Tests [--analyze file.cpp]"); return 2; }

var cases = new (string Name, Action Run)[]
{
    ("lex token spelling and source ranges", Tests.LexicalRanges),
    ("lex maximal punctuation and digraph exception", Tests.Punctuation),
    ("lex malformed comments and strings", Tests.MalformedLexicalInput),
    ("lex cursor checkpoint and retained replay", Tests.TokenReplay),
    ("semantic checkpoint restores all mutable state", Tests.SemanticCheckpoint),
    ("semantic checkpoint restores nested definitions and current functions", Tests.DefinitionCheckpoint),
    ("semantic checkpoint invalidates stale branches atomically", Tests.StaleCheckpoint),
    ("session independence lifecycle and canonical identity", Tests.Sessions),
    ("independent sessions support concurrent analysis", Tests.ConcurrentSessions),
    ("API rejects unsupported profile and null target", Tests.Options),
    ("public graph collections cannot mutate state", Tests.ReadOnlyGraph),
    ("M2 positive source fixture resolves complete graph", Tests.PositiveGraph),
    ("M2 negative and unsupported source fixtures", Tests.FixtureBoundaries),
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

internal static class Tests
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
        Check(!session.Lex("// splice\\\nint x;").IsSuccess);
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
        AnalysisResult unsupported = Analyze(Fixture("m2-unsupported-statement.cpp"));
        Check(!unsupported.IsSuccess && unsupported.ContainsUnsupportedFeatures);
        Check(Function(unsupported, "entry").Body!.Statements.Any(s => s is ErrorStatement));
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
        AnalysisResult assignment = Analyze("int f(){int x; (x=1)=2; return x;}");
        Check(!assignment.IsSuccess && assignment.ContainsUnsupportedFeatures);
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
            "#define X 1\nint x=X;", "template<class T> T id(T x){return x;}", "struct S {int x;};", "float f;",
            "int *p;", "int &r;", "int a[2];", "int f(){return sizeof(int);}", "int f(){return 1.0;}",
            "int f(){return 'a';}", "int f(){int a=1; a=2; return a;}", "int f(){int a=1; a++; return a;}",
            "int f(){return true && false;}", "int f(){return 1<<2;}", "int f(){return true?1:2;}",
            "int f(){return (1,2);}", "int f(){return (int)1;}", "int f(){return int(1);}", "int a{1};", "const void f();",
            "void main(){}", "bool main(){return true;}", "int main(){return main();}", "int main;", "int main=1;",
            "int f(); bool g(){return f;}", "int f(); bool g(){return !f;}", "bool g(){return 1 and 2;}", "int g(){return 1<=>2;}", "int n=1_custom;"
            , "int f(){\n#define V 1\nreturn 0;\n}", "int f(){ label: return 1; }", "int f(){return ::f();}",
            "int\n#define V 1\nx;", "int f(int\n#define V 1\nx);", "int value(1);"
        })
        {
            AnalysisResult result = Analyze(source);
            Check(!result.IsSuccess && result.ContainsUnsupportedFeatures, source);
        }
        Equal(BigInteger.Zero, Variable(Success("int n=1 bitand 2;"), "n").Initializer!.ConstantValue!.Value);
        Check(Success("int f(){int main=1; return main;}").IsSuccess);
    }
    internal static void InvalidDeclarations()
    {
        foreach (string source in new[] { "int value; int value;", "using I=int; int I;", "void value;", "const int value;",
            "unsigned signed int value;", "long short value;", "bool int value;", "int int value;", "int f(void x);" })
            Check(!Analyze(source).IsSuccess, source);
    }
}
