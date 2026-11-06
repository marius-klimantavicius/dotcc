comptime {
    const linkage = if (true) .weak else .strong;
    @export(&answer, .{ .name = "weak_hidden_answer", .linkage = linkage, .visibility = .hidden });
}
fn answer() callconv(.c) u8 { return 42; }
pub fn main() u8 { return answer(); }
