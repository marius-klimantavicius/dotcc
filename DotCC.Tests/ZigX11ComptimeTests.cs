using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Text;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigX11ComptimeTests
{
    [Fact]
    public void Actual_X11_table_preserves_all_resource_bytes_and_map_lookups_at_comptime()
    {
        var graphPath = Environment.GetEnvironmentVariable("DOTCC_X11_GRAPH");
        var view = Environment.GetEnvironmentVariable("DOTCC_X11_VIEW");
        if (string.IsNullOrEmpty(graphPath) || string.IsNullOrEmpty(view))
            Assert.Skip("Set DOTCC_X11_GRAPH and DOTCC_X11_VIEW to the actual campaign module graph and source-view root.");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-actual-x11-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var rows = File.ReadAllText(Path.Combine(Path.GetDirectoryName(view)!, "terminal", "res", "rgb.txt"))
                .Split('\n').Select(line => line.TrimEnd('\r')).Where(line => line.Length != 0).ToArray();
            ulong digest = 14695981039346656037;
            foreach (var row in rows)
            {
                var bytes = Encoding.ASCII.GetBytes(row[12..].Trim(' ', '\t')).Concat(new byte[] { 0 })
                    .Concat(new[] { 0, 4, 8 }.Select(start => byte.Parse(row.AsSpan(start, 3).Trim(), CultureInfo.InvariantCulture)));
                foreach (var resourceByte in bytes) digest = unchecked((digest ^ resourceByte) * 1099511628211);
            }
            var path = Path.Combine(directory, "proof.zig");
            File.WriteAllText(path, """
                const terminal = @import("ghostty");
                const std = @import("std");
                fn check() u8 {
                    @setEvalBranchQuota(40_000_000);
                    if ((std.fmt.parseInt(u8, " 0", 10))) |_| return 6 else |err| {
                        if (err != error.InvalidCharacter) return 7;
                    }
                    if ((std.fmt.parseInt(u8, "256", 10))) |_| return 8 else |err| {
                        if (err != error.Overflow) return 9;
                    }
                    const valid = std.fmt.parseInt(u8, "255", 10) catch return 10;
                    if (valid != 255) return 11;
                    const entries = terminal.x11_color.entries;
                    if (entries.len != __EXPECTED_COUNT__) return 1;
                    var hash: u64 = 14695981039346656037;
                    for (entries) |entry| {
                        for (entry.name) |byte| hash = (hash ^ byte) *% 1099511628211;
                        if (entry.name.ptr[entry.name.len] != 0) return 2;
                        hash = hash *% 1099511628211;
                        for ([_]u8{entry.color.r, entry.color.g, entry.color.b}) |byte| hash = (hash ^ byte) *% 1099511628211;
                        const got = terminal.x11_color.map.get(entry.name) orelse return 3;
                        if (got.r != entry.color.r or got.g != entry.color.g or got.b != entry.color.b) return 4;
                    }
                    if (hash != __EXPECTED_DIGEST__) return 5;
                    if (terminal.x11_color.map.get("nosuchcolor") != null) return 12;
                    const forest = terminal.x11_color.map.get("FoReStGReen") orelse return 13;
                    if (forest.r != 34 or forest.g != 139 or forest.b != 34) return 14;
                    return 42;
                }
                pub fn proof() u8 { const value = comptime check(); return value; }
                """.Replace("__EXPECTED_COUNT__", rows.Length.ToString(CultureInfo.InvariantCulture))
                    .Replace("__EXPECTED_DIGEST__", digest.ToString(CultureInfo.InvariantCulture)));
            var graph = ZigCompilationOptions.Load(graphPath);
            var modules = graph.Modules.Select(module => module.Name == graph.RootModule
                ? new ZigModuleDefinition("ghostty", view, module.Dependencies)
                : module).ToList();
            modules.Add(new ZigModuleDefinition("proof", path, [new("ghostty", "ghostty")]));
            var options = new ZigCompilationOptions("proof", modules, outputMode: ZigSourceOutputMode.Lib,
                zigLibDir: graph.ZigLibDir, optimizeMode: graph.OptimizeMode, stdlibTypes: ZigStdlibTypeModel.Source,
                functionPointerAbi: ZigFunctionPointerAbi.Managed);
            var ir = new ZigFrontend().BuildIr(new FrontendRequest([path], ZigOptions: options));
            var proof = ir.Functions.Single(function => function.Sym.Name == "proof");
            var value = ir.EvalComptimeValue(new Call(proof.Sym.Name, []) { CalleeSym = proof.Sym, Type = CType.UChar });
            value.ShouldBeOfType<IrBuilder.CtInt>().Value.ShouldBe(42);
        }
        finally { Directory.Delete(directory, true); }
    }
}
