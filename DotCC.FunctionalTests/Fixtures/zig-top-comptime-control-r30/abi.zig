const std = @import("std");
pub const calling_conv: std.lang.CallingConvention = .c;
pub const ScreenExtra = extern struct {
    size: usize,
    cursor: bool,
    style: bool,
    hyperlink: bool,
    protection: bool,
    kitty_keyboard: bool,
    charsets: bool,
};
pub const Extra = extern struct {
    size: usize,
    palette: bool,
    modes: bool,
    scrolling_region: bool,
    tabstops: bool,
    pwd: bool,
    keyboard: bool,
    screen: ScreenExtra,
};
pub const Options = extern struct {
    size: usize,
    emit: c_int,
    unwrap: bool,
    trim: bool,
    extra: Extra,
    selection: ?*const anyopaque,
};
