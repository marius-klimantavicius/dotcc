const Inferred = packed union { byte: u3, signed: i3 };
const Unsigned = packed union(u16) { word: u16, signed: i16 };
const Signed = packed union(i8) { byte: u8, signed: i8 };
comptime { @export(&answer, .{ .name = "answer" }); }
pub fn answer() callconv(.c) u8 {
    const InferredBacking = @typeInfo(Inferred).@"union".backing_integer.?;
    if (InferredBacking != u3 or @typeInfo(InferredBacking).int.bits != 3) return 1;
    const UnsignedBacking = @typeInfo(Unsigned).@"union".backing_integer.?;
    if (UnsignedBacking != u16 or @bitSizeOf(UnsignedBacking) != 16) return 2;
    const SignedBacking = @typeInfo(Signed).@"union".backing_integer.?;
    if (SignedBacking != i8 or @typeInfo(SignedBacking).int.signedness != .signed) return 3;
    if (@typeInfo(Signed).@"union".backing_integer) |Backing| {
        if (Backing != i8) return 4;
    } else return 5;
    return 42;
}
