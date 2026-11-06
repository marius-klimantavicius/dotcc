const Mode = enum(u8) { selected, other };
const mode: Mode = .selected;
pub const check = switch (mode) {
    .selected => (struct {
        pub fn check(ok: bool) void { if (!ok) unreachable; }
    }).check,
    .other => Poison.check,
};
const Poison = struct {
    pub fn check(_: bool) void { @compileError("UNUSED_FUNCTION_BRANCH"); }
};
