using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PosixBaseTypeHeaderTests
{
    [Fact]
    public void Unistd_alone_exposes_POSIX_base_types_in_callback_declarations()
    {
        WithSource("""
            #include <unistd.h>
            struct tracker { void (*track)(void *owner, ssize_t delta); };
            _Static_assert(sizeof(ssize_t) == sizeof(size_t), "signed size width");
            _Static_assert((ssize_t)-1 < 0, "signed size");
            _Static_assert(sizeof(pid_t) == 4, "pid width");
            _Static_assert(sizeof(off_t) == 8, "offset width");
            ssize_t adjust(struct tracker *t, size_t amount) {
                t->track(t, -(ssize_t)amount);
                return -(ssize_t)amount;
            }
            """, output => output.ShouldContain("adjust"));
    }

    [Fact]
    public void Winsize_preserves_Linux_layout_and_request_values()
    {
        WithSource("""
            #include <sys/ioctl.h>
            #include <stddef.h>
            _Static_assert(sizeof(struct winsize) == 8, "window size ABI");
            _Static_assert(_Alignof(struct winsize) == 2, "window size alignment");
            _Static_assert(offsetof(struct winsize, ws_row) == 0, "rows offset");
            _Static_assert(offsetof(struct winsize, ws_col) == 2, "columns offset");
            _Static_assert(offsetof(struct winsize, ws_xpixel) == 4, "horizontal pixel offset");
            _Static_assert(offsetof(struct winsize, ws_ypixel) == 6, "vertical pixel offset");
            _Static_assert(TIOCGWINSZ == 0x5413 && TIOCSWINSZ == 0x5414, "Linux ioctl requests");
            int read_window(int fd, struct winsize *window) { return ioctl(fd, TIOCGWINSZ, window); }
            """, output => output.ShouldContain("ioctl"));
    }

    private static void WithSource(string source, Action<string> inspect)
    {
        var root = Path.Combine(Path.GetTempPath(), "dotcc-posix-base-types-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "main.c");
        File.WriteAllText(path, source);
        try { inspect(Compiler.EmitCSharp([path], emit: EmitMode.Object)); }
        finally { Directory.Delete(root, recursive: true); }
    }
}
