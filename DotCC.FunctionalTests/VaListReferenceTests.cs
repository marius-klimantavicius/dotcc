using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class VaListReferenceTests
{
    [Theory]
    [InlineData("va-list-reference", "17 27")]
    [InlineData("empty-translation-unit", "selected empty unit linked")]
    public void Object_link_preserves_selected_units_and_boundary_semantics(string name, string expected)
    {
        var fixture=FixtureRunner.Discover().Single(f=>f.name==name);
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-va-reference-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var objects=fixture.sources.Select((source,index)=>{
                var path=Path.Combine(directory,index+".cs");
                File.WriteAllText(path,Compiler.EmitObject(source,includeDirs:new[]{fixture.dir}));
                return path;
            }).ToArray();
            var emitted=Compiler.LinkObjects(objects);
            FixtureRunner.CompileAndRun(emitted,Array.Empty<string>()).Trim().ShouldBe(expected);
        }
        finally { Directory.Delete(directory,true); }
    }
}
