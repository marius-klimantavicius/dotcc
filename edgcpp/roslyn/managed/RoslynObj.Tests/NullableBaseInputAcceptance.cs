using RoslynObj;

internal static class NullableBaseInputAcceptance
{
    internal static void Run(string path,string constructorName,string baseTypeName)
    {
        var checks=0;
        void Check(bool value,string message) { if(!value)throw new Exception(message);checks++; }
        using var snapshot=RoslynObject.Open(path);
        Check(snapshot.Manifest.FormatMajor==2&&snapshot.Manifest.FormatMinor==0,"The nullable-base input must use the exact supported 2.0 contract.");
        var functions=snapshot.TranslationUnit.SemanticModel.FindSymbols(constructorName).Take(65).OfType<FunctionSymbol>().Where(f=>f.Details.IsPrototypeInstantiation).ToArray();
        Check(functions.Length==2,"The agreed real input must retain its two prototype constructor descriptors.");
        var bases=new List<BaseSubobject>();
        foreach(var function in functions)
        {
            var scope=function.Details.BodyScope!;
            Check(scope.AssociatedEntity==function&&function.ContainingType?.Name==constructorName,"The selected constructor must retain its actual scope and containing-type associations.");
            var initializer=scope.ConstructorInitializers.Single(i=>i.BaseSubobject is not null);var subobject=initializer.BaseSubobject!;bases.Add(subobject);
            Check(initializer.Details.BaseSubobject==subobject&&initializer.Field is null,"The actual constructor initializer must retain its canonical base target.");
            Check(subobject.MostDerivedType is null&&subobject.Details.MostDerivedType is null,"The unpopulated most-derived pointer must remain null without a containing-class fallback.");
            Check(subobject.BaseType==subobject.Details.BaseType&&subobject.BaseType.Name==baseTypeName,"The temporary subobject must preserve its exact recorded base type.");
            var association=scope.Details.ScopeConstructorInitializerRows.Single(r=>r.Initializer==initializer);
            Check(association.Owner==scope&&association.Initializer==initializer&&association.Ordinal==0,"The recorded scope list must preserve its exact initializer identity and ordinal.");
            Check(function.Body?.Root.RepresentationRole.Name=="metadata"&&initializer.Initialization?.Operation?.RepresentationRole.Name=="metadata","Prototype body and constructor recipe navigation must not promote metadata into execution.");
        }
        Check(bases[0]!=bases[1]&&bases[0].BaseType==bases[1].BaseType,"The two recorded temporary subobjects must remain distinct despite sharing their exact base type.");
        Console.WriteLine($"PASS:nullable-base-input, {checks} assertions");
    }
}
