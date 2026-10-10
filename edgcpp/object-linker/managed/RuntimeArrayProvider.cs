namespace DotObj;

internal static class RuntimeArrayProvider
{
    public static string? TryGetMethod(Symbol symbol)
    {
        if (symbol.Entity.Definition != "declaration" || symbol.Entity.ExternalKey != symbol.Entity.SourceName ||
            symbol.Object.SourceFunctionKinds.GetValueOrDefault(symbol.Entity.Id) != "runtime") return null;
        string? method = symbol.Entity.ExternalKey switch
        {
            "__cxa_vec_ctor" => "Construct", "__cxa_vec_cctor" => "CopyConstruct",
            "__cxa_vec_dtor" => "Destroy", "__cxa_vec_cleanup" => "Cleanup", _ => null,
        };
        if (method is null) return null;
        List<string> actual = symbol.Object.Parameters.GetValueOrDefault(symbol.Entity.Id, []);
        bool Callback(string type, int count) => RoutineTypes.IsFunctionPointer(type) &&
            RoutineTypes.Signature(ScalarTypes.Pointee(type)) is var signature &&
            signature.Result == "void" && !signature.Variadic &&
            signature.Parameters.SequenceEqual(Enumerable.Repeat("ptr(void)", count));
        int prefix = method == "CopyConstruct" ? 4 : 3;
        if (symbol.Object.Type(symbol.Entity.ReturnTypeId) != "void" || symbol.Object.Routines[symbol.Entity.RoutineTypeId].Variadic ||
            actual.Count != prefix + (method is "Construct" or "CopyConstruct" ? 2 : 1) ||
            actual[0] != "ptr(void)" || method == "CopyConstruct" && actual[1] != "ptr(void)" ||
            actual[prefix - 2] != "u64" || actual[prefix - 1] != "u64" ||
            !Callback(actual[prefix], method == "CopyConstruct" ? 2 : 1) ||
            method is "Construct" or "CopyConstruct" && !Callback(actual[prefix + 1], 1))
            throw new InvalidDataException("Array runtime helper differs from its portable Itanium ABI: " + symbol.Entity.ExternalKey);
        return method;
    }
}
