using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using VM = Managed.Interpreters.CPython;

// Measures the emitted types in the selected PE. No mirror layouts or
// interpreter initialization are needed to check this representation boundary.
internal static unsafe class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 2) throw new ArgumentException("Expected native oracle report and output report.");
        using var oracle = JsonDocument.Parse(File.ReadAllBytes(args[0]));
        long[] expected = oracle.RootElement.GetProperty("native_matching_target_header_layouts")
            .GetProperty("upstream-tests-static").EnumerateArray().Select(v => v.GetInt64()).ToArray();
        VM._is* interpreter = (VM._is*)NativeMemory.AllocZeroed((nuint)sizeof(VM._is));
        VM.pyruntimestate* state = (VM.pyruntimestate*)NativeMemory.AllocZeroed((nuint)sizeof(VM.pyruntimestate));
        if (interpreter == null || state == null) throw new OutOfMemoryException();
        try
        {
            long[] actual = [sizeof(VM._is), (byte*)&interpreter->warnings - (byte*)interpreter,
                sizeof(VM.pyruntimestate), (byte*)&state->_main_interpreter - (byte*)state,
                sizeof(char), (byte*)&interpreter->before_forkers - (byte*)interpreter,
                (byte*)&interpreter->after_forkers_parent - (byte*)interpreter,
                (byte*)&interpreter->after_forkers_child - (byte*)interpreter];
            bool passed = expected.Length == 8 && actual.SequenceEqual(expected);
            var result = new LayoutResult(passed ? "passed" : "failed", actual, expected);
            File.WriteAllText(args[1], JsonSerializer.Serialize(result, LayoutJson.Default.LayoutResult) + "\n");
            return passed ? 0 : 1;
        }
        finally { NativeMemory.Free(interpreter); NativeMemory.Free(state); }
    }
}

internal sealed record LayoutResult(string status, long[] actual, long[] expected);
[JsonSerializable(typeof(LayoutResult))]
internal partial class LayoutJson : JsonSerializerContext;
