using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DotCC;

public static partial class Compiler
{
    private static void ValidateContextNames(CSharpOutputOptions? options, IEnumerable<string> names)
    {
        if (options?.StateContext != true) return;
        foreach (var name in names)
            if (name.TrimStart('@').StartsWith("__DotCc", StringComparison.Ordinal))
                throw new CompileException("--state-context reserves the __DotCc member prefix");
    }

    // Objects retain their storage recipes. Context ownership is selected only
    // after relocation, so the same objects can still produce a legacy library.
    private static Backends.CSharpGlobalOutput RenderStateContext(
        Backends.CSharpGlobalOutput output, IReadOnlyList<Backends.CSharpGlobalSource> globals,
        LiteralPool.Output literals, GlobalStorageReferences storage, string owner)
    {
        var specialFields = new StringBuilder();
        var threadFields = new StringBuilder();
        var specialMembers = new StringBuilder();
        var specialInitializers = new StringBuilder();
        foreach (var global in globals)
        {
            if (global.Name.StartsWith("__DotCc", StringComparison.Ordinal))
                throw new CompileException("--state-context reserves the __DotCc member prefix");
            var text = storage.Rewrite(literals.Rewrite(global.StaticMembers));
            if (text.Length == 0) continue;
            // This is a compiler-owned storage recipe, not arbitrary C# input.
            // Only its first backing field moves; property/method bodies remain
            // unchanged and resolve that backing through the bound context.
            var match = Regex.Match(text, @"\A\s*(?:\[ThreadStatic\]\s*)?private static (?:unsafe )?(?<type>[^\r\n=;]+?) (?<name>__dotcc_[A-Za-z0-9_]+)(?<tail>\s*[=;])");
            if (!match.Success) throw new CompileException("unsupported contextual global storage recipe: " + global.Name);
            int end = StorageFieldEnd(text, match.Index + match.Length - 1);
            string type = match.Groups["type"].Value;
            string name = match.Groups["name"].Value;
            bool thread = global.ThreadField.Length != 0;
            (thread ? threadFields : specialFields).Append("            internal ").Append(type).Append(' ').Append(name).Append(";\n");
            string fieldTail = text[(match.Index + match.Length - 1)..end].Trim();
            if (fieldTail.StartsWith('=') && thread)
                throw new CompileException("initialized contextual TLS storage recipe is unsupported");
            if (fieldTail.StartsWith('=')) specialInitializers.Append("        ").Append(name).Append(' ').Append(fieldTail).Append(";\n");
            specialMembers.Append("    private static ref ").Append(type).Append(' ').Append(name)
                .Append(" => ref ").Append(owner).Append(thread ? ".__DotCcCurrent.ThreadState." : ".__DotCcCurrent.Special.")
                .Append(name).Append(";\n").Append(text[(end + 1)..]);
        }
        specialMembers.Append("    internal static void __DotCcInitialize()\n    {\n").Append(specialInitializers).Append("    }\n");
        string globalsType = HelperClass(owner, "Globals"), threadType = globalsType + "ThreadLocal";
        string globalStorage = output.Fields.Length == 0 ? "" : $$"""
                    internal {{globalsType}}[] Values = global::System.GC.AllocateArray<{{globalsType}}>(1, pinned: true);
            """;
        string threadStorage = output.ThreadFields.Length == 0 ? "" : $$"""
                    internal {{threadType}}[] Values = global::System.GC.AllocateArray<{{threadType}}>(1, pinned: true);
            """;
        string accessors = output.Fields.Length == 0 ? "" : $$"""
                public static ref {{globalsType}} {{output.GlobalName}} => ref __DotCcCurrent.Values[0];
            """;
        if (output.ThreadFields.Length != 0) accessors += $$"""

                internal static ref {{threadType}} {{output.ThreadName}} => ref __DotCcCurrent.ThreadState.Values[0];
            """;
        string members = $$"""
                // Every entry into translated code must bind its program context
                // on that thread. No process-wide default is created implicitly.
                internal static __DotCcContext __DotCcCurrent =>
                    Libc.RuntimeContext.Current?.ProgramState as __DotCcContext
                    ?? throw new global::System.InvalidOperationException("No C program context is bound on this thread.");
            {{accessors}}
                internal sealed unsafe class __DotCcSpecialState
                {
            {{specialFields}}    }
                internal sealed unsafe class __DotCcThreadState
                {
            {{threadStorage}}
            {{threadFields}}    }
                public sealed unsafe class __DotCcContext : global::System.IDisposable
                {
                    private readonly Libc.RuntimeContext runtime;
                    private readonly global::System.Threading.ThreadLocal<__DotCcThreadState> threads = new(() => new __DotCcThreadState());
                    internal __DotCcSpecialState Special = new();
            {{globalStorage}}
                    internal __DotCcThreadState ThreadState => threads.Value!;
                    private readonly object disposeGate = new();
                    private bool disposed;
                    internal __DotCcContext() { runtime = new Libc.RuntimeContext(this); }
                    public global::System.IDisposable Enter() => runtime.Enter();
                    public void Dispose()
                    {
                        lock (disposeGate)
                        {
                        if (disposed) return;
                        // Runtime disposal refuses live bindings/created threads.
                        // Keep all backing intact when a caller must quarantine it.
                        runtime.Dispose();
                        disposed = true;
                        threads.Dispose();
                        Special = null!;
                        {{(output.Fields.Length == 0 ? "" : $"Values = global::System.Array.Empty<{globalsType}>();")}}
                        }
                    }
                }
                public static unsafe __DotCcContext __DotCcCreateContext()
                {
                    var context = new __DotCcContext();
                    try
                    {
                        using (context.Enter())
                        {
                            {{globalsType}}Special.__DotCcInitialize();
            {{output.Initializers}}
                        }
                        return context;
                    }
                    catch { context.Dispose(); throw; }
                }
            """;
        return output with { StaticMembers = specialMembers.ToString(), ContextMembers = members };
    }

    private static int StorageFieldEnd(string text, int start)
    {
        // Initializers can contain C# strings with semicolons. Template output
        // does not use raw/verbatim strings; handle quoted tokens and comments.
        char quote = '\0';
        for (int i = start; i < text.Length; i++)
        {
            char c = text[i];
            if (quote != '\0') { if (c == '\\') i++; else if (c == quote) quote = '\0'; continue; }
            if (c is '\'' or '"') { quote = c; continue; }
            if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
            { int close = text.IndexOf("*/", i + 2, StringComparison.Ordinal); if (close < 0) break; i = close + 1; continue; }
            if (c == ';') return i;
        }
        throw new CompileException("unterminated contextual storage recipe");
    }
}
