using System;
using System.Text;

namespace DotCC;

/// <summary>Names in the typed object-fragment contract, not parsed C# source.</summary>
internal static class FunctionPointerNames
{
    public const string TypeKeyPrefix = "DotCcFunctionPointers.";
    public static string OwnerAlias(string name) => "__DotCcFunctionOwner_" + Convert.ToHexString(Encoding.UTF8.GetBytes(name));
}
