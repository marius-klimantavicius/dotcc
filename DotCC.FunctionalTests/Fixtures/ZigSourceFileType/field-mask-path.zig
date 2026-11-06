const std = @import("std");
const assert = std.debug.assert;

fn fieldMask(
    comptime T: type,
    comptime fields: []const []const u8,
) @typeInfo(T).@"struct".backing_integer.? {
    // Backing int of the packed struct
    const Int = @typeInfo(T).@"struct".backing_integer.?;

    comptime var mask: Int = 0;
    inline for (fields) |path| {
        mask |= fieldMaskPath(T, path, 0, Int);
    }

    return mask;
}

// Recursively walk the same dot-separated path, retaining each intermediate
// type as a const rather than mutating a comptime type and split iterator.
fn fieldMaskPath(
    comptime T: type,
    comptime path: []const u8,
    comptime offset: comptime_int,
    comptime Int: type,
) Int {
    const end = comptime std.mem.indexOfScalar(u8, path, '.') orelse path.len;
    const name = path[0..end];
    comptime assert(switch (@typeInfo(T)) {
        .@"union" => |u| u.layout == .@"packed",
        else => true,
    });
    const field_offset = switch (@typeInfo(T)) {
        .@"struct" => @bitOffsetOf(T, name),
        .@"union" => 0,
        else => @compileError("invalid field path: " ++ path),
    };
    const Field = @FieldType(T, name);
    if (comptime end < path.len) {
        return fieldMaskPath(Field, path[end + 1 ..], offset + field_offset, Int);
    }
    const Ones = @Int(.unsigned, @bitSizeOf(Field));
    return @as(Int, std.math.maxInt(Ones)) << (offset + field_offset);
}

const Inner = packed struct(u8) { low: u3, high: u5 };
const Payload = packed union { raw: u8, parts: Inner };
const Record = packed struct(u16) { first: u3, payload: Payload, last: u5 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Reflected = @FieldType(Inner, "high");
    if (@bitSizeOf(Reflected) != 5) return 5;
    const nested = comptime fieldMask(Record, &.{"payload.parts.high"});
    const combined = comptime fieldMask(Record, &.{"first", "payload", "last"});
    const duplicated = comptime fieldMask(Record, &.{"first", "first", "last"});
    const empty = comptime fieldMask(Record, &.{});
    if (nested != 0x07C0) return 1;
    if (combined != 65535) return 2;
    if (duplicated != 0xF807) return 3;
    if (empty != 0) return 4;
    return 42;
}
