pub var conversions: u32 = 0;
pub var rejected: u32 = 0;

pub const RGB = struct {
    r: u8,
    g: u8,
    b: u8,

    pub const C = extern struct { blue: u32, red: u32, green: u32, error_code: u32 };

    pub fn cval(self: RGB) C {
        conversions += 1;
        return checked(self) catch |err| {
            rejected += 1;
            return .{ .blue = self.b, .red = self.r, .green = self.g,
                .error_code = if (err == error.Rejected) 71 else 72 };
        };
    }

    fn checked(self: RGB) error{Rejected}!C {
        if (self.r == 255) return error.Rejected;
        return .{ .blue = self.b + 3, .red = self.r + 1, .green = self.g + 2, .error_code = 0 };
    }

    pub fn unusedSignature() Signature() {
        unreachable;
    }
    fn Signature() type { @compileError("membership must not demand an unused signature"); }
};

pub const Unknown = struct {
    full: []const u16,
    partial: []const u16,
    pub const C = extern struct {
        full_ptr: [*]const u16,
        full_len: usize,
        partial_ptr: [*]const u16,
        partial_len: usize,
    };
    pub fn cval(self: Unknown) C {
        conversions += 1;
        return .{ .full_ptr = self.full.ptr, .full_len = self.full.len,
            .partial_ptr = self.partial.ptr, .partial_len = self.partial.len };
    }
};

pub const Mode = enum(u8) {
    off = 3,
    on = 9,
    pub const C = c_int;
    pub fn cval(self: Mode) C {
        conversions += 1;
        return @as(c_int, @intFromEnum(self)) + 30;
    }
};

pub const Plain = extern struct { value: u32 };
