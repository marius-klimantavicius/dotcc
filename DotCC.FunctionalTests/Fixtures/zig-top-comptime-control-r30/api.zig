fn add(value: c_int) callconv(.c) c_int { return value + 2; }
pub const same = add;
const poison = @compileError("UNUSED_API_DECLARATION");
comptime {
    var counter: usize = 0;
    defer { if (counter != 3) @compileError("DEFER_COUNTER"); }
    while (counter < 3) : (counter += 1) {
        if (counter > 2) @compileError("WHILE_UNTAKEN");
    }
    for (0..3) |index| {
        if (index > 2) @compileError("FOR_UNTAKEN");
    }
    @export(&add, .{ .name = "zig_top_add" });
    @export(&same, .{ .name = "zig_top_add_alias" });
}
pub fn pointerIdentity() bool { return &add == &same; }
const abi = @import("abi.zig");
fn optionsRoundtrip(options: abi.Options) callconv(abi.calling_conv) abi.Options {
    var result = options;
    result.emit += 2;
    return result;
}
comptime {
    if (@sizeOf(abi.Options) != 56) @compileError("OPTIONS_SIZE");
    if (@offsetOf(abi.Options, "extra") != 16) @compileError("OPTIONS_EXTRA_OFFSET");
    if (@offsetOf(abi.Options, "selection") != 48) @compileError("OPTIONS_SELECTION_OFFSET");
    @export(&optionsRoundtrip, .{ .name = "zig_top_options" });
    @export(&optionsRoundtrip, .{ .name = "zig_top_options_alias" });
}
pub fn aggregateAnswer() c_int {
    const options: abi.Options = .{
        .size = 56, .emit = 40, .unwrap = false, .trim = true,
        .extra = .{
            .size = 32, .palette = false, .modes = true, .scrolling_region = false,
            .tabstops = true, .pwd = false, .keyboard = true,
            .screen = .{ .size = 16, .cursor = true, .style = false, .hyperlink = true,
                .protection = false, .kitty_keyboard = true, .charsets = false },
        },
        .selection = null,
    };
    const result = optionsRoundtrip(options);
    if (options.emit != 40 or result.size != 56 or result.extra.size != 32 or result.extra.screen.size != 16) return 3;
    if (!result.extra.screen.cursor or result.extra.screen.style) return 4;
    return result.emit;
}
