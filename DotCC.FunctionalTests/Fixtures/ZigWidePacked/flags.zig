// Same field kinds and widths as original Terminal.flags (native c_abi=true).
const Redraw = enum(u2) { false, true, unknown };
const Event = enum(u32) { none, normal, button, any };
const Format = enum(u32) { x10, utf8, sgr, urxvt, sgr_pixels };
const Checksum = packed struct(u5) { a: bool = true, b: bool = false, c: bool = true, d: bool = false, e: bool = true };
const Dirty = packed struct { a: bool = true, b: bool = false, c: bool = false, d: bool = false, e: bool = true };
const Flags = packed struct {
    redraw: Redraw = .true,
    modify: bool = false,
    event: Event = .none,
    format: Format = .x10,
    shift: enum(u2) { null, false, true } = .null,
    checksum: Checksum = .{},
    focused: bool = true,
    visible: bool = true,
    resize: bool = true,
    password: bool = false,
    selection: bool = false,
    search: bool = false,
    dirty: Dirty = .{},
};
pub fn main() u8 {
    if (@bitSizeOf(Flags) != 85 or @sizeOf(Flags) != 16 or @alignOf(Flags) != 16) return 1;
    var flags: Flags = .{};
    if (flags.redraw != .true or flags.modify or flags.event != .none or flags.format != .x10 or flags.shift != .null) return 2;
    if (!flags.checksum.a or !flags.checksum.c or !flags.checksum.e or flags.checksum.b or flags.checksum.d) return 3;
    if (!flags.focused or !flags.visible or !flags.resize or flags.password or flags.selection or flags.search) return 4;
    flags.format = .sgr_pixels;
    flags.event = .any;
    flags.shift = .true;
    flags.checksum.d = true;
    flags.dirty.b = true;
    flags.focused = false;
    if (flags.format != .sgr_pixels or flags.event != .any or flags.shift != .true or flags.focused) return 5;
    if (!flags.checksum.d or !flags.checksum.a or !flags.dirty.b or !flags.dirty.a or !flags.dirty.e) return 6;
    const bits: u85 = @bitCast(flags);
    if (((bits >> 35) & 0xffffffff) != 4 or ((bits >> 67) & 3) != 2) return 7;
    var copy = flags;
    copy.dirty.a = false;
    if (!flags.dirty.a or copy.dirty.a) return 8;
    return 42;
}
