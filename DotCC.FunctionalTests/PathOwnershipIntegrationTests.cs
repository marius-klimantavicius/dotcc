using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class PathOwnershipIntegrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Translated_instances_keep_relative_file_operations_in_their_own_directories(bool objects)
    {
        string directory = Directory.CreateTempSubdirectory("dotcc-owner-path-source-").FullName;
        try
        {
            string source = Path.Combine(directory, "paths.c");
            File.WriteAllText(source, """
                #include <stdio.h>
                #include <unistd.h>
                int change_dir(const char *path) { return chdir(path); }
                int put(int value) {
                    FILE *file = fopen("owner.data", "w");
                    if (!file) return -1;
                    fputc(value, file);
                    return fclose(file);
                }
                int take(void) {
                    FILE *file = fopen("owner.data", "r");
                    if (!file) return -1;
                    int value = fgetc(file);
                    fclose(file);
                    return value;
                }
                """);
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, InstanceMethods: true);
            string generated;
            if (objects)
            {
                string fragment = source + ".o.cs";
                File.WriteAllText(fragment, Compiler.EmitObject(source, outputOptions: new(InstanceMethods: true)));
                generated = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib, className: "PathApi", outputOptions: options);
            }
            else generated = Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, className: "PathApi", outputOptions: options);
            generated += """

                public static unsafe class PathProbe {
                    public static int Main() {
                        string host = global::System.IO.Directory.GetCurrentDirectory();
                        string a = global::System.IO.Directory.CreateTempSubdirectory("dotcc-owner-a-").FullName;
                        string b = global::System.IO.Directory.CreateTempSubdirectory("dotcc-owner-b-").FullName;
                        try {
                            using var first = new PathApi();
                            using var second = new PathApi();
                            using (first.__DotCcEnter())
                            fixed (byte* p = global::System.Text.Encoding.UTF8.GetBytes(a + "\0"))
                                if (first.change_dir(p) != 0) return 1;
                            using (second.__DotCcEnter())
                            fixed (byte* p = global::System.Text.Encoding.UTF8.GetBytes(b + "\0"))
                                if (second.change_dir(p) != 0) return 2;
                            using (first.__DotCcEnter()) if (first.put(41) != 0) return 3;
                            using (second.__DotCcEnter()) if (second.put(72) != 0) return 3;
                            using (first.__DotCcEnter()) if (first.take() != 41) return 4;
                            using (second.__DotCcEnter()) if (second.take() != 72) return 4;
                            first.Dispose();
                            using (second.__DotCcEnter()) if (second.take() != 72) return 5;
                            return global::System.IO.Directory.GetCurrentDirectory() == host ? 0 : 6;
                        } finally {
                            global::System.IO.Directory.SetCurrentDirectory(host);
                            global::System.IO.Directory.Delete(a, true);
                            global::System.IO.Directory.Delete(b, true);
                        }
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(generated, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
