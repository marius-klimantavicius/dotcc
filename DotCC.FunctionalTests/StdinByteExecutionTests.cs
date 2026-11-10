using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class StdinByteExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Process_pipe_preserves_UTF8_binary_bytes_pushback_and_EOF(bool objects)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-stdin-bytes-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, Source);
            string emitted;
            if (objects)
            {
                string fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib);
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(p => MetadataReference.CreateFromFile(p));
            var compilation = CSharpCompilation.Create("StdinProbe", [CSharpSyntaxTree.ParseText(
                emitted + "\npublic static class StdinEntry { public static int Main() => DotCcLib.main(); }",
                new CSharpParseOptions(LanguageVersion.Preview))], references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication, allowUnsafe: true));
            string dll = Path.Combine(directory, "probe.dll");
            var result = compilation.Emit(dll);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            File.WriteAllText(Path.Combine(directory, "probe.runtimeconfig.json"),
                """{"runtimeOptions":{"tfm":"net10.0","framework":{"name":"Microsoft.NETCore.App","version":"10.0.0"}}}""");
            var info = new ProcessStartInfo("dotnet") { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
            info.ArgumentList.Add(dll);
            using var process = Process.Start(info)!;
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            // Raw OS pipe: Arabic, supplementary Unicode, NUL and invalid UTF-8.
            byte[] payload = [.. Encoding.UTF8.GetBytes("ءب😀\n"), 0, 0xff, 0xc0, 0x80, .. Encoding.ASCII.GetBytes(" 42Q")];
            process.StandardInput.BaseStream.Write(payload);
            process.StandardInput.BaseStream.Flush();
            process.StandardInput.Close();
            if (!process.WaitForExit(15000)) { process.Kill(entireProcessTree: true); throw new TimeoutException("stdin child"); }
            process.ExitCode.ShouldBe(0, stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
            stdout.GetAwaiter().GetResult().Trim().ShouldBe("stdin bytes, pushback and EOF ok");
            stderr.GetAwaiter().GetResult().ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private const string Source = """
        #include <stdio.h>
        int main(void) {
            unsigned char want[] = {0xd8,0xa1,0xd8,0xa8,0xf0,0x9f,0x98,0x80,10,0,255,192,128};
            if (fgetc(stdin) != want[0]) return 1;
            if (ungetc(want[0], stdin) != want[0]) return 2;
            char line[16];
            if (!fgets(line, sizeof line, stdin)) return 3;
            for (int i=0;i<9;i++) if ((unsigned char)line[i] != want[i]) return 4;
            if (line[9] || feof(stdin)) return 5;
            unsigned char tail[4];
            if (fread(tail, 1, 4, stdin) != 4) return 6;
            for (int i=0;i<4;i++) if (tail[i] != want[i+9]) return 7;
            int number = 0;
            if (fscanf(stdin, "%d", &number) != 1 || number != 42) return 12;
            if (fgetc(stdin) != 'Q') return 13; /* formatted lookahead shares bytes */
            if (fgetc(stdin) != EOF || !feof(stdin) || ferror(stdin)) return 8;
            if (ungetc('X', stdin) != 'X' || feof(stdin)) return 9;
            if (fgetc(stdin) != 'X' || fgetc(stdin) != EOF || !feof(stdin)) return 10;
            clearerr(stdin);
            if (feof(stdin) || ferror(stdin)) return 11;
            puts("stdin bytes, pushback and EOF ok"); return 0;
        }
        """;
}
