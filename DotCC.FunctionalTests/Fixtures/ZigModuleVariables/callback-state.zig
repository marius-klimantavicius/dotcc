const Callback = *const fn (*u32, u32) void;
const Global = struct { userdata: ?*u32 = null, callback: ?Callback = null, calls: u32 = 0 };
var global: Global = .{};
pub var override: ?Callback = selected: {
    if (true) break :selected null;
    break :selected &add;
};
fn add(value: *u32, amount: u32) void { value.* += amount; }
fn twice(value: *u32, amount: u32) void { value.* += amount * 2; }
pub var direct: Callback = &add;
pub fn bare(value: *u32) void { direct(value, 1); }
pub fn set(value: *u32) void { global.userdata = value; global.callback = &add; override = &add; }
pub fn replace() void { global.callback = &twice; }
pub fn invoke(amount: u32) void {
    if (global.callback) |callback| {
        callback(global.userdata.?, amount);
        global.calls += 1;
    }
}
pub fn count() u32 { return global.calls; }
pub fn fallback(value: *u32) void {
    const callback = override orelse return;
    callback(value, 1);
}
