using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class EmptyTranslationUnitTests
{
    [Theory]
    [InlineData("")]
    [InlineData("/* no declarations */")]
    [InlineData("#ifdef DISABLED_FEATURE\nint feature(void) { return 1; }\n#endif\n")]
    public void Empty_selected_units_emit_normal_objects(string source)
    {
        var path=Path.Combine(Path.GetTempPath(),$"dotcc-empty-unit-{Guid.NewGuid():N}.c");
        File.WriteAllText(path,source);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
