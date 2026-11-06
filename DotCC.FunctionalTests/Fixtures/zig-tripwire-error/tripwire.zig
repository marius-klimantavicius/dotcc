const builtin = @import("builtin");
const assert = @import("std").debug.assert;

fn module(comptime P: type, comptime E: anytype) type {
    return struct {
        pub const FailPoint = P;
        pub const Error = err: {
            const T = if (@TypeOf(E) == type) E else @TypeOf(E);
            break :err switch (@typeInfo(T)) {
                .error_set => E,
                .error_union => |info| info.error_set,
                .@"fn" => |info| @typeInfo(info.return_type.?).error_union.error_set,
                else => @compileError("E must be an error set or function type"),
            };
        };
        pub const enabled = builtin.is_test;
        comptime {
            assert(@typeInfo(FailPoint) == .@"enum");
            assert(@typeInfo(Error) == .error_set);
        }
        pub fn check(point: FailPoint) Error!void {
            if (comptime !enabled) return;
            _ = point;
        }
        pub fn forward(err: Error) Error!void { return err; }
    };
}

var effects: u8 = 0;
fn seed(which: bool) error{First}!u8 {
    effects += 1;
    if (which) return error.First;
    return 2;
}
fn other() error{Second}!void { effects += 10; return error.Second; }
const Fn = module(enum { allocation }, seed);
const Other = module(enum { allocation }, other);
const Set = module(enum { allocation }, error{Third});
const Union = module(enum { allocation }, error{Fourth}!u8);
fn Generic(comptime Failure: type) type {
    return struct { pub fn operation() Failure!u8 { return error.First; } };
}
const GenericSeed = Generic(error{First});
const Nested = module(enum { allocation }, GenericSeed.operation);
const Later = struct {
    pub const Kind = Fn.Error;
    pub const OtherKind = Other.Error;
};
fn lazyModule(comptime E: anytype) type {
    return struct {
        pub const Error = err: {
            const T = if (@TypeOf(E) == type) E else @TypeOf(E);
            break :err switch (@typeInfo(T)) {
                .error_set => E,
                .error_union => |info| info.error_set,
                .@"fn" => |info| @typeInfo(info.return_type.?).error_union.error_set,
                else => @compileError("E must be an error set or function type"),
            };
        };
    };
}
const Lazy = lazyModule(seed);
fn demand(comptime E: type) u8 {
    _ = E;
    const Captured = Lazy.Error;
    const err: Captured = error.First;
    return if (err == error.First) 42 else 0;
}
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    effects = 0;
    Fn.check(.allocation) catch return 1;
    Set.check(.allocation) catch return 2;
    Union.check(.allocation) catch return 3;
    Other.check(.allocation) catch return 4;
    Nested.check(.allocation) catch return 13;
    const a: Later.Kind = error.First;
    const b: Later.OtherKind = error.Second;
    Fn.forward(a) catch |err| { if (err != error.First) return 5; };
    Other.forward(b) catch |err| { if (err != error.Second) return 6; };
    Set.forward(error.Third) catch |err| { if (err != error.Third) return 7; };
    Union.forward(error.Fourth) catch |err| { if (err != error.Fourth) return 8; };
    // Reflection must never call either function operand.
    if (effects != 0) return 9;
    if (demand(u64) != 42) return 10;
    if (Fn.Error != error{First} or Set.Error != error{Third}) return 11;
    if (Fn.Error == Other.Error or Set.Error == Union.Error) return 12;
    if (Nested.Error != error{First}) return 14;
    return 42;
}
