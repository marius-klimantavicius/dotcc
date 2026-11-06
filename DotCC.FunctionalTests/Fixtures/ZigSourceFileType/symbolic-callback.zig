const Callback = *const fn (u8) u8;
const Record = struct { callback: Callback, value: u8 };
fn increment(value: u8) u8 { return value + 1; }
fn decrement(value: u8) u8 { return value - 1; }
const original: Record = .{ .callback = increment, .value = 40 };
const copied = original;
const alias: Callback = copied.callback;
fn invoke(callback: Callback, value: u8) u8 { return callback(value); }
fn evaluate() u8 {
    const erased: *const anyopaque = comptime @ptrCast(&increment);
    const recovered: Callback = @ptrCast(erased);
    var local = copied;
    if (local.callback != alias or alias == decrement or alias != &increment or recovered != alias) return 1;
    local.callback = decrement;
    if (copied.callback != increment or local.callback != decrement) return 2;
    var local_callback = local.callback;
    const pointer = &local_callback;
    if (pointer.* != decrement) return 3;
    pointer.* = alias;
    local.callback = local_callback;
    return invoke(local.callback, invoke(recovered, local.value));
}
const bound = evaluate();
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    var buffer: [bound]u8 = undefined;
    buffer[0] = evaluate();
    if (buffer.len != 42) return 4;
    return buffer[0];
}
