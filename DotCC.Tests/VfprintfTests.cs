#nullable enable
using System;
using System.IO;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Console")]
public sealed unsafe class VfprintfTests
{
    [Fact]
    public void Standard_streams_use_their_redirected_writers_and_count_utf8_bytes()
    {
        var previousOut = Console.Out;
        var previousError = Console.Error;
        using var output = new StringWriter();
        using var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);
        try
        {
            vprintf(L("%s:%d\0"u8), new VaList(new VaArg[] { L("é\0"u8), 7 })).ShouldBe(4);
            vfprintf(stderr, L("error:%d\0"u8), new VaList(new VaArg[] { 42 })).ShouldBe(8);
            output.ToString().ShouldBe("é:7");
            error.ToString().ShouldBe("error:42");
        }
        finally
        {
            Console.SetOut(previousOut);
            Console.SetError(previousError);
        }
    }

    [Fact]
    public void File_output_preserves_embedded_null_and_non_ascii_bytes()
    {
        var stream = tmpfile();
        ((nint)stream).ShouldNotBe((nint)0);
        try
        {
            vfprintf(stream, L("%c%s\0"u8), new VaList(new VaArg[] { 0, L("é\0"u8) })).ShouldBe(3);
            rewind(stream);
            byte* bytes = stackalloc byte[4];
            fread(bytes, 1, 4, stream).ShouldBe(3);
            bytes[0].ShouldBe((byte)0);
            bytes[1].ShouldBe((byte)0xc3);
            bytes[2].ShouldBe((byte)0xa9);
        }
        finally { fclose(stream); }
    }
}
