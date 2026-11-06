const Kind = enum {
    first,
    second,
    pub const max_response_bytes = 256;
    pub const alias: @This() = .second;
    pub const @"limit-bytes": usize = 3;
    pub const limit_bytes: usize = 4;
};
fn size(comptime E: type) usize { return E.max_response_bytes; }
pub fn main() u8 {
    var bytes: [Kind.max_response_bytes]u8 = undefined;
    bytes[255] = 42;
    if (bytes.len != 256 or size(Kind) != 256) return 1;
    if (@field(Kind, "max_response_bytes") != 256 or Kind.@"limit-bytes" != 3) return 2;
    if (Kind.alias != .second or @field(Kind, "alias") != .second) return 3;
    if (@typeInfo(Kind).@"enum".field_names.len != 2) return 4;
    if (Kind.limit_bytes != 4 or @field(Kind, "limit-bytes") != 3) return 5;
    return bytes[255];
}
