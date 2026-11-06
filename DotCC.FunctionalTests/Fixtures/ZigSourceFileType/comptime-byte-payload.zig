const Value = union(enum) { bad: void, text: []const u8 };
const Entry = struct { value: Value };
const Result = struct { bytes: []const u8 };
fn keep(comptime bytes: []const u8) []const u8 {
    if (@TypeOf(bytes) != []const u8 or @TypeOf(bytes[0]) != u8)
        @compileError("comptime byte slice lost its declared type");
    return bytes;
}
fn build() Result {
    comptime {
        const entries = [_]Entry{ .{ .value = .{ .text = "a\x00z" } } };
        var output: []const u8 = "";
        for (entries[0..]) |entry| {
            const text = switch (entry.value) {
                .bad => @compileError("unselected string payload"),
                .text => |value| keep(value),
            };
            const retained = keep(text[1..3]);
            output = retained;
        }
        return .{ .bytes = output };
    }
}
const result = build();
pub fn main() u8 {
    if (result.bytes.len != 2 or result.bytes[0] != 0 or result.bytes[1] != 'z') return 1;
    return 42;
}
