const payloads = @import("payloads.zig");
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    if (!@hasDecl(payloads.RGB, "cval")) return 1;
    if (!@hasDecl(payloads.Unknown, "cval") or !@hasDecl(payloads.Mode, "cval")) return 2;
    if (!@hasDecl(payloads.RGB, "unusedSignature")) return 3;
    if (@hasDecl(payloads.Plain, "cval") or @hasDecl(payloads.RGB, "absent")) return 4;
    if (payloads.conversions != 0 or payloads.rejected != 0) return 5;
    return 42;
}
