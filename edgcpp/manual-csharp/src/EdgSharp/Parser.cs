// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten bounded port of EDG decls.c declaration/scan_nonmember_declaration,
// enter_local_symbol; func_def.c function_definition; statements.c
// compound_statement_full/return_statement; lookup.c normal_id_lookup.
// Declaration insertion, name lookup, expressions and conversions run together.
namespace EdgSharp;

internal sealed partial class Parser(ParseState state, TypeTable types, AnalysisOptions? analysisOptions = null)
{
    private readonly AnalysisOptions options = analysisOptions ?? new AnalysisOptions();
    private FunctionTemplateDeclaration? activeTemplate;
    private SemanticType[]? activeTemplateArguments;
    private bool prototypeAnalysis;
    private int instantiationDepth;
    private Token Current => state.Tokens.Current;
    private Token Take() => state.Tokens.Advance();
    private bool Is(string spelling) => Current.Spelling == spelling;
    private bool Is(TokenKind kind) => Current.Kind == kind;
    private bool Eat(TokenKind kind) { if (!Is(kind)) return false; Take(); return true; }
    private bool Eat(string spelling) { if (!Is(spelling)) return false; Take(); return true; }
    private sealed class ParseFailure : Exception { }
    private Token Expect(TokenKind kind, string description)
    {
        if (Is(kind)) return Take();
        Fail("PAR001", $"Expected {description}.", Current.Range);
        throw new ParseFailure();
    }
    private void Fail(string code, string message, SourceRange range) => state.Diagnostics.Add(code, message, range);
    private void Unsupported(string message, SourceRange? range = null)
        => state.Diagnostics.Add("UNS100", message, range ?? Current.Range, DiagnosticClass.UnsupportedFeature);
    private void UnsupportedAndThrow(string message)
    {
        Unsupported(message);
        throw new ParseFailure();
    }
    internal void TranslationUnit()
    {
        while (!Is(TokenKind.EndOfSource))
        {
            int before = state.Tokens.Index;
            try
            {
                if (Is("static_assert")) StaticAssertion();
                else if (!Eat(TokenKind.Semicolon)) Declaration(allowFunction: true);
            }
            catch (ParseFailure) { Recover(topLevel: true); }
            if (before == state.Tokens.Index) Take();
        }
    }
    private bool StartsDeclaration() => Is("using") || Is("typedef") || Is("const") || Is("constexpr") || IsIntegerKeyword(Current.Spelling) ||
        Is("void") || Current.Kind == TokenKind.Identifier && SoleVisibleSymbol(Current.Spelling)?.Kind is SymbolKind.TypeAlias or SymbolKind.TemplateParameter;
    private static bool IsIntegerKeyword(string spelling) => spelling is "bool" or "char" or "short" or "int" or "long" or "signed" or "unsigned" or "wchar_t" or "char8_t" or "char16_t" or "char32_t";
    private Declaration Declaration(bool allowFunction)
    {
        SourceRange start = Current.Range;
        if (Is("template"))
        {
            if (!allowFunction) UnsupportedAndThrow("Block-scope templates are not implemented.");
            return TemplateDeclaration();
        }
        bool isConstexpr = Eat("constexpr");
        if (isConstexpr && (Is("using") || Is("typedef")))
            Fail("CEX004", "constexpr cannot be applied to an alias declaration.", Current.Range);
        if (Eat("using"))
        {
            if (!Is(TokenKind.Identifier)) UnsupportedAndThrow("Only simple alias declarations ('using Name = type;') are implemented.");
            Token name = Take();
            Expect(TokenKind.Equal, "'=' in alias declaration");
            SemanticType target = ScanType();
            Token end = ExpectSimpleSemicolon();
            return EnterAlias(name, target, SourceRange.Cover(start, end.Range));
        }
        bool typedef = Eat("typedef");
        SemanticType type = ScanType();
        if (Is(TokenKind.Star) || Is(TokenKind.Ampersand) || Is(TokenKind.AndAnd) || Is(TokenKind.LeftParen))
            UnsupportedAndThrow("Pointer, reference and parenthesized declarators are not implemented.");
        Token identifier = Expect(TokenKind.Identifier, "a declarator identifier");
        if (Is(TokenKind.LeftParen))
        {
            if (typedef) UnsupportedAndThrow("Function typedef declarators are not implemented.");
            if (!allowFunction) UnsupportedAndThrow("Block-scope function declarations and definitions are not implemented.");
            return FunctionDeclaration(start, identifier, type, isConstexpr);
        }
        if (typedef)
        {
            Token end = ExpectSimpleSemicolon();
            return EnterAlias(identifier, type, SourceRange.Cover(start, end.Range));
        }
        if (type.Kind == TypeKind.Void) Fail("SEM001", "A variable cannot have void type.", identifier.Range);
        if (isConstexpr) type = types.Const(type);
        Symbol variable = NewLocalSymbol(identifier, SymbolKind.Variable, type);
        // The declarator's point of declaration precedes its initializer, as in EDG's id_linkage path.
        var declaration = new VariableDeclaration(variable, type, SourceRange.Cover(start, identifier.Range), identifier.Range) { IsConstexpr = isConstexpr };
        state.AddDeclaration(declaration);
        if (Eat(TokenKind.Equal))
        {
            bool saved = constantRequiredParsing;
            constantRequiredParsing = type.Qualifiers.HasFlag(TypeQualifiers.Const);
            try { declaration.Initializer = Convert(ScanExprFull(), type); }
            finally { constantRequiredParsing = saved; }
        }
        else if (type.Qualifiers.HasFlag(TypeQualifiers.Const)) Fail("SEM002", "A const scalar variable requires an initializer.", identifier.Range);
        Token semicolon = ExpectSimpleSemicolon();
        declaration.Range = SourceRange.Cover(start, semicolon.Range);
        EvaluateVariableInitializer(declaration);
        return declaration;
    }
    private Token ExpectSimpleSemicolon()
    {
        if (Is(TokenKind.Comma)) UnsupportedAndThrow("Multiple declarators in one declaration are not implemented.");
        if (Is(TokenKind.LeftBracket) || Is(TokenKind.LeftBrace) || Is(TokenKind.LeftParen))
            UnsupportedAndThrow("Array declarators and direct/list initialization are not implemented.");
        if (Is(TokenKind.Scope) || Is(TokenKind.Star) || Is(TokenKind.Ampersand))
            UnsupportedAndThrow("Qualified and compound declarators are not implemented.");
        return Expect(TokenKind.Semicolon, "';'");
    }
    private AliasDeclaration EnterAlias(Token name, SemanticType type, SourceRange range)
    {
        Symbol? previous = state.CurrentScope.FindLocal(name.Spelling);
        Symbol symbol;
        if (previous != null && previous.Kind == SymbolKind.TypeAlias && ReferenceEquals(previous.Type, type)) symbol = previous;
        else symbol = NewLocalSymbol(name, SymbolKind.TypeAlias, type);
        var declaration = new AliasDeclaration(symbol, type, range, name.Range);
        state.AddDeclaration(declaration);
        return declaration;
    }
    private Symbol NewLocalSymbol(Token identifier, SymbolKind kind, SemanticType type)
    {
        if (kind == SymbolKind.Variable && state.CurrentScope.Kind == ScopeKind.TranslationUnit && identifier.Spelling == "main")
            Unsupported("The special global name rules for main are not implemented.", identifier.Range);
        var symbol = new Symbol(identifier.Spelling, kind, type, state.CurrentScope);
        for (Scope? scope = state.CurrentScope; scope != null; scope = scope.Parent)
            if (scope.Kind == ScopeKind.Template && scope.FindLocal(symbol.Name)?.Kind == SymbolKind.TemplateParameter)
            {
                Fail("TMP001", $"A declaration cannot redeclare template parameter '{symbol.Name}'.", identifier.Range);
                break;
            }
        NameBinding? binding = state.CurrentScope.FindLocalBinding(symbol.Name);
        if (binding != null && (!NameBinding.IsFunction(symbol) || binding.Symbols.Any(s => !NameBinding.IsFunction(s))))
            Fail("SEM003", $"Conflicting declaration of '{symbol.Name}' in this scope.", identifier.Range);
        else state.Enter(symbol);
        return symbol;
    }
    private SemanticType ScanType()
    {
        SourceRange start = Current.Range;
        int qualifiers = 0;
        bool isUnsigned = false, isSigned = false, isShort = false, isInt = false, isChar = false, isBool = false;
        int longs = 0, count = 0;
        IntegerKind? distinctCharacter = null;
        while (true)
        {
            string s = Current.Spelling;
            if (s == "const")
            {
                if (++qualifiers > 1) Fail("SEM004", "Repeated const qualifier.", Current.Range);
                Take();
                continue;
            }
            if (!IsIntegerKeyword(s)) break;
            bool repeated = s switch
            {
                "unsigned" => isUnsigned,
                "signed" => isSigned,
                "short" => isShort,
                "int" => isInt,
                "char" => isChar,
                "bool" => isBool,
                "long" => longs >= 2,
                _ => false
            };
            if (repeated) Fail("SEM005", $"Repeated type specifier '{s}'.", Current.Range);
            if (s is "wchar_t" or "char8_t" or "char16_t" or "char32_t")
                distinctCharacter = s switch { "wchar_t" => IntegerKind.WChar, "char8_t" => IntegerKind.Char8, "char16_t" => IntegerKind.Char16, _ => IntegerKind.Char32 };
            switch (s)
            {
                case "unsigned": isUnsigned = true; break;
                case "signed": isSigned = true; break;
                case "short": isShort = true; break;
                case "int": isInt = true; break;
                case "char": isChar = true; break;
                case "bool": isBool = true; break;
                case "long": ++longs; break;
            }
            ++count;
            Take();
        }
        SemanticType type;
        if (count != 0)
        {
            bool invalid = isUnsigned && isSigned || isShort && longs != 0 || isChar && (isShort || longs != 0 || isInt || isBool) ||
                isBool && (count != 1) || longs > 2 || distinctCharacter != null && count != 1;
            if (invalid)
            {
                Fail("SEM006", "Invalid combination of scalar type specifiers.", start);
                type = types.Error;
            }
            else
            {
                IntegerKind kind = distinctCharacter ?? (isBool ? IntegerKind.Bool : isChar ? isUnsigned ? IntegerKind.UnsignedChar : isSigned ? IntegerKind.SignedChar : IntegerKind.Char :
                    isShort ? isUnsigned ? IntegerKind.UnsignedShort : IntegerKind.Short :
                    longs >= 2 ? isUnsigned ? IntegerKind.UnsignedLongLong : IntegerKind.LongLong :
                    longs == 1 ? isUnsigned ? IntegerKind.UnsignedLong : IntegerKind.Long :
                    isUnsigned ? IntegerKind.UnsignedInt : IntegerKind.Int);
                type = types.Integer(kind);
            }
        }
        else if (Eat("void")) type = types.Void;
        else if (Current.Kind == TokenKind.Identifier)
        {
            Token identifier = Take();
            Symbol? alias = SoleVisibleSymbol(identifier.Spelling);
            if (alias?.Kind is SymbolKind.TypeAlias or SymbolKind.TemplateParameter) type = SubstituteActiveType(alias.Type);
            else
            {
                Fail("SEM007", $"'{identifier.Spelling}' does not name a known type.", identifier.Range);
                throw new ParseFailure();
            }
        }
        else
        {
            if (Current.Kind == TokenKind.Keyword || Is(TokenKind.Hash)) Unsupported("This declaration specifier or preprocessing directive is not implemented.");
            else Fail("PAR002", "Expected a type specifier.", Current.Range);
            throw new ParseFailure();
        }
        while (Eat("const")) ++qualifiers;
        if (qualifiers > 0)
        {
            if (type.Kind == TypeKind.Void) Unsupported("Const-qualified void types are not implemented.", start);
            else type = types.Const(type);
        }
        if (qualifiers > 1) Fail("SEM004", "Repeated const qualifier.", start);
        return type;
    }
    private (Scope Scope, VariableDeclaration[] Parameters) FunctionParameters()
    {
        Take(); // '('
        Scope parameterScope = state.PushScope(ScopeKind.Function);
        var parameters = new List<VariableDeclaration>();
        try
        {
            if (Is("void") && state.Tokens.Peek().Kind == TokenKind.RightParen) Take();
            else if (!Is(TokenKind.RightParen))
            {
                while (true)
                {
                    if (Is(TokenKind.Ellipsis)) UnsupportedAndThrow("Variadic function declarations are not implemented.");
                    if (!StartsDeclaration()) UnsupportedAndThrow("Parenthesized direct initialization and non-scalar parameter declarators are not implemented.");
                    SourceRange parameterStart = Current.Range;
                    SemanticType type = ScanType();
                    SourceRange typeRange = SourceRange.Cover(parameterStart, state.Tokens.Previous.Range);
                    Token? name = Is(TokenKind.Identifier) ? Take() : null;
                    if (Is(TokenKind.Star) || Is(TokenKind.Ampersand) || Is(TokenKind.AndAnd) || Is(TokenKind.LeftBracket) || Is(TokenKind.LeftParen))
                        UnsupportedAndThrow("Only by-value scalar parameters are implemented.");
                    if (Is(TokenKind.Equal)) UnsupportedAndThrow("Default function arguments are not implemented.");
                    if (type.Kind == TypeKind.Void)
                    {
                        // declarator.c accepts a sole unnamed void parameter
                        // through a typedef/alias as well as the keyword itself.
                        if (parameters.Count == 0 && !name.HasValue && Is(TokenKind.RightParen)) break;
                        Fail("SEM008", "void must be the sole unnamed parameter marker.", parameterStart);
                    }
                    Symbol symbol = name.HasValue ? NewLocalSymbol(name.Value, SymbolKind.Parameter, type) : new Symbol("", SymbolKind.Parameter, type, parameterScope);
                    var parameter = new VariableDeclaration(symbol, type, name.HasValue ? SourceRange.Cover(parameterStart, name.Value.Range) : typeRange,
                        name?.Range ?? parameterStart.File.Range(typeRange.End, 0));
                    state.AddDeclaration(parameter);
                    parameters.Add(parameter);
                    if (!Eat(TokenKind.Comma)) break;
                    if (Is(TokenKind.Ellipsis)) UnsupportedAndThrow("Variadic function declarations are not implemented.");
                }
            }
            Expect(TokenKind.RightParen, "')'");
        }
        finally { state.PopScope(); }
        return (parameterScope, parameters.ToArray());
    }
    private FunctionDeclaration FunctionDeclaration(SourceRange start, Token identifier, SemanticType resultType, bool isConstexpr)
    {
        if (identifier.Spelling == "main") Unsupported("The special declarations and use rules of global main are not implemented.", identifier.Range);
        if (isConstexpr && resultType.Kind == TypeKind.Void) Unsupported("Constexpr void functions are outside the implemented integer evaluator.", identifier.Range);
        Scope enclosing = state.CurrentScope;
        var (parameterScope, parameters) = FunctionParameters();
        var functionType = types.Function(resultType, parameters.Select(p => p.Type));
        NameBinding? binding = enclosing.FindLocalBinding(identifier.Spelling);
        Symbol? previous = binding?.Symbols.FirstOrDefault(s => s.Kind == SymbolKind.Function &&
            ((FunctionType)s.Type).ParameterTypes.SequenceEqual(functionType.ParameterTypes));
        Symbol function;
        if (previous?.Kind == SymbolKind.Function)
        {
            if (ReferenceEquals(previous.Type, functionType)) function = previous;
            else
            {
                Fail("SEM009", $"Return type conflicts with an earlier declaration of '{identifier.Spelling}'.", identifier.Range);
                function = new Symbol(identifier.Spelling, SymbolKind.Function, functionType, enclosing);
            }
        }
        else function = NewLocalSymbol(identifier, SymbolKind.Function, functionType);
        var declaration = new FunctionDeclaration(function, functionType, SourceRange.Cover(start, identifier.Range), identifier.Range, parameters, parameterScope) { IsConstexpr = isConstexpr };
        if (function.Declarations.OfType<FunctionDeclaration>().Any(d => d.IsConstexpr != isConstexpr))
            Fail("CEX003", "Function redeclarations must agree on constexpr.", identifier.Range);
        state.AddDeclaration(declaration);
        if (Is(TokenKind.LeftBrace))
        {
            if (function.Definition != null) Fail("SEM010", $"Redefinition of function '{function.Name}'.", identifier.Range);
            else state.SetDefinition(function, declaration);
            // The outer body shares parameter redeclaration restrictions: there is no extra block scope.
            FunctionDeclaration? saved = state.CurrentFunction;
            state.CurrentFunction = declaration;
            try
            {
                declaration.Body = CompoundStatement(parameterScope);
                declaration.Range = SourceRange.Cover(start, declaration.Body.Range);
            }
            finally { state.CurrentFunction = saved; }
        }
        else
        {
            if (!Is(TokenKind.Semicolon)) UnsupportedAndThrow("Function suffixes, qualifiers and alternate definition forms are not implemented.");
            declaration.Range = SourceRange.Cover(start, Take().Range);
        }
        return declaration;
    }
    private BlockStatement CompoundStatement(Scope? existingScope = null)
    {
        Token start = Expect(TokenKind.LeftBrace, "'{'");
        Scope scope;
        if (existingScope == null) scope = state.PushScope(ScopeKind.Block);
        else
        {
            // Reenter the prototype's retained scope; its symbols are active for the definition body.
            scope = existingScope;
            state.ActivateRetainedScope(scope);
        }
        var statements = new List<Statement>();
        Token end = start;
        try
        {
            while (!Is(TokenKind.RightBrace) && !Is(TokenKind.EndOfSource))
            {
                int before = state.Tokens.Index;
                SourceRange statementStart = Current.Range;
                try { statements.Add(Statement()); }
                catch (ParseFailure)
                {
                    SourceRange recovered = Recover(topLevel: false);
                    statements.Add(new ErrorStatement(SourceRange.Cover(statementStart, recovered)));
                }
                if (before == state.Tokens.Index) Take();
            }
            end = Expect(TokenKind.RightBrace, "'}'");
        }
        finally { state.PopScope(); }
        return new BlockStatement(scope, statements.ToArray(), SourceRange.Cover(start.Range, end.Range));
    }
    private Statement Statement()
    {
        if (Is(TokenKind.LeftBrace)) return CompoundStatement();
        if (Is(TokenKind.Semicolon)) return new EmptyStatement(Take().Range);
        if (Is("return")) return ReturnStatement();
        if (Is("static_assert")) return new StaticAssertStatement(StaticAssertion());
        if (Current.Kind == TokenKind.Identifier && state.Tokens.Peek().Kind == TokenKind.Colon)
            UnsupportedAndThrow("Labeled statements are not implemented.");
        // EDG checks declaration/expression ambiguity using the currently visible type symbol.
        // The implemented grammar excludes cast/function-style ambiguities, which are diagnosed.
        if (StartsDeclaration()) return new DeclarationStatement(Declaration(allowFunction: false));
        if (Current.Kind == TokenKind.Keyword && !Is("true") && !Is("false")) UnsupportedAndThrow("This statement or expression keyword is not implemented.");
        SourceRange start = Current.Range;
        Expression expression = ScanExprFull();
        if (expression.Type.Kind == TypeKind.OverloadSet)
            UnsupportedAndThrow("An overload set is implemented only as the callee of a direct function call.");
        Token end = ExpectExpressionSemicolon();
        return new ExpressionStatement(expression, SourceRange.Cover(start, end.Range));
    }
    private ReturnStatement ReturnStatement()
    {
        Token start = Take();
        FunctionDeclaration function = state.CurrentFunction ?? throw new InvalidOperationException("Return parser requires an active function.");
        Expression? expression = null;
        if (!Is(TokenKind.Semicolon)) expression = Convert(ScanExprFull(), function.Type.ReturnType);
        else if (function.Type.ReturnType.Kind != TypeKind.Void) Fail("SEM011", "A non-void function must return a value.", start.Range);
        Token end = ExpectExpressionSemicolon();
        return new ReturnStatement(expression, SourceRange.Cover(start.Range, end.Range));
    }
    private Token ExpectExpressionSemicolon()
    {
        if (!Is(TokenKind.Semicolon) && IsUnsupportedExpressionContinuation()) UnsupportedAndThrow("This expression operator or expression form is not implemented.");
        return Expect(TokenKind.Semicolon, "';' after expression");
    }
    private bool IsUnsupportedExpressionContinuation() => Current.Kind is TokenKind.Equal or TokenKind.PlusEqual or TokenKind.MinusEqual or
        TokenKind.StarEqual or TokenKind.SlashEqual or TokenKind.PercentEqual or TokenKind.AmpersandEqual or TokenKind.PipeEqual or TokenKind.CaretEqual or
        TokenKind.ShiftLeftEqual or TokenKind.ShiftRightEqual or TokenKind.PlusPlus or TokenKind.MinusMinus or TokenKind.LeftBracket or TokenKind.Question or
        TokenKind.Dot or TokenKind.Arrow or TokenKind.DotStar or TokenKind.ArrowStar or TokenKind.Scope or TokenKind.Comma or TokenKind.ShiftLeft or TokenKind.ShiftRight or TokenKind.AndAnd or TokenKind.OrOr or TokenKind.Spaceship;
    private SourceRange Recover(bool topLevel)
    {
        SourceRange last = Current.Range;
        int braces = 0;
        while (!Is(TokenKind.EndOfSource))
        {
            if (Is(TokenKind.RightBrace) && braces == 0)
            {
                if (topLevel) last = Take().Range;
                return last;
            }
            Token token = Take();
            last = token.Range;
            if (token.Kind == TokenKind.LeftBrace) ++braces;
            if (token.Kind == TokenKind.RightBrace && --braces == 0) return last;
            if (token.Kind == TokenKind.Semicolon && braces == 0) return last;
        }
        return last;
    }
}
