using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class UnixSupportHeaderTests
{
    [Theory]
    [InlineData("grp.h")]
    [InlineData("strings.h")]
    [InlineData("termios.h")]
    [InlineData("libgen.h")]
    public void Header_is_embedded(string header) => Compiler.SystemHeaders.ContainsKey(header).ShouldBeTrue();

    [Fact]
    public void Unix_host_declarations_preserve_pointer_returns_and_Linux_layout()
    {
        const string source = """
            #include <grp.h>
            #include <termios.h>
            #include <libgen.h>
            #include <stddef.h>
            _Static_assert(sizeof(struct group) == 32, "LP64 group layout");
            _Static_assert(offsetof(struct group, gr_gid) == 16, "gid offset");
            _Static_assert(offsetof(struct group, gr_mem) == 24, "members offset");
            _Static_assert(sizeof(struct termios) == 60, "Linux termios layout");
            _Static_assert(offsetof(struct termios, c_cc) == 17, "control character offset");
            _Static_assert(offsetof(struct termios, c_ispeed) == 52, "speed offset");
            _Static_assert(ECHO == 8 && ICANON == 2, "terminal flags");
            struct group *lookup(const char *name) { return getgrnam(name); }
            char *parent_path(char *path) { return dirname(path); }
            int terminal_read(int fd, struct termios *attributes) {
                return tcgetattr(fd, attributes);
            }
            """;
        var root = Path.Combine(Path.GetTempPath(), "dotcc-unix-support-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "main.c");
        File.WriteAllText(path, source);
        try
        {
            // Object emission preserves unresolved host imports. This tests the
            // declarations, not an implementation of group or terminal services.
            var emitted = Compiler.EmitCSharp([path], emit: EmitMode.Object);
            emitted.ShouldContain("getgrnam");
            emitted.ShouldContain("dirname");
            emitted.ShouldContain("tcgetattr");
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
