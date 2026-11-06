const build_mode_has_safety = switch (@import("builtin").mode) {
    .debug, .safe => true,
    .fast, .small => false,
};
const safety_checks_hint: union(enum) {
    checked_to_fixed_depth: usize,
    assumed_correct,
} = .{ .checked_to_fixed_depth = 256 };
const safety_checks: @TypeOf(safety_checks_hint) = if (build_mode_has_safety)
    safety_checks_hint
else
    .assumed_correct;
const Container = struct {
    storage: switch (safety_checks) {
        .checked_to_fixed_depth => |count| [(count + 7) >> 3]u8,
        .assumed_correct => void,
    } = switch (safety_checks) {
        .checked_to_fixed_depth => @splat(0),
        .assumed_correct => {},
    },
};
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const Storage = @FieldType(Container, "storage");
    if (build_mode_has_safety) {
        if (Storage != [32]u8) return 1;
    } else {
        if (Storage != void) return 2;
    }
    return 42;
}
