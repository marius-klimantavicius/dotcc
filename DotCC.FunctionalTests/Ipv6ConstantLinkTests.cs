using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class Ipv6ConstantLinkTests
{
    [Fact]
    public void Separately_compiled_ipv6_constant_addresses_have_shared_identity()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-ipv6-link-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var main = Path.Combine(directory, "main.c");
            var other = Path.Combine(directory, "other.c");
            File.WriteAllText(main, """
                #include <netinet/in.h>
                const struct in6_addr *other_loopback(void);
                int main(void) {
                    struct in6_addr a = IN6ADDR_ANY_INIT;
                    struct in6_addr b = IN6ADDR_LOOPBACK_INIT;
                    struct in6_addr c = in6addr_loopback;
                    c.s6_addr[15] = 9;
                    return other_loopback() == &in6addr_loopback && a.s6_addr[15] == 0
                        && b.s6_addr[15] == 1 && c.s6_addr[15] == 9 && in6addr_loopback.s6_addr[15] == 1 ? 0 : 1;
                }
                """);
            File.WriteAllText(other, "#include <netinet/in.h>\nconst struct in6_addr *other_loopback(void) { return &in6addr_loopback; }\n");
            var mainObject = Path.Combine(directory, "main.cs");
            var otherObject = Path.Combine(directory, "other.cs");
            File.WriteAllText(mainObject, Compiler.EmitObject(main));
            File.WriteAllText(otherObject, Compiler.EmitObject(other));
            var linked = Compiler.LinkObjects(new[] { mainObject, otherObject });
            FixtureRunner.CompileAndRunCapturingExit(linked, Array.Empty<string>()).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
