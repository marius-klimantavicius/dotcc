using System;
using System.IO;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public class StaticLocalObjectIdentityTests
{
    [Fact]
    public void Same_basename_objects_qualify_static_aliases_by_full_source_identity()
    {
        var root=Path.Combine(Path.GetTempPath(),"dotcc-static-identity-"+Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root,"left"));Directory.CreateDirectory(Path.Combine(root,"right"));
            var left=Path.Combine(root,"left","same.c");var right=Path.Combine(root,"right","same.c");
            File.WriteAllText(left,"int exported_left=3; int left(void){static int once=0;return ++once;}");
            File.WriteAllText(right,"int exported_right=4; int right(void){static int once=0;return ++once;}");
            var first=Compiler.EmitObject(left);var second=Compiler.EmitObject(right);
            var pattern=@"Globals\.(once__s0__unit_[A-F0-9]+) = 0;";
            var firstName=Regex.Match(first,pattern).Groups[1].Value;
            var secondName=Regex.Match(second,pattern).Groups[1].Value;
            firstName.ShouldNotBeEmpty();secondName.ShouldNotBeEmpty();firstName.ShouldNotBe(secondName);
            first.ShouldContain(firstName+";");second.ShouldContain(secondName+";");
            first.ShouldContain("exported_left = 3;");second.ShouldContain("exported_right = 4;");
            Compiler.EmitObject(left).ShouldBe(first);
            var direct=Compiler.EmitCSharp(new[]{left,right},emit:EmitMode.ManagedLib);
            direct.ShouldContain("once__s0 = 0;");direct.ShouldContain("once__s1 = 0;");
        }
        finally { if(Directory.Exists(root))Directory.Delete(root,true); }
    }
}
