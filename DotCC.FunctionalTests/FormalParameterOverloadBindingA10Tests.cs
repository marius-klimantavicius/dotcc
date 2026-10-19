using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class FormalParameterOverloadBindingA10Tests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void The_C_prototype_controls_host_overloads_and_variadic_promotions(bool objects, bool nested)
    {
        string fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "formal-parameter-overloads-a10");
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-formal-overload-a10-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string source = Path.Combine(fixture, "calls.c");
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, NestTypes: nested);
            var preprocessing = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[] {
                new FunctionOverride("choose", new("unsigned long", new[] { "unsigned long" }),
                    new("managedMethod", "global::FormalHost.Choose"), RequireMatch: true)
            });
            string generated;
            if (objects)
            {
                string obj = Path.Combine(directory, "calls.obj");
                File.WriteAllText(obj, Compiler.EmitObject(source, includeDirs: [fixture], preprocessing: preprocessing));
                generated = Compiler.LinkObjects([obj], emit: EmitMode.ManagedLib,
                    className: "FormalApi", outputOptions: options);
            }
            else generated = Compiler.EmitCSharp([source], includeDirs: [fixture], emit: EmitMode.ManagedLib,
                className: "FormalApi", outputOptions: options, preprocessing: preprocessing);
            generated += "\n" + File.ReadAllText(Path.Combine(fixture, "host.cs.txt"));
            var result = FixtureRunner.CompileAndRunCapturingExit(generated, []);
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe("formal parameters: ordinary/tight/empty/wide, callbacks, promotions, evaluations=2 trace=12");
        }
        finally { Directory.Delete(directory, true); }
    }
}
