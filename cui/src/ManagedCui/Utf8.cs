using System.Buffers;
using System.Text;
using Raw = global::Managed.Forms.Cui;

namespace Cui.Managed;

internal static class Utf8Validation
{
    internal static void Check(ReadOnlySpan<byte> text)
    {
        while (!text.IsEmpty)
        {
            if (text[0] == 0 || Rune.DecodeFromUtf8(text, out _, out int consumed) != OperationStatus.Done)
                throw new ArgumentException("Supply valid UTF-8 without embedded NUL.", nameof(text));
            text = text[consumed..];
        }
    }
}

public sealed unsafe partial class Ui
{
    private byte* CopyUtf8(ReadOnlySpan<byte> text)
    {
        Utf8Validation.Check(text);
        byte* result = (byte*)arena.Allocate(checked(text.Length + 1));
        text.CopyTo(new Span<byte>(result, text.Length)); result[text.Length] = 0;
        return result;
    }
    /// <summary>Copies validated UTF-8 into stable frame storage, with no managed encoding allocation.</summary>
    public void TextUtf8(ReadOnlySpan<byte> text, TextOpts opts = default)
    {
        CheckFrame();
        Raw.cui_text_opts options = ToNative(opts);
        Raw.cui_text_ex(ctx, CopyUtf8(text), &options); SyncState();
    }
    /// <summary>Builds a button from validated UTF-8. Input storage may be stack-backed.</summary>
    public bool ButtonUtf8(ReadOnlySpan<byte> label, ButtonOpts opts = default)
    {
        CheckFrame();
        Raw.cui_button_opts options = ToNative(opts);
        bool result = Raw.cui_button_ex(ctx, CopyUtf8(label), &options) != 0; SyncState(); return result;
    }
}
