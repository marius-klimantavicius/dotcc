using System;
using System.Text;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

// Isolated C descriptor input, avoiding managed character-reader injection.
internal sealed unsafe class StdinBytes : IDisposable
{
    private readonly RuntimeContext owner = new();
    private readonly RuntimeBinding binding;

    public StdinBytes(string text)
    {
        binding = owner.Enter();
        FILE* input = tmpfile();
        if (input == null) throw new InvalidOperationException("stdin temporary file");
        byte[] data = Encoding.UTF8.GetBytes(text);
        fixed (byte* p = data)
            if (fwrite(p, 1, data.Length, input) != data.Length) throw new InvalidOperationException("stdin write");
        rewind(input);
        if (dup2(fileno(input), 0) != 0) throw new InvalidOperationException("stdin dup2");
        fclose(input);
    }

    public void Dispose() { binding.Dispose(); owner.Dispose(); }
}
