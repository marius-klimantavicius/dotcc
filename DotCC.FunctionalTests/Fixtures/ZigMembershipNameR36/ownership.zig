const owner = @import("owner.zig");
const Target = struct { pub const answer: u8 = 42; };
fn query(choose: bool) u8 {
    _ = choose;
    return if (@hasDecl(Target, owner.selected)) Target.answer else 1;
}
pub fn main() u8 { return query(true); }
