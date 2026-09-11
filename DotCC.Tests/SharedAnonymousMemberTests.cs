#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class SharedAnonymousMemberTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Repeated_shared_header_uses_one_canonical_anonymous_member_route(bool reverse)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-shared-anonymous-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "shared.h"), """
            struct shared {
                union { struct { int value; } item; int bits; };
                enum state { READY = 7 } state;
            };
            int read_shared(struct shared *value);
            """);
        var first = Path.Combine(directory, "first.c");
        var second = Path.Combine(directory, "second.c");
        File.WriteAllText(first, """
            #include "shared.h"
            int main(void) { struct shared value; value.item.value = 9; value.state = READY; return read_shared(&value) - 16; }
            """);
        File.WriteAllText(second, """
            struct unrelated { struct { int padding; } member; };
            #include "shared.h"
            int read_shared(struct shared *value) { return value->item.value + READY; }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(reverse ? new[] { second, first } : new[] { first, second });
            Regex.Matches(emitted, @"__anon___Anon\d+").Select(match => match.Value).Distinct().Count().ShouldBe(1);
            emitted.ShouldContain("READY");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
