// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored source-text semantic tests. Native controls are separate supervisor
// work; this worker has not executed these or the existing regression harness.
using System.Runtime.CompilerServices;
using System.Numerics;
using EdgSharp;

internal static partial class Tests
{
    private static ReferenceBindingExpression Binding(VariableDeclaration declaration) => As<ReferenceBindingExpression>(declaration.Initializer);
    private static ReferenceType ReferenceParameter(CallExpression call, int index = 0)
        => As<ReferenceType>(call.SelectedCandidate.FunctionType.ParameterTypes[index]);
    private static VariableDeclaration Local(FunctionDeclaration function, string name)
        => function.ParameterScope.Declarations.OfType<VariableDeclaration>().Single(d => d.Name == name);

    internal static void ReferenceIdentity()
    {
        AnalysisResult result = Success("using R=int&; using CR=const int&; using RR=int&&; typedef int & Other; " +
            "int pick(R); int pick(const R x){return x;} long pick(CR); bool pick(RR); " +
            "int object=1; const R alias=object; using LR=RR&; using Collapsed=R&&; using Keep=RR&&; using BothL=R&;");
        FunctionDeclaration[] functions = result.Declarations.OfType<FunctionDeclaration>().Where(d => d.Name == "pick").ToArray();
        Equal(4, functions.Length);
        Equal(3, result.GlobalScope.FindLocalBinding("pick")!.Symbols.Count);
        Same(functions[0].Symbol, functions[1].Symbol);
        Same(functions[0].Type, functions[1].Type);
        Same(functions[1], functions[0].Symbol.Definition);
        Check(!ReferenceEquals(functions[0].Symbol, functions[2].Symbol));
        Check(!ReferenceEquals(functions[0].Symbol, functions[3].Symbol));
        var r = As<ReferenceType>(result.GlobalScope.FindLocal("R")!.Type);
        var cr = As<ReferenceType>(result.GlobalScope.FindLocal("CR")!.Type);
        var rr = As<ReferenceType>(result.GlobalScope.FindLocal("RR")!.Type);
        Same(r, result.GlobalScope.FindLocal("Other")!.Type);
        Same(r, result.GlobalScope.FindLocal("LR")!.Type);
        Same(r, result.GlobalScope.FindLocal("Collapsed")!.Type);
        Same(r, result.GlobalScope.FindLocal("BothL")!.Type);
        Same(rr, result.GlobalScope.FindLocal("Keep")!.Type);
        Same(r, Variable(result, "alias").Type);
        Equal(TypeQualifiers.None, r.Qualifiers);
        Equal(TypeQualifiers.None, r.ReferredType.Qualifiers);
        Equal(TypeQualifiers.Const, cr.ReferredType.Qualifiers);
        Same(r, functions[0].Type.ParameterTypes[0]);
        Same(cr, functions[2].Type.ParameterTypes[0]);
        Same(rr, functions[3].Type.ParameterTypes[0]);
        AnalysisResult conflict = Analyze("int& result(int&); const int& result(int&);");
        Check(!conflict.IsSuccess && conflict.Diagnostics.Any(d => d.Code == "SEM009"));
        Equal(1, conflict.GlobalScope.FindLocalBinding("result")!.Symbols.Count);
        Same(conflict.Declarations[0].Symbol, conflict.GlobalScope.FindLocal("result"));
        Check(!ReferenceEquals(conflict.Declarations[0].Symbol, conflict.Declarations[1].Symbol));
    }

    internal static void ReferenceBindings()
    {
        AnalysisResult result = Success("int object=3; const int fixed=4; int& writable=object; const int& read=object; " +
            "const int& immutable=fixed; int& again=writable; int load(int& p){int& local=p; {int object=6; int& inner=object;} return local;}");
        foreach (string name in new[] { "writable", "read", "immutable", "again" })
        {
            VariableDeclaration variable = Variable(result, name);
            ReferenceBindingExpression binding = Binding(variable);
            Same(variable.Type, binding.Binding.ReferenceType);
            Equal(ReferenceBindingKind.Direct, binding.Binding.Kind);
            Check(!binding.Binding.RequiresLValueToRValue && binding.Temporary == null);
            Check(binding.ConstantValue == null && variable.ConstantEvaluation is { Status: ConstantEvaluationStatus.Success, Value: null, ReferenceValue: not null });
            Same(binding.Source, binding.Referent);
            Equal(ReferenceReferentOrigin.StaticObject, binding.ReferentOrigin);
        }
        Same(Variable(result, "object").Symbol, As<ReferenceExpression>(Binding(Variable(result, "writable")).Referent).ReferencedSymbol);
        Check(Binding(Variable(result, "read")).Binding.AddsQualification);
        Check(!Binding(Variable(result, "immutable")).Binding.AddsQualification);
        Same(Variable(result, "writable").Symbol, As<ReferenceExpression>(Binding(Variable(result, "again")).Source).ReferencedSymbol);
        Equal(TypeKind.Integer, Binding(Variable(result, "again")).Source.Type.Kind);
        FunctionDeclaration load = Function(result, "load");
        Equal(ReferenceReferentOrigin.CallerReference, Binding(Local(load, "local")).ReferentOrigin);
        var nested = As<BlockStatement>(load.Body!.Statements[1]);
        var inner = nested.Scope.Declarations.OfType<VariableDeclaration>().Single(d => d.Name == "inner");
        Same(nested.Scope.FindLocal("object")!, As<ReferenceExpression>(Binding(inner).Source).ReferencedSymbol);
        Equal(ReferenceReferentOrigin.AutomaticObject, Binding(inner).ReferentOrigin);
        var converted = As<ConversionExpression>(ReturnValue(load));
        Equal(ConversionKind.LValueToRValue, converted.Conversion);
        Same(Local(load, "local").Symbol, As<ReferenceExpression>(converted.Operand).ReferencedSymbol);
    }

    internal static void ReferenceTemporaries()
    {
        AnalysisResult result = Success("const int& literal=7; int&& movable=8; const long& wide=9; short small=2; " +
            "const int& promoted=small; long source=5; int&& converted=source; const int copy=literal; " +
            "int sink(const int&); int run(){return sink(3);}");
        foreach (string name in new[] { "literal", "movable", "wide", "promoted", "converted" })
        {
            VariableDeclaration variable = Variable(result, name);
            ReferenceBindingExpression binding = Binding(variable);
            MaterializedTemporaryExpression temporary = As<MaterializedTemporaryExpression>(binding.Temporary);
            Same(As<ReferenceType>(variable.Type).ReferredType, temporary.Type);
            Equal(ReferenceTemporaryLifetime.ReferenceVariable, temporary.Lifetime);
            Same(variable.Symbol, temporary.LifetimeOwner);
            Equal(ReferenceReferentOrigin.Temporary, binding.ReferentOrigin);
            Check(temporary.ConstantValue == null && binding.ConstantValue == null && variable.ConstantEvaluation != null);
        }
        Equal(ReferenceBindingKind.MaterializedTemporary, Binding(Variable(result, "literal")).Binding.Kind);
        Equal(StandardConversionRank.ExactMatch, Binding(Variable(result, "movable")).Binding.Rank);
        ReferenceBindingExpression promoted = Binding(Variable(result, "promoted"));
        Equal(ReferenceBindingKind.ConvertedTemporary, promoted.Binding.Kind);
        Equal(StandardConversionRank.Promotion, promoted.Binding.Rank);
        Check(promoted.Binding.RequiresLValueToRValue);
        var promotion = As<ConversionExpression>(promoted.Temporary!.Initializer);
        Equal(ConversionKind.IntegralPromotion, promotion.Conversion);
        Equal(ConversionKind.LValueToRValue, As<ConversionExpression>(promotion.Operand).Conversion);
        Equal(StandardConversionRank.Conversion, Binding(Variable(result, "converted")).Binding.Rank);
        Equal(ConversionKind.IntegralConversion, As<ConversionExpression>(Binding(Variable(result, "wide")).Temporary!.Initializer).Conversion);
        Check(Variable(result, "copy").Initializer!.ConstantValue == null);
        Equal(ConstantEvaluationStatus.Success, Variable(result, "copy").ConstantEvaluation!.Status);
        Equal(new BigInteger(7), Variable(result, "copy").ConstantEvaluation!.Value!.Value);
        CallExpression call = ReturnedCall(result, "run");
        var argument = As<ReferenceBindingExpression>(call.Arguments[0]);
        Equal(ReferenceBindingContext.Argument, argument.Context);
        Equal(ReferenceTemporaryLifetime.FullExpression, argument.Temporary!.Lifetime);
        Check(argument.Temporary.LifetimeOwner == null);
        Same(call.SelectedCandidate.ArgumentConversions[0].ReferenceBinding!, argument.Binding);
        AnalysisResult fromXvalue = Success("long&& source(); const int& converted=source(); const long& direct=source();");
        ReferenceBindingExpression convertedXvalue = Binding(Variable(fromXvalue, "converted"));
        Equal(ValueCategory.XValue, convertedXvalue.Binding.SourceCategory);
        Check(convertedXvalue.Binding.RequiresLValueToRValue);
        var scalarConversion = As<ConversionExpression>(convertedXvalue.Temporary!.Initializer);
        Equal(ConversionKind.LValueToRValue, As<ConversionExpression>(scalarConversion.Operand).Conversion);
        ReferenceBindingExpression directXvalue = Binding(Variable(fromXvalue, "direct"));
        Equal(ReferenceBindingKind.Direct, directXvalue.Binding.Kind);
        Check(directXvalue.Temporary == null && !directXvalue.Binding.RequiresLValueToRValue);
        // Uninitialized self-reference is not a fabricated constant/referent.
        AnalysisResult self = Success("int& self=self; int& again=self;");
        Equal(ReferenceReferentOrigin.Unknown, Binding(Variable(self, "self")).ReferentOrigin);
        Equal(ReferenceReferentOrigin.Unknown, Binding(Variable(self, "again")).ReferentOrigin);
    }

    internal static void ReferenceInvalid()
    {
        foreach (string source in new[]
        {
            "int& missing;", "void& invalid=0;", "using V=void; V&& invalid=0;", "int& invalid=1;",
            "const int object=1; int& invalid=object;", "const int object=1; int&& invalid=object;",
            "int object=1; int&& invalid=object;", "short object=1; int& invalid=object;",
            "int&& first=1; int&& invalid=first;", "const int& first=1; int& invalid=first;",
            "int&& get(); int& invalid=get();", "int& bad(){return 1;}", "int&& bad(int& p){return p;}",
            "const int&& get(); int&& invalid=get();", "const int&& get(); int& invalid=get();",
            "void nothing(); const int& invalid=nothing();", "void nothing(); int& bad(){return nothing();}",
            "void bad(void&);", "int& same(int&); const int& same(int&);",
            "int & & invalid=0;", "int& const invalid=0;", "void&& bad();",
            "int need(int&); int bad(){return need(0);}", "int need(int&&); int bad(int x){return need(x);}",
            "int need(int&); int bad(const int x){return need(x);}"
        })
        {
            AnalysisResult result = Analyze(source);
            Check(!result.IsSuccess && !result.ContainsUnsupportedFeatures, source);
            Check(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error && d.Class == DiagnosticClass.InvalidSource), source);
        }
    }

    internal static void ReferenceOverloads()
    {
        AnalysisResult result = Success("int select(int&); long select(const int&); bool select(int&&); int&& xvalue(); " +
            "int mut(int x){return select(x);} long fixed(const int x){return select(x);} bool temp(){return select(1);} " +
            "bool expiring(){return select(xvalue());} int named(){int&& x=1; return select(x);}");
        CallExpression mutable = ReturnedCall(result, "mut");
        CallExpression immutable = ReturnedCall(result, "fixed");
        CallExpression temporary = ReturnedCall(result, "temp");
        CallExpression xvalue = ReturnedCall(result, "expiring");
        CallExpression named = ReturnedCall(result, "named");
        Equal(TypeQualifiers.None, ReferenceParameter(mutable).ReferredType.Qualifiers);
        Equal(ReferenceKind.LValue, ReferenceParameter(mutable).ReferenceKind);
        Equal(TypeQualifiers.Const, ReferenceParameter(immutable).ReferredType.Qualifiers);
        Equal(ReferenceKind.RValue, ReferenceParameter(temporary).ReferenceKind);
        Same(temporary.SelectedFunction, xvalue.SelectedFunction);
        Same(mutable.SelectedFunction, named.SelectedFunction);
        Same(result.GlobalScope.FindLocalBinding("select")!.Symbols[0], mutable.SelectedFunction);
        Same(result.GlobalScope.FindLocalBinding("select")!.Symbols[1], immutable.SelectedFunction);
        Same(result.GlobalScope.FindLocalBinding("select")!.Symbols[2], temporary.SelectedFunction);
        Equal(2, mutable.Candidates.Count(c => c.IsViable));
        Equal(1, immutable.Candidates.Count(c => c.IsViable));
        Equal(2, temporary.Candidates.Count(c => c.IsViable));
        Equal(ValueCategory.XValue, As<ReferenceBindingExpression>(xvalue.Arguments[0]).Source.ValueCategory);
        Equal(ValueCategory.LValue, As<ReferenceBindingExpression>(named.Arguments[0]).Source.ValueCategory);
        Check(!mutable.SelectedCandidate.ArgumentConversions[0].RequiresLValueToRValue);
        Check(!xvalue.SelectedCandidate.ArgumentConversions[0].RequiresLValueToRValue);
        Check(xvalue.Arguments[0] is ReferenceBindingExpression { Temporary: null });
    }

    internal static void ReferenceRanking()
    {
        AnalysisResult result = Success("int rank(const int&); long rank(int&&); bool rank(const long&); " +
            "int promote(const int&); long promote(const long&); int direct(int&); long direct(long); " +
            "int cv(int&&); long cv(const int&&); int source(long value){return rank(value);} " +
            "int shortcall(short value){return promote(value);} int exact(int value){return direct(value);} " +
            "int samecv(){return cv(1);} const int&& constsource(); long kept(){return cv(constsource());}");
        // Exact const-long& beats the conversion into either int temporary.
        CallExpression rank = ReturnedCall(result, "source");
        Equal(IntegerKind.Long, Kind(ReferenceParameter(rank).ReferredType));
        Equal(StandardConversionRank.ExactMatch, rank.SelectedCandidate.ArgumentConversions[0].Rank);
        // short -> const int& is a promotion; short -> const long& conversion.
        CallExpression promotion = ReturnedCall(result, "shortcall");
        Equal(IntegerKind.Int, Kind(ReferenceParameter(promotion).ReferredType));
        Equal(StandardConversionRank.Promotion, promotion.SelectedCandidate.ArgumentConversions[0].Rank);
        CallExpression exact = ReturnedCall(result, "exact");
        Equal(ReferenceKind.LValue, ReferenceParameter(exact).ReferenceKind);
        CallExpression samecv = ReturnedCall(result, "samecv");
        Equal(TypeQualifiers.None, ReferenceParameter(samecv).ReferredType.Qualifiers);
        Equal(ReferenceKind.RValue, ReferenceParameter(samecv).ReferenceKind);
        Check(!samecv.SelectedCandidate.ArgumentConversions[0].ReferenceBinding!.AddsQualification);
        CallExpression kept = ReturnedCall(result, "kept");
        Equal(TypeQualifiers.Const, kept.Arguments[0].Type.Qualifiers);
        Equal(1, kept.Candidates.Count(candidate => candidate.IsViable));
        Equal(TypeQualifiers.Const, ReferenceParameter(kept).ReferredType.Qualifiers);

        AnalysisResult kindFirst = Success("int choose(const int&&); long choose(const int&); int run(){return choose(1);}");
        Equal(ReferenceKind.RValue, ReferenceParameter(ReturnedCall(kindFirst, "run")).ReferenceKind);
        AnalysisResult converted = Success("int choose(int&&); long choose(const int&); int run(long x){return choose(x);}");
        CallExpression convertedCall = ReturnedCall(converted, "run");
        Equal(ReferenceKind.RValue, ReferenceParameter(convertedCall).ReferenceKind);
        Equal(ReferenceBindingKind.ConvertedTemporary, As<ReferenceBindingExpression>(convertedCall.Arguments[0]).Binding.Kind);
        // Cv must not rank unrelated targets with the same conversion rank.
        AnalysisResult unrelated = Analyze("int choose(const int&); long choose(const long&); int run(unsigned x){return choose(x);}");
        Check(unrelated.Diagnostics.Any(d => d.Code == "SEM026") && !unrelated.ContainsUnsupportedFeatures);
        foreach (string type in new[] { "bool", "char", "signed char", "unsigned char", "short", "unsigned short" })
        {
            AnalysisResult generic = Success("int promote(const int&); long promote(const long&); int run(" + type + " value){return promote(value);}");
            CallExpression call = ReturnedCall(generic, "run");
            Equal(IntegerKind.Int, Kind(ReferenceParameter(call).ReferredType));
            Equal(StandardConversionRank.Promotion, call.SelectedCandidate.ArgumentConversions[0].Rank);
        }
    }

    internal static void ReferenceAmbiguity()
    {
        foreach (string source in new[]
        {
            "int choose(int); long choose(const int&); int run(){return choose(1);}",
            "int choose(int); long choose(int&); int run(int x){return choose(x);}",
            "int choose(int); long choose(int&&); int run(){return choose(1);}",
            "int choose(int&,const int&); long choose(const int&,int&); int run(int x){return choose(x,x);}",
            "int choose(int&&,const int&); long choose(const int&,int&&); int run(){return choose(1,2);}",
            "int choose(const int&,long); long choose(const long&,int); int run(short x){return choose(x,x);}"
        })
        {
            AnalysisResult result = Analyze(source);
            Check(!result.IsSuccess && !result.ContainsUnsupportedFeatures, source);
            Check(result.Diagnostics.Any(d => d.Code == "SEM026"), source);
        }
    }

    internal static void ReferenceCandidateOrder()
    {
        foreach (string declarations in new[]
        {
            "int choose(int&); long choose(const int&); bool choose(int&&);",
            "bool choose(int&&); int choose(int&); long choose(const int&);",
            "long choose(const int&); bool choose(int&&); int choose(int&);",
            "long choose(const int&); int choose(int&); bool choose(int&&);",
            "int choose(int&); bool choose(int&&); long choose(const int&);",
            "bool choose(int&&); long choose(const int&); int choose(int&);"
        })
        {
            AnalysisResult result = Success(declarations + "int lv(int x){return choose(x);} long cv(const int x){return choose(x);} bool rv(){return choose(0);}");
            Equal(IntegerKind.Int, Kind(ReturnedCall(result, "lv").Type));
            Equal(IntegerKind.Long, Kind(ReturnedCall(result, "cv").Type));
            Equal(IntegerKind.Bool, Kind(ReturnedCall(result, "rv").Type));
            Check(ReturnedCall(result, "lv").Candidates.All(c => c.Function.Name == "choose"));
        }
    }

    internal static void ReferenceLookup()
    {
        AnalysisResult result = Success("long select(const int&); long before(int x){return select(x);} " +
            "int select(int&); int after(int x){return select(x);} int shadow(){int select=0; return select;} " +
            "template<class T> long frozen(T x){int n=1; return select(n);} bool select(int&&); long run(){return frozen(1);} long select(const int& x){return x;}");
        CallExpression before = ReturnedCall(result, "before");
        CallExpression after = ReturnedCall(result, "after");
        Equal(1, before.Candidates.Count);
        Equal(2, after.Candidates.Count);
        Check(!ReferenceEquals(before.SelectedFunction, after.SelectedFunction));
        Same(before.SelectedFunction, before.Candidates[0].Function);
        Same(result.Declarations.Last(), before.SelectedFunction.Definition);
        FunctionTemplateDeclaration frozen = Template(result, "frozen");
        CallExpression pattern = As<CallExpression>(As<ConversionExpression>(ReturnValue(frozen.Pattern)).Operand);
        // int& was already visible at template definition; the later int&&
        // entity cannot appear in replay's ordinary candidate snapshot.
        Equal(2, pattern.Candidates.Count);
        CallExpression replay = As<CallExpression>(As<ConversionExpression>(ReturnValue(frozen.Instances.Single().Declaration!)).Operand);
        Equal(2, replay.Candidates.Count);
        Same(after.SelectedFunction, replay.SelectedFunction);
        Same(pattern.SelectedFunction, replay.SelectedFunction);
        AnalysisResult hidden = Analyze("int select(int&); int run(){int select=0; return select(0);}");
        Check(!hidden.IsSuccess && hidden.Diagnostics.Any(d => d.Code == "SEM022"));
        Check(!hidden.ContainsUnsupportedFeatures);
        AnalysisResult dependentCall = Success("int refcall(int&); long refcall(long&); template<class T> T invoke(T x){return refcall(x);} int one(){return invoke(1);} long two(){return invoke(2L);}");
        FunctionTemplateDeclaration invoke = Template(dependentCall, "invoke");
        Equal(2, invoke.Instances.Count);
        foreach (FunctionTemplateInstance instance in invoke.Instances)
        {
            var call = As<CallExpression>(ReturnValue(instance.Declaration!));
            Same(instance.TypeArguments[0], ReferenceParameter(call).ReferredType);
            Equal(ReferenceBindingKind.Direct, As<ReferenceBindingExpression>(call.Arguments[0]).Binding.Kind);
            Equal(1, instance.BodyReplayCount);
        }
        AnalysisResult byValue = Success("template<class T> T copy(T x){return x;} const int object=1; const int& alias=object; int run(){return copy(alias);}");
        CallExpression copied = ReturnedCall(byValue, "run");
        Equal(IntegerKind.Int, Kind(copied.TemplateInstance!.TypeArguments[0]));
        Equal(TypeQualifiers.None, copied.TemplateInstance.TypeArguments[0].Qualifiers);
        Check(copied.Arguments[0] is ConversionExpression { Conversion: ConversionKind.LValueToRValue });
    }

    internal static void ReferenceReturns()
    {
        AnalysisResult result = Success("int object=1; int& lv(int& x){return x;} const int& cv(const int& x){return x;} " +
            "int&& rv(); int& relay(){return lv(object);} const int& readonly(){return cv(object);} int&& rrelay(){return rv();} " +
            "int load(){return rv();} int named(int&& value){return value;} int& local(){int&& value=1; return value;}");
        foreach (string name in new[] { "lv", "cv", "relay", "readonly", "rrelay" })
        {
            var binding = As<ReferenceBindingExpression>(ReturnValue(Function(result, name)));
            Equal(ReferenceBindingContext.Return, binding.Context);
            Check(!binding.Binding.RequiresLValueToRValue && binding.Temporary == null && binding.ConstantValue == null);
            Same(Function(result, name).Type.ReturnType, binding.Binding.ReferenceType);
        }
        var relay = As<CallExpression>(As<ReferenceBindingExpression>(ReturnValue(Function(result, "relay"))).Source);
        Equal(ValueCategory.LValue, relay.ValueCategory);
        Equal(IntegerKind.Int, Kind(relay.Type));
        Same(Function(result, "lv").Type.ReturnType, relay.DeclaredReturnType);
        var readonlyCall = As<CallExpression>(As<ReferenceBindingExpression>(ReturnValue(Function(result, "readonly"))).Source);
        Equal(TypeQualifiers.Const, readonlyCall.Type.Qualifiers);
        Equal(ValueCategory.LValue, readonlyCall.ValueCategory);
        var rrelay = As<CallExpression>(As<ReferenceBindingExpression>(ReturnValue(Function(result, "rrelay"))).Source);
        Equal(ValueCategory.XValue, rrelay.ValueCategory);
        Equal(ReferenceKind.RValue, As<ReferenceType>(rrelay.DeclaredReturnType).ReferenceKind);
        var load = As<ConversionExpression>(ReturnValue(Function(result, "load")));
        Equal(ConversionKind.LValueToRValue, load.Conversion);
        Equal(ValueCategory.XValue, As<CallExpression>(load.Operand).ValueCategory);
        var named = As<ConversionExpression>(ReturnValue(Function(result, "named")));
        Equal(ValueCategory.LValue, named.Operand.ValueCategory);
        Check(named.Operand.ConstantValue == null && load.ConstantValue == null);
    }

    internal static void ReferenceLifetimeHazards()
    {
        AnalysisResult result = Success("int global=1; const int& staticTemporary=2; " +
            "int& local(){int x=1; return x;} int& parameter(int x){return x;} const int& temporary(){return 3;} " +
            "const int& namedTemporary(){const int& first=4; const int& second=first; return second;} " +
            "int& namedRvalueTemporary(){int&& value=5; return value;} " +
            "const int& safe(){return staticTemporary;} int& caller(int& x){return x;}");
        Equal(ReferenceReferentOrigin.AutomaticObject, As<ReferenceBindingExpression>(ReturnValue(Function(result, "local"))).ReferentOrigin);
        Equal(ReferenceReferentOrigin.AutomaticObject, As<ReferenceBindingExpression>(ReturnValue(Function(result, "parameter"))).ReferentOrigin);
        var temporary = As<ReferenceBindingExpression>(ReturnValue(Function(result, "temporary")));
        Equal(ReferenceReferentOrigin.Temporary, temporary.ReferentOrigin);
        Equal(ReferenceTemporaryLifetime.FullExpression, temporary.Temporary!.Lifetime);
        Equal(ReferenceReferentOrigin.CallerReference, As<ReferenceBindingExpression>(ReturnValue(Function(result, "caller"))).ReferentOrigin);
        Equal(5, result.Diagnostics.Count(d => d.Code == "REF006" && d.Severity == DiagnosticSeverity.Warning));
        Check(result.Diagnostics.All(d => d.Severity == DiagnosticSeverity.Warning));
        // A reference to an existing materialized object does not re-extend
        // its lifetime and does not create a second temporary.
        var safe = As<ReferenceBindingExpression>(ReturnValue(Function(result, "safe")));
        Check(safe.Temporary == null);
        Same(Binding(Variable(result, "staticTemporary")).Temporary!, safe.KnownTemporary);
        Same(Variable(result, "staticTemporary").Symbol, As<ReferenceExpression>(safe.Source).ReferencedSymbol);
        FunctionDeclaration namedFunction = Function(result, "namedTemporary");
        var named = As<ReferenceBindingExpression>(ReturnValue(namedFunction));
        Equal(ReferenceReferentOrigin.Temporary, named.ReferentOrigin);
        Check(named.Temporary == null);
        Same(Binding(Local(namedFunction, "first")).Temporary!, named.KnownTemporary);
        Same(Local(namedFunction, "first").Symbol, named.KnownTemporary!.LifetimeOwner);
        Equal(ReferenceTemporaryLifetime.ReferenceVariable, named.KnownTemporary.Lifetime);
        Check(result.Diagnostics.Where(d => d.Code == "REF006").All(d => d.Range.Spelling != "staticTemporary"));
        AnalysisResult nested = Success("int run(){const int& first=1; const int& second=first; return second;}");
        Check(Binding(Local(Function(nested, "run"), "second")).Temporary == null);
        var chainSource = new System.Text.StringBuilder("int object=1; int& r0=object; const int& t0=2; const int& chain(){const int& local0=3;");
        for (int index = 1; index <= 512; ++index) chainSource.Append($"const int& local{index}=local{index - 1};");
        chainSource.Append("return local512;}");
        for (int index = 1; index <= 512; ++index) chainSource.Append($"int& r{index}=r{index - 1}; const int& t{index}=t{index - 1};");
        AnalysisResult chains = Success(chainSource.ToString());
        Equal(ReferenceReferentOrigin.StaticObject, Binding(Variable(chains, "r512")).ReferentOrigin);
        Check(Binding(Variable(chains, "r512")).KnownTemporary == null);
        Same(Binding(Variable(chains, "t0")).Temporary!, Binding(Variable(chains, "t512")).KnownTemporary!);
        ReferenceBindingExpression final = As<ReferenceBindingExpression>(ReturnValue(Function(chains, "chain")));
        Same(Binding(Local(Function(chains, "chain"), "local0")).Temporary!, final.KnownTemporary!);
        Check(final.Temporary == null); Equal(1, chains.Diagnostics.Count(diagnostic => diagnostic.Code == "REF006"));
    }

    internal static void ReferenceRollback()
    {
        using var session = new CompilationSession();
        var table = new TypeTable(TargetInfo.Lp64);
        var diagnostics = new DiagnosticBag();
        var owner = new ParseState(session.Lex("int source=1; int choose(const int&);").Tokens.CreateCursor(), diagnostics);
        new Parser(owner, table).TranslationUnit();
        Symbol original = owner.GlobalScope.FindLocal("choose")!;
        int declarations = owner.GlobalScope.Declarations.Count;
        var checkpoint = owner.Checkpoint();
        var replay = new ParseState(session.Lex("int choose(int&); int run(){int& alias=source; return choose(alias);}").Tokens.CreateCursor(), diagnostics, owner.GlobalScope, owner);
        new Parser(replay, table).TranslationUnit();
        Equal(0, diagnostics.Count);
        var firstRun = owner.GlobalScope.FindLocal("run")!.Definition!;
        CallExpression firstCall = As<CallExpression>(ReturnValue(firstRun));
        Check(!ReferenceEquals(original, firstCall.SelectedFunction));
        ReferenceBindingExpression firstBinding = Binding(Local(firstRun, "alias"));
        Same(owner.GlobalScope.FindLocal("source")!, As<ReferenceExpression>(firstBinding.Referent).ReferencedSymbol);
        owner.Restore(checkpoint);
        Equal(declarations, owner.GlobalScope.Declarations.Count);
        Same(original, owner.GlobalScope.FindLocal("choose"));
        Check(owner.GlobalScope.FindLocal("run") == null);
        var second = new ParseState(session.Lex("int run(){return choose(source);}").Tokens.CreateCursor(), diagnostics, owner.GlobalScope, owner);
        new Parser(second, table).TranslationUnit();
        Equal(0, diagnostics.Count);
        CallExpression secondCall = As<CallExpression>(ReturnValue(owner.GlobalScope.FindLocal("run")!.Definition!));
        Same(original, secondCall.SelectedFunction);
        Equal(1, secondCall.Candidates.Count);
        Equal(2, firstCall.Candidates.Count);
        Check(!ReferenceEquals(firstCall.SelectedFunction, secondCall.SelectedFunction));
        owner.Restore(checkpoint);
        Equal(declarations, owner.GlobalScope.Declarations.Count);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Scope> DiscardReferenceAnalysis(CompilationSession session, string source, bool success)
    {
        AnalysisResult result = session.Analyze(source);
        Equal(success, result.IsSuccess);
        return new WeakReference<Scope>(result.GlobalScope);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Scope> DiscardScopedReferenceTypes(TypeTable table)
    {
        var scope = new Scope(ScopeKind.Template, new Scope(ScopeKind.TranslationUnit, null));
        var parameter = new TemplateParameterType("T", 0, scope);
        ReferenceType reference = table.Reference(parameter, ReferenceKind.LValue);
        // Exercise both result and parameter recursive ownership, including a
        // function nested beneath a reference and then another function.
        FunctionType result = table.Function(reference, [table.Integer(IntegerKind.Int)]);
        FunctionType argument = table.Function(table.Integer(IntegerKind.Int), [reference]);
        ReferenceType nested = table.Reference(argument, ReferenceKind.LValue);
        FunctionType outer = table.Function(table.Integer(IntegerKind.Int), [nested]);
        Check(TypeTable.ContainsScopedOrDependentType(result) && TypeTable.ContainsScopedOrDependentType(outer));
        Check(!ReferenceEquals(result, table.Function(reference, [table.Integer(IntegerKind.Int)])));
        return new WeakReference<Scope>(scope);
    }

    internal static void ReferenceOwnership()
    {
        using var session = new CompilationSession();
        var weak = new List<WeakReference<Scope>>
        {
            DiscardReferenceAnalysis(session, "int object=1; int& f(int& x){return x;} int& r=object;", true),
            DiscardReferenceAnalysis(session, "void& invalid();", false),
            DiscardReferenceAnalysis(session, "int refcall(int&); template<class T> T invoke(T x){return refcall(x);} int run(){return invoke(1);}", true),
            DiscardReferenceAnalysis(session, "template<class T> T f(int& x){return x;}", false),
            DiscardReferenceAnalysis(session, "template<class T> T f(T& x){return x;}", false),
            DiscardReferenceAnalysis(session, "template<class T> T f(T x){using R=T&; return x;}", false)
        };
        var table = new TypeTable(TargetInfo.Lp64);
        weak.Add(DiscardScopedReferenceTypes(table));
        Equal(0, table.InternedReferenceTypeCount);
        Equal(0, table.InternedFunctionTypeCount);
        for (int i = 0; i < 3; ++i) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
        Check(weak.All(reference => !reference.TryGetTarget(out _)), "A live compound type cache retained a dropped TU/template scope.");
        AnalysisResult first = session.Analyze("int object=1; int& f(int& x){return x;} int& r=object;");
        AnalysisResult second = session.Analyze("int object=2; int& f(int& x){return x;} int& r=object;");
        Check(first.IsSuccess && second.IsSuccess);
        Same(Function(first, "f").Type, Function(second, "f").Type);
        Same(Variable(first, "r").Type, Variable(second, "r").Type);
        Check(!ReferenceEquals(Function(first, "f").Symbol, Function(second, "f").Symbol));
        Check(!ReferenceEquals(Binding(Variable(first, "r")).Source, Binding(Variable(second, "r")).Source));
        Same(Variable(first, "object").Symbol, As<ReferenceExpression>(Binding(Variable(first, "r")).Source).ReferencedSymbol);
        Same(Variable(second, "object").Symbol, As<ReferenceExpression>(Binding(Variable(second, "r")).Source).ReferencedSymbol);
        using var independentSession = new CompilationSession();
        AnalysisResult independent = independentSession.Analyze("int& f(int& x){return x;}");
        Check(independent.IsSuccess);
        Check(!ReferenceEquals(Function(first, "f").Type, Function(independent, "f").Type));
        Check(!ReferenceEquals(Function(first, "f").Type.ReturnType, Function(independent, "f").Type.ReturnType));
        GC.KeepAlive(session);
        GC.KeepAlive(table);
    }

    internal static void ReferenceBoundaries()
    {
        foreach (string source in new[]
        {
            "template<class T> T& f(T& x){return x;}",
            "template<class T> T f(T& x){return x;}",
            "template<class T> T f(int& x){return x;}",
            "using R=int&; template<class T> R f(T x){return x;}",
            "template<class T> T f(T x){using R=T&; return x;}",
            "using R=int&; R* pointer;", "int (&parenthesized)=0;", "int& array[2];"
        })
        {
            AnalysisResult result = Analyze(source);
            Check(!result.IsSuccess && result.ContainsUnsupportedFeatures, source);
        }
    }

    internal static void ReferenceFixtures()
    {
        AnalysisResult result = Success(Fixture("m3-reference-positive.cpp"));
        Same(Function(result, "left").Symbol, As<CallExpression>(As<ReferenceBindingExpression>(ReturnValue(Function(result, "relay"))).Source).SelectedFunction);
        Equal(ReferenceKind.RValue, ReferenceParameter(ReturnedCall(result, "temporary")).ReferenceKind);
        Equal(TypeQualifiers.Const, ReferenceParameter(ReturnedCall(result, "immutable")).ReferredType.Qualifiers);
        Equal(TypeQualifiers.None, ReferenceParameter(ReturnedCall(result, "writable")).ReferredType.Qualifiers);
        AnalysisResult ranking = Success(Fixture("m3-reference-ranking.cpp"));
        Equal(StandardConversionRank.Promotion, ReturnedCall(ranking, "reference_short").SelectedCandidate.ArgumentConversions[0].Rank);
        Equal(ReferenceKind.RValue, ReferenceParameter(ReturnedCall(ranking, "reference_from_lvalue")).ReferenceKind);
        foreach (string fixture in new[]
        {
            "m3-reference-invalid.cpp", "m3-reference-ambiguous.cpp", "m3-reference-rvalue-invalid.cpp",
            "m3-reference-temporary-invalid.cpp", "m3-reference-void-invalid.cpp", "m3-reference-redeclaration-invalid.cpp",
            "m3-reference-crossed-ambiguous.cpp"
        })
        {
            AnalysisResult invalid = Analyze(Fixture(fixture));
            Check(!invalid.IsSuccess && !invalid.ContainsUnsupportedFeatures, fixture);
        }
        Check(Analyze(Fixture("m3-reference-dependent-unsupported.cpp")).ContainsUnsupportedFeatures);
        Check(Success(Fixture("m3-reference-lifetime.cpp")).Diagnostics.All(d => d.Severity == DiagnosticSeverity.Warning));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Scope> DiscardCharacterReferenceTemplate(CompilationSession session, bool unsupported)
    {
        AnalysisResult result = session.Analyze(unsupported
            ? "template<class T> T failed(T value){T& alias=value; return value;}"
            : "int need(char16_t&); template<class T> T failed(T value){return need(u'A');}");
        Check(!result.IsSuccess && result.ContainsUnsupportedFeatures == unsupported);
        FunctionTemplateDeclaration template = Template(result, "failed");
        Check(!template.IsValid);
        Same(template.TemplateScope, As<TemplateParameterType>(template.Type.ReturnType).ParameterScope);
        return new WeakReference<Scope>(template.TemplateScope);
    }

    internal static void CharacterReferences()
    {
        AnalysisResult result;
        AnalysisResult invalid;
        AnalysisResult excluded;
        using (var session = new CompilationSession())
        {
            result = session.Analyze(Fixture("m3-reference-character.cpp"), "m3-reference-character.cpp");
            invalid = session.Analyze("int need(char16_t&); template<class T> T failed(T value){return need(u'A');}");
            excluded = session.Analyze("template<class T> T failed(T value){T& alias=value; return value;}");
            WeakReference<Scope>[] dropped = [DiscardCharacterReferenceTemplate(session, false), DiscardCharacterReferenceTemplate(session, true)];
            for (int i = 0; i < 3; ++i) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
            Check(dropped.All(reference => !reference.TryGetTarget(out _)), "Reference handling retained a dropped template scope through the session.");
            GC.KeepAlive(session);
        }
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        foreach (var (name, kind) in new[] { ("ordinary_alias", IntegerKind.Char), ("wide_alias", IntegerKind.WChar), ("bound", IntegerKind.Char16) })
        {
            ReferenceBindingExpression binding = Binding(Variable(result, name));
            Equal(kind, Kind(binding.Type)); Equal(ValueCategory.LValue, binding.ValueCategory);
            Equal(ReferenceBindingKind.Direct, binding.Binding.Kind);
            Check(binding.ConstantValue == null && binding.Temporary == null && !binding.Binding.RequiresLValueToRValue);
        }
        Same(result.GlobalScope.FindLocal("UnitReference")!.Type, Variable(result, "bound").Type);
        ReferenceBindingExpression returned = As<ReferenceBindingExpression>(ReturnValue(Function(result, "relayed")));
        CallExpression call = As<CallExpression>(returned.Source);
        Same(Function(result, "left").Type.ReturnType, call.DeclaredReturnType);
        Equal(IntegerKind.Char16, Kind(call.Type)); Equal(ValueCategory.LValue, call.ValueCategory);
        Equal(IntegerKind.Char16, Kind(As<ReferenceBindingExpression>(call.Arguments[0]).Type));
        ReferenceBindingExpression expiring = Binding(Variable(result, "expiring_alias"));
        Equal(ValueCategory.XValue, expiring.Source.ValueCategory);
        Equal(ReferenceBindingKind.Direct, expiring.Binding.Kind); Check(expiring.Temporary == null);
        ReferenceBindingExpression materialized = Binding(Variable(result, "materialized"));
        Equal(ReferenceBindingKind.MaterializedTemporary, materialized.Binding.Kind);
        CharacterLiteralExpression literal = As<CharacterLiteralExpression>(materialized.Temporary!.Initializer);
        Equal(IntegerKind.Char8, Kind(literal.Type)); Equal(LiteralEncoding.Utf8, literal.Encoding);
        Equal((uint)65, literal.CodeUnits.Single()); Equal(new BigInteger(65), literal.ConstantValue!.Value);
        Check(literal.Range.File.OriginForRange(literal.Range).ExpansionHistory.Any(frame => frame.Name == "UTF8_VALUE"));
        Check(materialized.ConstantValue == null && materialized.Temporary.ConstantValue == null);
        CallExpression promotion = ReturnedCall(result, "promoted");
        Equal(IntegerKind.UnsignedInt, Kind(ReferenceParameter(promotion).ReferredType));
        Equal(StandardConversionRank.Promotion, promotion.SelectedCandidate.ArgumentConversions[0].Rank);
        Equal(StandardConversionRank.Conversion, promotion.Candidates.Single(candidate => !ReferenceEquals(candidate, promotion.SelectedCandidate)).ArgumentConversions[0].Rank);
        var promotedArgument = As<ReferenceBindingExpression>(promotion.Arguments[0]);
        Equal(ReferenceBindingKind.ConvertedTemporary, promotedArgument.Binding.Kind);
        var conversion = As<ConversionExpression>(promotedArgument.Temporary!.Initializer);
        Equal(ConversionKind.IntegralPromotion, conversion.Conversion);
        Equal(IntegerKind.Char32, Kind(As<CharacterLiteralExpression>(conversion.Operand).Type));
        CallExpression exact = ReturnedCall(result, "exact_call");
        Equal(IntegerKind.Char32, Kind(ReferenceParameter(exact).ReferredType));
        Equal(StandardConversionRank.ExactMatch, exact.SelectedCandidate.ArgumentConversions[0].Rank);
        CallExpression wide = ReturnedCall(result, "wide_call");
        Equal(IntegerKind.WChar, Kind(ReferenceParameter(wide).ReferredType));
        Equal(ReferenceBindingKind.Direct, As<ReferenceBindingExpression>(wide.Arguments[0]).Binding.Kind);
        FunctionTemplateDeclaration relay = Template(result, "relay");
        FunctionTemplateInstance instance = relay.Instances.Single();
        Equal(IntegerKind.Char16, Kind(instance.TypeArguments.Single())); Equal(1, instance.BodyReplayCount);
        Same(instance, ReturnedCall(result, "invoke").TemplateInstance!);
        Same(instance, ReturnedCall(result, "repeated").TemplateInstance!);
        CallExpression frozen = As<CallExpression>(ReturnValue(instance.Declaration!));
        Equal(2, frozen.Candidates.Count);
        Equal(IntegerKind.Char16, Kind(ReferenceParameter(frozen).ReferredType));
        Equal(TypeQualifiers.Const, ReferenceParameter(frozen).ReferredType.Qualifiers);
        Equal(TypeQualifiers.None, ReferenceParameter(ReturnedCall(result, "later_call")).ReferredType.Qualifiers);
        foreach (AnalysisResult retained in new[] { invalid, excluded })
        {
            Check(!retained.IsSuccess);
            FunctionTemplateDeclaration template = Template(retained, "failed");
            Check(!template.IsValid && template.BodyTokens.Tokens.Count > 1);
            Same(template.TemplateScope, template.Pattern.ParameterScope.Parent!);
            Same(template.TemplateScope, As<TemplateParameterType>(template.Type.ReturnType).ParameterScope);
            Check(template.BodyTokens.Tokens.Any(token => token.Spelling == "return"));
        }
        Check(invalid.Diagnostics.Any(diagnostic => diagnostic.Code == "SEM023") && !invalid.ContainsUnsupportedFeatures);
        Check(excluded.ContainsUnsupportedFeatures);
        AnalysisResult discriminating = Success(Fixture("m3-reference-frozen.cpp"));
        FunctionTemplateDeclaration frozenTemplate = Template(discriminating, "frozen");
        FunctionTemplateInstance frozenInstance = frozenTemplate.Instances.Single();
        Same(frozenInstance, ReturnedCall(discriminating, "run").TemplateInstance!);
        Equal(TemplateInstanceState.Complete, frozenInstance.State); Equal(1, frozenInstance.BodyReplayCount);
        CallExpression earlier = As<CallExpression>(ReturnValue(frozenInstance.Declaration!));
        Equal(1, earlier.Candidates.Count);
        Same(discriminating.GlobalScope.FindLocalBinding("helper")!.Symbols[0], earlier.SelectedFunction);
        Equal(IntegerKind.Int, Kind(earlier.Type));
        Equal(TypeQualifiers.Const, ReferenceParameter(earlier).ReferredType.Qualifiers);
        Equal(ReferenceBindingKind.Direct, As<ReferenceBindingExpression>(earlier.Arguments[0]).Binding.Kind);
        Equal(TypeKind.Void, As<FunctionType>(discriminating.GlobalScope.FindLocalBinding("helper")!.Symbols[1].Type).ReturnType.Kind);
    }
}
