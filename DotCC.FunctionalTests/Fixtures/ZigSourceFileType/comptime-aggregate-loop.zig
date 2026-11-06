const Value = union(enum) { invalid: void, boolean: void, numeric: u8 };
const Capability = struct { value: Value };
const Source = struct {
    capabilities: []const Capability,
    fn build(comptime self: @This()) Result {
        comptime {
            var entries: [3]Entry = @splat(.{ .value = 0 });
            for (self.capabilities, 3..) |cap, i| {
                entries[i - 3] = .{ .value = switch (cap.value) {
                    .invalid => @compileError("unselected capability"),
                    .boolean => 1,
                    .numeric => |value| value,
                } };
            }
            for (&entries) |*entry| {
                entry.value += 1;
                const width = entry.value;
                var bytes: [width]u8 = @splat(0);
                bytes[width - 1] = entry.value;
                entry.value = bytes[width - 1];
            }
            const final = entries;
            return .{ .entries = &final };
        }
    }
};
const Entry = struct { value: u8 };
const Result = struct { entries: []const Entry };
const all = [_]Capability{
    .{ .value = .{ .invalid = {} } },
    .{ .value = .{ .boolean = {} } },
    .{ .value = .{ .numeric = 17 } },
    .{ .value = .{ .numeric = 21 } },
};
const source: Source = .{ .capabilities = all[1..] };
const result = source.build();
pub fn main() u8 {
    if (result.entries[0].value != 2) return 1;
    if (result.entries[1].value != 18) return 2;
    return result.entries[0].value + result.entries[1].value + result.entries[2].value;
}
