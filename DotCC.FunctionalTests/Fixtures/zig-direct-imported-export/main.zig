const api = @import("api.zig");

export fn zig_direct_anchor() u8 {
    return api.marker;
}
