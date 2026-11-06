const lib = @import("unions");
const Payload = struct {
    value: u16,
    pub const C = extern struct { value: u32 };
    pub fn cval(self: @This()) C { return .{ .value = self.value + 2 }; }
};
const Tag = enum(c_int) { empty, number, custom };
const Value = union(Tag) { empty, number: u32, custom: Payload };
fn Api() type { return lib.TaggedUnion(.c, Value, .{ .padding = [8]u64, .field_renames = .{ .custom = "renamed" } }); }
fn ByteApi() type { return lib.TaggedUnion(.c, Value, .{ .padding = [64]u8 }); }
fn convert(value: Value) Api().C { return Api().cval(value); }
pub fn main() u8 {
    if (@sizeOf(Api().CValue) != 64 or @alignOf(Api().CValue) != 8) return 1;
    if (@sizeOf(Api().C) != 72 or @offsetOf(Api().C, "value") != 8) return 2;
    if (@sizeOf(ByteApi().CValue) != 64 or @alignOf(ByteApi().CValue) != 4) return 8;
    if (Api().CValue == ByteApi().CValue) return 9;
    const number = convert(.{ .number = 19 });
    const custom = convert(.{ .custom = .{ .value = 19 } });
    const empty = convert(.empty);
    if (number.tag != .number or custom.tag != .custom or empty.tag != .empty) return 3;
    if (@sizeOf(Api().Value(.empty)) != 0 or @sizeOf(Api().Value(.custom)) != @sizeOf(Payload)) return 4;
    const renamed = Api().C.cFieldRename(.custom) orelse return 5;
    if (renamed.len != 7 or renamed[0] != 'r') return 6;
    if (Api().C.cFieldRename(.empty) != null) return 7;
    return @intCast(number.value.number + custom.value.custom.value + 2);
}
