const Os = enum { linux, other };
const Abi = enum { gnu, other };
const os: Os = .linux;
const abi: Abi = .gnu;
const private = struct {
    pub fn answer(value: u8) u8 { return value + 2; }
};
pub const selected = switch (os) {
    .linux => switch (abi) { .gnu => private.answer, else => {} },
    else => {},
};
