using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DotCC;

public static partial class Compiler
{
    // Large linked programs can have megabytes of static initialization IL.
    // Keep the constructor small without changing storage or statement order.
    // A single initializer remains indivisible here; aggregate lowering owns
    // splitting a large aggregate into its individual C stores.
    private static (string Calls, string Members) PartitionStaticInitializers(string source)
    {
        const int limit = 32768;
        if (source.Length <= limit) return (source, "");
        var calls = new StringBuilder();
        var members = new StringBuilder();
        var part = new StringBuilder();
        void Flush()
        {
            if (part.Length == 0) return;
            string body = part.ToString();
            string name = "__DotCcInitializeGlobals_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
            calls.Append("        ").Append(name).Append("();\n");
            members.Append("    private static unsafe void ").Append(name).Append("()\n    {\n")
                .Append(body).Append("\n    }\n");
            part.Clear();
        }
        foreach (string statement in InitializerStatements(source))
        {
            if (part.Length + statement.Length > limit) Flush();
            part.Append(statement);
        }
        Flush();
        return (calls.ToString(), members.ToString());
    }

    internal static IEnumerable<string> InitializerStatements(string source)
    {
        int start = 0, nesting = 0;
        char quote = '\0';
        for (int i = 0; i < source.Length; ++i)
        {
            char c = source[i];
            if (quote != '\0')
            {
                if (c == '\\') ++i;
                else if (c == quote) quote = '\0';
                continue;
            }
            if (c is '\'' or '"') { quote = c; continue; }
            if (c == '/' && i + 1 < source.Length)
            {
                if (source[i + 1] == '*')
                {
                    int close = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    if (close < 0) throw new CompileException("unterminated compiler initializer comment");
                    i = close + 1;
                    continue;
                }
                if (source[i + 1] == '/')
                {
                    int close = source.IndexOf('\n', i + 2);
                    if (close < 0) break;
                    i = close;
                    continue;
                }
            }
            if (c is '(' or '[' or '{') ++nesting;
            else if (c is ')' or ']' or '}') --nesting;
            else if (c == ';' && nesting == 0)
            {
                yield return source[start..(i + 1)];
                start = i + 1;
            }
        }
        if (start < source.Length) yield return source[start..];
    }
}
