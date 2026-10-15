namespace RoslynObj.CSharp;

// Selected public identities stay within the translator while their input is
// open. Generated projects and the runtime do not retain reader entities.
internal sealed record SemanticRuntimeFunctionImplementation(FunctionSymbol Function, string Contract)
{
    internal const string Int32ExceptionOwnerContract = "managed-cpp-int32-exception-owner-v1";
}
