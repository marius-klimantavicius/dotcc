pub const State = struct { bias: u32, calls: u32 };
pub const VTable = struct {
    run: *const fn (*State, u32) u32,
    cookie: u64,
};
pub const Handle = struct { context: *State, vtable: *const VTable };
pub const vtable: VTable = .{ .run = run, .cookie = 123456789 };
fn run(state: *State, value: u32) u32 { state.calls += 1; return state.bias + value; }
pub fn make(state: *State) Handle { return .{ .context = state, .vtable = &vtable }; }
