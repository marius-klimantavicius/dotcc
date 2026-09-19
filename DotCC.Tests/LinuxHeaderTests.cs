using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class LinuxHeaderTests
{
    [Theory]
    [InlineData("endian.h")]
    [InlineData("byteswap.h")]
    [InlineData("sys/unistd.h")]
    [InlineData("sys/errno.h")]
    [InlineData("sys/random.h")]
    [InlineData("sys/poll.h")]
    [InlineData("sys/uio.h")]
    [InlineData("netdb.h")]
    [InlineData("sys/ioctl.h")]
    public void Linux_header_is_embedded(string header) => Compiler.SystemHeaders.ContainsKey(header).ShouldBeTrue();

    [Fact]
    public void Linux_network_and_io_declarations_parse_and_bind()
    {
        const string source = """
            #include <netdb.h>
            #include <sys/uio.h>
            #include <sys/random.h>
            #include <sys/ioctl.h>
            #include <sys/errno.h>
            #include <sys/poll.h>
            #include <sys/unistd.h>
            int inspect(struct addrinfo *ai, struct iovec *iov, struct pollfd *p) {
                return ai->ai_family + (int)iov->iov_len + p->events + FIONREAD + GRND_NONBLOCK;
            }
            """;
        WithSource(source, path => Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib).ShouldContain("inspect"));
    }

    [Fact]
    public void Endian_conversions_have_defined_bodies_and_single_evaluation()
    {
        const string source = """
            #include <endian.h>
            #include <byteswap.h>
            unsigned long convert(unsigned long value) {
                return htobe64(value) + htole16(value) + be32toh(value) + bswap_32(value++);
            }
            """;
        WithSource(source, path =>
        {
            var emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib);
            emitted.ShouldContain("__dotcc_bswap_64");
            emitted.ShouldContain("__dotcc_bswap_32");
            emitted.ShouldNotContain("htobe64(");
        });
    }

    [Fact]
    public void Sockaddr_storage_has_linux_size_and_alignment()
    {
        const string source = """
            #include <sys/socket.h>
            #include <stddef.h>
            _Static_assert(sizeof(struct sockaddr_storage) == 128, "storage size");
            _Static_assert(_Alignof(struct sockaddr_storage) == 8, "storage alignment");
            _Static_assert(offsetof(struct sockaddr_storage, ss_family) == 0, "family offset");
            struct sockaddr_storage storage;
            int family(void) { return storage.ss_family; }
            """;
        WithSource(source, path => Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib).ShouldContain("ss_family"));
    }

    private static void WithSource(string source, Action<string> action)
    {
        var root = Path.Combine(Path.GetTempPath(), "dotcc-linux-header-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "main.c");
        File.WriteAllText(path, source);
        try { action(path); }
        finally { Directory.Delete(root, recursive: true); }
    }
}
