fn missing() ?usize { return null; }
pub const count: usize = missing() orelse fallback: {
    const result: usize = 3;
    break :fallback result;
};
