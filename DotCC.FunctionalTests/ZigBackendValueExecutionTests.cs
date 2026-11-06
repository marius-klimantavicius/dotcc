using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigBackendValueExecutionTests
{
    public const string VoidMembers = """
        var trace: u32 = 0;
        fn mark(n: u32) u32 { trace = trace * 10 + n; return n; }
        fn effect(n: u32) void { _ = mark(n); }
        const Record = struct { first: u32, context: void, last: u32 };
        const Choice = union(enum) { none: void, number: u32 };
        var stored: Record = .{ .first = 8, .context = {}, .last = 9 };
        fn make() Record {
            return .{ .first = mark(1), .context = effect(2), .last = mark(3) };
        }
        fn capture(context: void) Record {
            return .{ .first = 4, .context = context, .last = 5 };
        }
        fn use(c: Choice) u32 { return switch (c) { .none => 7, .number => |n| n }; }
        fn lazy(flag: bool) Record {
            return switch (flag) {
                true => .{ .first = mark(1), .context = effect(2), .last = mark(3) },
                false => .{ .first = mark(4), .context = effect(5), .last = mark(6) },
            };
        }
        pub fn main() u8 {
            const r = make();
            if (trace != 123 or r.first != 1 or r.last != 3) return 1;
            const c = capture({});
            _ = c.context;
            if (c.first != 4 or c.last != 5) return 2;
            const empty: Choice = .{ .none = effect(4) };
            if (trace != 1234 or use(empty) != 7) return 3;
            if (use(.{ .number = 42 }) != 42) return 4;
            trace = 0;
            _ = make().context;
            if (trace != 123) return 5;
            trace = 0;
            const l = lazy(false);
            if (trace != 456 or l.first != 4 or l.last != 6) return 6;
            if (stored.first != 8 or stored.last != 9) return 7;
            return 42;
        }
        """;

    public const string BooleanLoop = """
        fn choose(value: u8) u8 {
            while (true) {
                if (value == 0) return 13;
                return value;
            }
        }
        pub fn main() u8 {
            if (choose(0) != 13) return 1;
            return choose(42);
        }
        """;

    public const string ElseSequence = """
        fn value(n: u8) error{Bad}!u8 { if (n == 0) return error.Bad; return n; }
        fn choose(flag: bool, n: u8) error{Bad}!u8 {
            var result: u8 = 0;
            if (flag) { result = 1; } else result = try value(n);
            return result;
        }
        pub fn main() error{Bad}!u8 {
            if (try choose(true, 0) != 1) return 1;
            if (choose(false, 0)) |_| { return 2; } else |err| { if (err != error.Bad) return 3; }
            return try choose(false, 42);
        }
        """;

    [Theory]
    [InlineData(false, VoidMembers)]
    [InlineData(true, VoidMembers)]
    [InlineData(false, BooleanLoop)]
    [InlineData(true, BooleanLoop)]
    [InlineData(false, ElseSequence)]
    [InlineData(true, ElseSequence)]
    public void Void_storage_and_control_flow_keep_runtime_effects(bool objects, string source)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-backend-values-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, source);
            var options = new ZigCompilationOptions("root", [new("root", path, [])]);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}
