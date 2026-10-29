using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class SourceAbiStorageExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void X87_storage_uses_real_sixteen_byte_layout_and_byte_copies(bool objects)
    {
        const string source = """
            #include <stdint.h>
            #include <stddef.h>
            #include <stdio.h>
            #include <string.h>
            struct Container { char lead; long double value; char tail; };
            static long double global_value;
            _Static_assert(sizeof(long double) == 16, "storage size");
            _Static_assert(_Alignof(long double) == 16, "storage alignment");
            _Static_assert(sizeof(struct Container) == 48, "container size");
            int main(void) {
                unsigned char bytes[16] = {0,0,0,0,0,0,0,128,255,63,161,162,163,164,165,166};
                long double local_value;
                long double array[2];
                struct Container container;
                memcpy(&local_value, bytes, 16);
                memcpy(&global_value, bytes, 16);
                memcpy(&container.value, bytes, 16);
                memcpy(&array[1], bytes, 16);
                long double copy = local_value;
                printf("abi %d %d %d %d %d\n", (int)sizeof(local_value), (int)_Alignof(long double),
                    (int)sizeof(container), (int)offsetof(struct Container, value), (int)offsetof(struct Container, tail));
                printf("storage %d %d %d %d stride %d bytes %d %d %d %d %d\n",
                    (int)((uintptr_t)&local_value & 15), (int)((uintptr_t)&global_value & 15),
                    (int)((uintptr_t)&container & 15), (int)((uintptr_t)array & 15),
                    (int)((unsigned char *)&array[1] - (unsigned char *)&array[0]),
                    memcmp(&local_value, bytes, 16) == 0, memcmp(&global_value, bytes, 16) == 0,
                    memcmp(&container.value, bytes, 16) == 0, memcmp(&array[1], bytes, 16) == 0,
                    memcmp(&copy, bytes, 10) == 0);
                return 0;
            }
            """;
        var profile = new CPreprocessingOptions(Array.Empty<MacroOverride>(),
            sourceAbi: new(CLongDoubleRepresentation.X87Storage16));
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-source-abi-storage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, source);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path, preprocessing: profile));
                emitted = Compiler.LinkObjects(new[] { fragment });
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, preprocessing: profile);
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.ReplaceLineEndings("\n").ShouldBe(
                "abi 16 16 48 16 32\nstorage 0 0 0 0 stride 16 bytes 1 1 1 1 1\n");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
