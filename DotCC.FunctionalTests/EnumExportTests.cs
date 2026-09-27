using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class EnumExportTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Managed_consumer_uses_profile_exports_as_compile_time_constants(bool objects, bool nested)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-enum-consumer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "tags.c");
            File.WriteAllText(source, """
                enum { TAG_NEGATIVE = -9, TAG_EXCEPTION = 6, TAG_FLOAT64 = 8, event = 7 };
                enum Wide : unsigned int { TAG_MAX = 4294967295U };
                int read_tag(void) { return TAG_EXCEPTION; }
                """);
            var profilePath = Path.Combine(directory, "overrides.json");
            File.WriteAllText(profilePath, """{"version":1,"enumExports":["TAG_*","event"]}""");
            var profile = CPreprocessingOptions.Load(profilePath);
            var options = new CSharpOutputOptions(NestTypes: nested, Runtime: RuntimeProfile.C);
            string generated;
            if (objects)
            {
                var obj = Path.Combine(directory, "tags.obj");
                File.WriteAllText(obj, Compiler.EmitObject(source, dialect: new(2023), preprocessing: profile));
                generated = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib,
                    className: "Engine", namespaceName: "Example", outputOptions: options);
            }
            else generated = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.ManagedLib,
                dialect: new(2023), preprocessing: profile, className: "Engine", namespaceName: "Example", outputOptions: options);
            generated += """

                public static class Consumer
                {
                    private const long ExceptionTag = Engine.TAG_EXCEPTION;
                    public static int Main()
                    {
                        const uint maximum = Engine.TAG_MAX;
                        return Engine.TAG_NEGATIVE == -9 && ExceptionTag == Engine.read_tag()
                            && Engine.TAG_FLOAT64 == 8 && Engine.@event == 7 && maximum == uint.MaxValue ? 0 : 1;
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(generated, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
