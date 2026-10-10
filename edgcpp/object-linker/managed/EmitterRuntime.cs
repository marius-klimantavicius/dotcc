using System.Security.Cryptography;
using System.Text;

namespace DotObj;

internal sealed partial class Emitter
{
    private readonly Dictionary<string, (string Name, string Code)> operationHelpers = new(StringComparer.Ordinal);
    private string VolatileRead(string type, string address, string? place = null) => type == "f80" ? "global::Edg.Runtime.Float80Access.Read(" + address + ")" : ScalarTypes.IsPointer(type)
        ? "((" + Type(type) + ")global::System.Threading.Volatile.Read(ref *(nint*)" + address + "))"
        : "global::System.Threading.Volatile.Read(ref " + (place ?? "*(" + Type(type) + "*)" + address) + ")";
    private string VolatileWrite(string type, string address, string value, string? place = null) => type == "f80" ? "global::Edg.Runtime.Float80Access.Write(" + address + ", " + value + ")" : "global::System.Threading.Volatile.Write(ref " +
        (ScalarTypes.IsPointer(type) ? "*(nint*)" + address : place ?? "*(" + Type(type) + "*)" + address) + ", " + (ScalarTypes.IsPointer(type) ? "(nint)" : "") + value + ")";
    private string VolatileOperation(string type, string operation, string address, string? value, string? valueType = null)
    {
        valueType ??= type; string key = "volatile:" + type + ":" + operation + ":" + valueType;
        if (!operationHelpers.TryGetValue(key, out var helper))
        {
            string name = generatedMembers.Allocate("__dot_volatile_" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..12], "volatile_operation");
            string t = Type(type), argument = value is null ? "" : ", " + Type(valueType) + " value";
            var code = new StringBuilder("    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\n")
                .Append("    private static ").Append(t).Append(' ').Append(name).Append('(').Append(t).Append("* address").Append(argument).Append(")\n    {\n");
            if (operation != "assign")
            {
                code.Append("        ").Append(t).Append(" previous = ").Append(VolatileRead(type, "address")).Append(";\n");
                string expression = operation switch
                {
                    "pre-inc" or "post-inc" => "unchecked((" + t + ")(previous + " + (ScalarTypes.IsZeroStridePointer(type) ? "0" : "1") + "))",
                    "pre-dec" or "post-dec" => "unchecked((" + t + ")(previous - " + (ScalarTypes.IsZeroStridePointer(type) ? "0" : "1") + "))",
                    _ => CompoundValue(type, valueType, operation[..^1], "previous", "value")
                };
                code.Append("        ").Append(t).Append(" next = ").Append(expression).Append(";\n");
            }
            string assigned = operation == "assign" ? "value" : "next";
            code.Append("        ").Append(VolatileWrite(type, "address", assigned)).Append(";\n        return ")
                .Append(operation is "post-inc" or "post-dec" ? "previous" : assigned).Append(";\n    }\n\n");
            helper = (name, code.ToString()); operationHelpers.Add(key, helper);
        }
        return Owner + "." + helper.Name + "(" + address + (value is null ? "" : ", " + value) + ")";
    }
    private static string PromotedType(string left, string right) => left == "f80" || right == "f80" ? "f80"
        : left == "f64" || right == "f64" ? "f64" : left == "f32" || right == "f32" ? "f32"
        : left == "u128" || right == "u128" ? "u128" : left == "i128" || right == "i128" ? "i128" : left == "u64" || right == "u64" ? "u64"
        : left == "i64" || right == "i64" ? "i64" : left == "u32" || right == "u32" ? "u32" : "i32";
    private string CompoundValue(string type, string valueType, string op, string left, string right)
    {
        if (ScalarTypes.IsPointer(type)) return "(" + left + " " + op + " " +
            (ScalarTypes.IsZeroStridePointer(type) ? "(" + right + " * 0 /* zero-size array stride */)" : right) + ")";
        string promoted = op is "<<" or ">>" ? PromotedType(type, "i32") : valueType;
        return "unchecked((" + Type(type) + ")((" + Type(promoted) + ")" + left + " " + op + " (" + (op is "<<" or ">>" ? "int" : Type(promoted)) + ")" + right + "))";
    }
    private void WriteRuntimeSupport(string output, bool programArguments)
    {
        bool varargs = linker.Objects.Any(obj => obj.Types.Values.Contains("va-list") || obj.Routines.Values.Any(routine => routine.Variadic));
        string[] runtimeFiles = (varargs ? new[] { "VaList.cs", "VaArg.cs", "VaPack.cs" } : [])
            .Concat(linker.Objects.Any(obj => obj.Types.Values.Contains("f80")) ? ["Float80.cs"] : [])
            .Concat(programArguments ? ["ProgramArguments.cs"] : [])
            .Concat(linker.Externs.Values.Any(symbol => RuntimeArrayProvider.TryGetMethod(symbol) is not null) ||
                linker.Definitions.Any(symbol => symbol.Object.FunctionCleanupActions.GetValueOrDefault(symbol.Entity.Id, []).Any(action => symbol.Object.CleanupArrays.ContainsKey(action.Id))) ? ["CppArrays.cs"] : []).ToArray();
        foreach (string file in runtimeFiles)
            {
                using Stream source = typeof(Emitter).Assembly.GetManifestResourceStream("DotObj.Runtime." + file)
                    ?? throw new InvalidOperationException("Shared runtime source resource is missing");
                using var reader = new StreamReader(source);
                File.WriteAllText(Path.Combine(output, "Runtime." + file), "// <auto-generated />\n" + (file == "Float80.cs" ? "#nullable enable\n" : "") + reader.ReadToEnd(), new UTF8Encoding(false));
            }
        if (linker.Objects.Any(obj => obj.Types.Values.Contains("f80"))) WriteFloat80Access(output);
        if (operationHelpers.Count != 0)
            File.WriteAllText(Path.Combine(output, "Translated.Helpers.generated.cs"),
                "// <auto-generated />\n#nullable enable\nnamespace " + namespaceName + ";\npublic static unsafe partial class " + className + "\n{\n" +
                string.Concat(operationHelpers.Values.Select(helper => helper.Code)) + "}\n", new UTF8Encoding(false));
    }
}
