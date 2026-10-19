namespace DotCC;

// A block comment is one preprocessing whitespace token even when it contains
// physical newlines. Keep its byte width (and the original source map), but
// prevent those internal newlines from ending a directive in the token reader.
internal static class CCommentLogicalLines
{
    internal static bool Normalize(byte[] text)
    {
        bool changed = false;
        for (int i = 0; i < text.Length; ++i)
        {
            if (text[i] is (byte)'"' or (byte)'\'')
            {
                byte quote = text[i];
                while (++i < text.Length)
                {
                    if (text[i] == '\\') ++i;
                    else if (text[i] == quote) break;
                }
                continue;
            }
            if (text[i] != '/' || i + 1 >= text.Length) continue;
            if (text[i + 1] == '/')
            {
                while (++i < text.Length && text[i] != '\n') { }
                continue;
            }
            if (text[i + 1] != '*') continue;
            int end = i + 2;
            while (end + 1 < text.Length && !(text[end] == '*' && text[end + 1] == '/')) ++end;
            if (end + 1 >= text.Length) break; // Leave malformed input to the lexer.
            for (int j = i + 2; j < end; ++j)
                if (text[j] is (byte)'\n' or (byte)'\r')
                {
                    text[j] = (byte)' ';
                    changed = true;
                }
            i = end + 1;
        }
        return changed;
    }
}
