const api = @import("imported.zig");
const Names = api.Parent.Names;
const Value = union { first: u32, last: u32 };
const Declaration = struct { T: type, names: ?type, marker: u8 };

fn initUnion(comptime T: type, comptime field_names_T: ?type) Declaration {
    if (@typeInfo(T) != .@"union") @compileError("constructor requires its source union type");
    if (field_names_T) |Tag| {
        return .{ .T = T, .names = field_names_T, .marker = if (Tag == Names) 17 else 23 };
    } else return .{ .T = T, .names = field_names_T, .marker = 9 };
}
const named = initUnion(Value, api.Parent.Names);
const unnamed = initUnion(Value, null);
fn checkedMarker(comptime declaration: Declaration) u8 {
    return declaration.marker;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (checkedMarker(named) != 17 or checkedMarker(unnamed) != 9) return 2;
    return 42;
}
