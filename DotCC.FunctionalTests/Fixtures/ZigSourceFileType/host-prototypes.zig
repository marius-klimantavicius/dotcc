pub const fd_t = c_int;
pub const Chosen = if (false) u8 else if (false) c_int else extern struct { a: u64, b: u32 };
pub const MaximumAlignment = if (false) f64 else if (false) c_longdouble else extern struct { a: c_longlong, b: c_longdouble };
pub extern "c" fn write(fd: fd_t, buffer: [*]const u8, count: usize) isize;
pub extern "c" fn abs(value: c_int) c_int;
pub const query = switch (@as(u8, 0)) {
    255 => @compileError("UNTAKEN_ALIAS_ARM"),
    0 => private.abs,
    else => {},
};
pub const unavailable = switch (@as(u8, 0)) { 1 => private.abs, else => {} };
const private = struct {
    extern "c" fn abs(value: c_int) c_int;
    extern "c" fn never_demanded_nested(value: @compileError("UNUSED_NESTED_EXTERN_SIGNATURE")) void;
};
pub extern "c" fn never_demanded(value: @compileError("UNUSED_EXTERN_SIGNATURE")) void;
pub fn emptyWrite() isize { return write(1, "", 0); }
