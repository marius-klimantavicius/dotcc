using System.Runtime.InteropServices;
using Managed.Interpreters;
using VM = Managed.Interpreters.QuickJs;

// Links the actual production marshaller. Both stack and owned destination
// storage start poisoned. Every byte count excludes its required terminator.
internal static unsafe class QuickJsTextControl
{
    private static int assertions;
    private static void Check(bool value, string message)
    {
        assertions++;
        if (!value) throw new InvalidOperationException(message);
    }

    private static void Bytes(string text, byte[] expected)
    {
        Check(QuickJsText.GetByteCount(text) == expected.Length, "WTF-8 byte count");
        Span<byte> stack = stackalloc byte[1024];
        stack.Fill(0x7f);
        int count = QuickJsText.GetBytes(text, stack);
        Check(count == expected.Length && stack[..count].SequenceEqual(expected), "stack exact bytes");
        Check(stack[count] == 0 && stack[count + 1] == 0x7f, "stack terminator and guard");
        byte* owned = (byte*)NativeMemory.Alloc((nuint)(expected.Length + 2));
        try
        {
            var destination = new Span<byte>(owned, expected.Length + 2);
            destination.Fill(0x7f);
            count = QuickJsText.GetBytes(text, destination);
            Check(count == expected.Length && destination[..count].SequenceEqual(expected), "allocated exact bytes");
            Check(owned[count] == 0 && owned[count + 1] == 0x7f, "allocated terminator and guard");
        }
        finally { NativeMemory.Free(owned); }
        for (int capacity = 0; capacity <= 1024; capacity += 1024)
        {
            stack.Fill(0x7f);
            var marshaller = new QuickJsText.ManagedToUnmanagedIn();
            try
            {
                marshaller.FromManaged(text, stack[..capacity]);
                Check(marshaller.Count == expected.Length, "marshaller byte count");
                Check(new ReadOnlySpan<byte>(marshaller.ToUnmanaged(), marshaller.Count).SequenceEqual(expected), "marshaller bytes");
                Check(marshaller.ToUnmanaged()[marshaller.Count] == 0, "marshaller terminator");
            }
            finally { marshaller.Free(); }
        }
    }

    private static void Evaluate(VM.JSContext* context, string source, string expected)
    {
        Span<byte> stack = stackalloc byte[1024];
        for (int capacity = 0; capacity <= 1024; capacity += 1024)
        {
            stack.Fill(0x7f);
            var marshaller = new QuickJsText.ManagedToUnmanagedIn();
            try
            {
                marshaller.FromManaged(source, stack[..capacity]);
                Check(marshaller.ToUnmanaged()[marshaller.Count] == 0, "JS_Eval input[input_len]");
                fixed (byte* filename = "sentinel-boundary.js\0"u8)
                {
                    var value = VM.JS_Eval(context, marshaller.ToUnmanaged(), (ulong)marshaller.Count, filename, 0);
                    Check(value.tag != VM.JS_TAG_EXCEPTION, "actual JS_Eval success");
                    try
                    {
                        ulong length = 0;
                        byte* result = VM.JS_ToCStringLen2(context, &length, value, 1);
                        Check(result != null, "actual JS string conversion");
                        try { Check(QuickJsText.Decode(result, length) == expected, "actual JS_Eval exact text"); }
                        finally { if (result != null) VM.JS_FreeCString(context, result); }
                    }
                    finally { VM.JS_FreeValue(context, value); }
                }
            }
            finally { marshaller.Free(); }
        }
    }

    public static int Main()
    {
        Bytes("", []);
        Bytes("let x=1;", "let x=1;"u8.ToArray());
        Bytes("é水", [0xc3, 0xa9, 0xe6, 0xb0, 0xb4]);
        Bytes("😀", [0xf0, 0x9f, 0x98, 0x80]);
        Bytes("\ud800", [0xed, 0xa0, 0x80]);
        Bytes("\udc00", [0xed, 0xb0, 0x80]);
        Bytes("A\0B", [65, 0, 66]);
        Bytes(new string('a', 600), Enumerable.Repeat((byte)'a', 600).ToArray());
        var nullMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            nullMarshaller.FromManaged(null, Span<byte>.Empty);
            Check(nullMarshaller.ToUnmanaged() == null && nullMarshaller.Count == 0, "null pointer contract");
        }
        finally { nullMarshaller.Free(); }

        var runtime = VM.JS_NewRuntime();
        Check(runtime != null, "actual QuickJS runtime");
        try
        {
            var context = VM.JS_NewContext(runtime);
            Check(context != null, "actual QuickJS context");
            try
            {
                Evaluate(context, "", "undefined");
                Evaluate(context, "40+2", "42");
                foreach (string text in new[] { "", "é水", "😀", "\ud800", "\udc00", "A\0B", new string('a', 600) })
                    Evaluate(context, "'" + text + "'", text);
            }
            finally { if (context != null) VM.JS_FreeContext(context); }
        }
        finally { if (runtime != null) VM.JS_FreeRuntime(runtime); }
        Console.WriteLine($"QuickJsText actual-source boundary passed assertions={assertions}");
        return 0;
    }
}
