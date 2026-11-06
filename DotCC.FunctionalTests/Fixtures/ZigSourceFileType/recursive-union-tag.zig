const provider = @import("recursive-tag-provider.zig");
pub fn main() u8 {
    const Tag = u64;
    _ = Tag;
    const op: provider.Operation = .{ .read = .{ .value = 42 } };
    const early: provider.Early = .{ .kind = .close };
    if (early.kind != .close) return 5;
    const pending: provider.Operation.Pending = .{ .kind = .write, .value = 42 };
    if (pending.kind != .write or op.read.value != 42) return 1;
    if (@typeInfo(provider.Operation).@"union".field_names.len != 3) return 2;
    if (@typeInfo(provider.Operation.Tag).@"enum".field_names.len != 3) return 3;
    const explicit: provider.Explicit = .{ .first = 42 };
    const payload: provider.Explicit.Payload = .{ .tag = .last, .value = 42 };
    if (payload.tag != .last or @intFromEnum(payload.tag) != 9 or explicit.first != 42) return 4;
    return op.read.value;
}
