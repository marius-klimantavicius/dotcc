using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class SourceAbiAlignmentExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void X87_typedef_alignment_matches_native_addresses_layout_and_raw_byte_copies(bool objects)
    {
        // GCC LP64 scalar-alignment fixture and stdout, with physical member
        // address checks in addition to the source-level offsetof assertions.
        // All sixteen bytes, including x87 padding, are copied without numeric operations.
        const string source = """
            #include <stddef.h>
            #include <stdint.h>
            #include <stdio.h>
            #include <string.h>
            typedef long double Weak __attribute__((aligned(1)));
            typedef long double Strong __attribute__((aligned(32)));
            struct WeakRecord { char lead; Weak value; char tail; };
            struct StrongRecord { char lead; Strong value; char tail; };
            _Static_assert(sizeof(Weak) == 16 && _Alignof(Weak) == 1, "weak scalar");
            _Static_assert(sizeof(Strong) == 16 && _Alignof(Strong) == 32, "strong scalar");
            _Static_assert(sizeof(long double) == 16 && _Alignof(long double) == 16, "natural scalar");
            _Static_assert(offsetof(struct WeakRecord, value) == 1, "weak value");
            _Static_assert(offsetof(struct WeakRecord, tail) == 17, "weak tail");
            _Static_assert(sizeof(struct WeakRecord) == 18 && _Alignof(struct WeakRecord) == 1, "weak record");
            _Static_assert(offsetof(struct StrongRecord, value) == 32, "strong value");
            _Static_assert(offsetof(struct StrongRecord, tail) == 48, "strong tail");
            _Static_assert(sizeof(struct StrongRecord) == 64 && _Alignof(struct StrongRecord) == 32, "strong record");
            static Weak global_weak;
            static Strong global_strong;
            static struct WeakRecord global_weak_record;
            static struct StrongRecord global_strong_record;
            int main(void) {
                unsigned char bytes[16] = {0,1,2,3,4,5,6,128,255,63,161,162,163,164,165,166};
                Weak local_weak;
                Strong local_strong;
                struct WeakRecord weak_record;
                struct StrongRecord strong_record;
                memcpy(&local_weak, bytes, 16);
                memcpy(&local_strong, bytes, 16);
                memcpy(&global_weak, bytes, 16);
                memcpy(&global_strong, bytes, 16);
                memcpy(&weak_record.value, bytes, 16);
                memcpy(&strong_record.value, bytes, 16);
                memcpy(&global_weak_record.value, bytes, 16);
                memcpy(&global_strong_record.value, bytes, 16);
                weak_record.lead = 7; weak_record.tail = 9;
                strong_record.lead = 11; strong_record.tail = 13;
                if ((unsigned char *)&weak_record.value - (unsigned char *)&weak_record != 1 ||
                    (unsigned char *)&weak_record.tail - (unsigned char *)&weak_record != 17 ||
                    (unsigned char *)&strong_record.value - (unsigned char *)&strong_record != 32 ||
                    (unsigned char *)&strong_record.tail - (unsigned char *)&strong_record != 48 ||
                    (unsigned char *)&global_weak_record.value - (unsigned char *)&global_weak_record != 1 ||
                    (unsigned char *)&global_weak_record.tail - (unsigned char *)&global_weak_record != 17 ||
                    (unsigned char *)&global_strong_record.value - (unsigned char *)&global_strong_record != 32 ||
                    (unsigned char *)&global_strong_record.tail - (unsigned char *)&global_strong_record != 48)
                    return 1;
                printf("scalar %d %d %d %d\n", (int)sizeof(Weak), (int)_Alignof(Weak), (int)sizeof(Strong), (int)_Alignof(Strong));
                printf("weak %d %d %d %d\n", (int)_Alignof(struct WeakRecord), (int)offsetof(struct WeakRecord, value), (int)offsetof(struct WeakRecord, tail), (int)sizeof(struct WeakRecord));
                printf("strong %d %d %d %d\n", (int)_Alignof(struct StrongRecord), (int)offsetof(struct StrongRecord, value), (int)offsetof(struct StrongRecord, tail), (int)sizeof(struct StrongRecord));
                printf("alignment %d %d %d %d\n", (int)((uintptr_t)&local_strong & 31), (int)((uintptr_t)&global_strong & 31), (int)((uintptr_t)&strong_record & 31), (int)((uintptr_t)&global_strong_record & 31));
                printf("bytes %d %d %d %d %d %d %d %d sentinels %d %d %d %d\n", memcmp(&local_weak, bytes, 16) == 0, memcmp(&local_strong, bytes, 16) == 0, memcmp(&global_weak, bytes, 16) == 0, memcmp(&global_strong, bytes, 16) == 0, memcmp(&weak_record.value, bytes, 16) == 0, memcmp(&strong_record.value, bytes, 16) == 0, memcmp(&global_weak_record.value, bytes, 16) == 0, memcmp(&global_strong_record.value, bytes, 16) == 0, (int)weak_record.lead, (int)weak_record.tail, (int)strong_record.lead, (int)strong_record.tail);
                return 0;
            }
            """;
        const string expected = """
            scalar 16 1 16 32
            weak 1 1 17 18
            strong 32 32 48 64
            alignment 0 0 0 0
            bytes 1 1 1 1 1 1 1 1 sentinels 7 9 11 13
            """ + "\n";
        var profile = new CPreprocessingOptions(Array.Empty<MacroOverride>(),
            sourceAbi: new(CLongDoubleRepresentation.X87Storage16));
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-source-abi-alignment-run-" + Guid.NewGuid().ToString("N"));
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
            result.stdout.ReplaceLineEndings("\n").ShouldBe(expected);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
