const Build = @import("builder.zig").Build;
const C = Build(c_int, &.{ null, "b", null, "d", null, "f" });
const Z = Build(u3, &.{ null, "b", null, "d", null, "f" });
fn answer(value: C) u8 { return if (value == .f) 42 else 5; }
pub fn main() u8 {
    if (@sizeOf(C) != 4 or @sizeOf(Z) != 1) return 1;
    if (@bitSizeOf(@typeInfo(C).@"enum".tag_type) != 32) return 7;
    if (@bitSizeOf(@typeInfo(Z).@"enum".tag_type) != 3) return 2;
    if (@backingInt(C.b) != 1 or @backingInt(C.d) != 3 or @backingInt(C.f) != 5) return 3;
    if (@typeInfo(C).@"enum".field_names.len != 3) return 4;
    if (@backingInt(Z.b) != 1 or @backingInt(Z.d) != 3 or @backingInt(Z.f) != 5) return 6;
    return answer(.f);
}
