using System.Text;

namespace Managed.CPython;

internal static unsafe class CallbackThunks
{
    // Emitted C function pointers use the managed calling convention. This exact
    // static signature is address-taken by TranslatedExports, including in AOT.
    // UnmanagedCallersOnly/Marshal delegate conversion would be the WRONG ABI.
    internal static int Int64(long token, long argument, long* result, byte* error, ulong capacity)
    {
        try
        {
            if (result == null || error == null || capacity == 0) return -1;
            error[0] = 0;
            CallbackOutcome outcome = CallbackRoots.Invoke(token, argument);
            if (outcome.Succeeded) { *result = outcome.Value; return 0; }
            WriteError(outcome.Error ?? "Managed callback failed.", error, capacity);
            return -1;
        }
        catch (Exception)
        {
            // Even a throwing/invalid exception.Message cannot unwind through C.
            if (error != null && capacity > 0)
            {
                ReadOnlySpan<byte> fallback = "Managed callback failed while formatting its error."u8;
                int length = (int)Math.Min((ulong)fallback.Length, capacity - 1);
                fallback[..length].CopyTo(new Span<byte>(error, length));
                error[length] = 0;
            }
            return -1;
        }
    }

    private static void WriteError(string message, byte* error, ulong capacity)
    {
        int available = (int)Math.Min(capacity - 1, int.MaxValue);
        // Convert writes a complete UTF-8 prefix; truncation never splits a
        // multibyte code point. Embedded NULs are visible in the C diagnostic.
        string visible = message.Replace("\0", "\\0", StringComparison.Ordinal);
        var encoding = new UTF8Encoding(false, true);
        encoding.GetEncoder().Convert(visible.AsSpan(), new Span<byte>(error, available), true,
            out _, out int used, out _);
        error[used] = 0;
    }
}
