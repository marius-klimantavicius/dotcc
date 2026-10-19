using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class OwnedArrayInitializerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Expression_payloads_initialize_final_storage_through_bounded_helpers(bool objects)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-owned-rows-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "data.c");
            string rows = string.Join(",", Enumerable.Range(0, 1200).Select(i => "{" + i + ",&root.seed,&root.seed}"));
            File.WriteAllText(path, "struct Row { int value; int *first; int *second; }; "
                + "struct Root { int seed; struct Row *rows; }; struct Root root = {.seed=7,.rows=(struct Row[]){" + rows + "}};");
            if (objects)
            {
                string fragment = path + ".o";
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                path = fragment;
            }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, LiteralPool: false);
            var source = objects ? Compiler.LinkObjects([path], emit: EmitMode.ManagedLib, className: "Api", outputOptions: options)
                : Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, className: "Api", outputOptions: options);
            source.ShouldNotContain("new Row[]");
            source.ShouldNotContain("GlobalArrayFrom<Row>");
            source.ShouldNotContain("/*__dotcc_owner_member__*/");
            source.ShouldNotContain("/*__dotcc_bound_initializer_address__*/");
            source.ShouldContain("GlobalArrayZeroed<Row>(1200)");
            source.ShouldContain("root_storage[1199] = new()");
            source.ShouldContain("__dotcc_initializerAddress_");
            Regex.Matches(source, @"void __dotcc_owned_array_\w+_Fill_\d+_\d+\(").Count.ShouldBeGreaterThan(1);
            // Whole rows remain bounded; the C026 containing value stays in place.
            source.ShouldContain("fixed (Root* root_destination");
            source.ShouldNotContain("new Root {");
        }
        finally { Directory.Delete(directory, true); }
    }
}
