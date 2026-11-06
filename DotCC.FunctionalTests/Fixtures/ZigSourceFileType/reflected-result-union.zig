const Kind = enum(u8) { read = 7, poll = 39, excluded = 91 };
const ReadResult = struct { count: u64, extra: u64 };
const Read = struct { value: u64, pub const Result = error{Failed}!ReadResult; };
const Poll = struct { value: i32, pub const Result = i32; };
const Operation = union(Kind) { read: Read, poll: Poll, excluded: noreturn };
const Result = result: {
    const info = @typeInfo(Operation).@"union";
    var names: [info.field_names.len][]const u8 = undefined;
    var types: [info.field_names.len]type = undefined;
    for (info.field_names, info.field_types, &names, &types) |name, Type, *out_name, *out_type| {
        out_name.* = name;
        out_type.* = if (Type == noreturn) noreturn else Type.Result;
    }
    break :result @Union(.auto, @typeInfo(Operation).@"union".tag_type.?, &names, &types, &@splat(.{}));
};
fn read() Read.Result { return .{ .count = 21, .extra = 21 }; }
pub fn main() u8 {
    if (@sizeOf(Result) != 32 or @alignOf(Result) != 8) return 1;
    if (@typeInfo(Result).@"union".tag_type.? != Kind) return 2;
    if (@typeInfo(Result).@"union".field_types[2] != noreturn) return 3;
    const value = @unionInit(Result, "read", read());
    var values: [2]Result = .{ value, @unionInit(Result, "poll", 42) };
    if (@intFromPtr(&values[1]) - @intFromPtr(&values[0]) != @sizeOf(Result)) return 6;
    switch (value) {
        .read => |answer, tag| {
            if (@TypeOf(tag) != Kind or @intFromEnum(tag) != 7) return 4;
            const payload = answer catch return 5;
            return @intCast(payload.count + payload.extra);
        },
        .poll => |answer| return @intCast(answer),
        .excluded => unreachable,
    }
}
