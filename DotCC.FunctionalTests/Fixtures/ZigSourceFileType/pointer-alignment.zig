fn alignment(pointer: anytype) usize {
    return @typeInfo(@TypeOf(pointer)).pointer.attrs.@"align" orelse 0;
}
fn typeAlignment(comptime P: type) usize {
    return @typeInfo(P).pointer.attrs.@"align" orelse 0;
}
fn dynamicAlignment(slice: []align(8) const u32, start: usize) usize {
    return alignment(slice[start..]);
}
const LocalAlignment = enum {
    one, eight,
    fn bytes(value: LocalAlignment) usize { return if (value == .eight) 8 else 1; }
};
fn chooseAlignment() LocalAlignment { return .eight; }
fn optionalAlignment(comptime override: ?LocalAlignment, pointer: *u64) usize {
    const chosen: LocalAlignment = override orelse comptime chooseAlignment();
    const aligned: *align(chosen.bytes()) u64 = @ptrCast(pointer);
    return alignment(aligned);
}
fn OptionalReader(comptime option: ?LocalAlignment) type {
    return struct {
        pub fn read(pointer: *u64) usize { return optionalAlignment(option, pointer); }
    };
}
fn Aligned(comptime amount: usize) type {
    return struct {
        const Ptr = *align(bytes) u32;
        pub const bytes: usize = amount;
        pub fn read(pointer: Ptr) usize {
            _ = pointer;
            return @typeInfo(Ptr).pointer.attrs.@"align" orelse 0;
        }
    };
}
pub fn main() u8 {
    var value: u64 = 42;
    const plain: *u32 = @ptrCast(&value);
    const one: *align(1) u32 = @ptrCast(&value);
    const four: *align(4) u32 = @ptrCast(&value);
    const eight: *align(8) u32 = @ptrCast(&value);
    if (optionalAlignment(null, &value) != 8 or optionalAlignment(.one, &value) != 1) return 11;
    if (OptionalReader(.one).read(&value) != 1) return 12;
    const many: [*]align(8) const u32 = @ptrCast(&value);
    const slice: []const u32 = many[0..1];
    const aligned_slice: []align(8) const u32 = many[0..2];
    if (alignment(slice) != 0 or alignment(aligned_slice) != 8 or alignment(aligned_slice.ptr) != 8) return 7;
    if (alignment(slice[1..]) != 0 or alignment(aligned_slice[0..]) != 8) return 8;
    if (alignment(aligned_slice[1..]) != 4 or alignment(aligned_slice[2..]) != 8) return 9;
    if (dynamicAlignment(aligned_slice, 1) != 4 or dynamicAlignment(aligned_slice, 0) != 4) return 10;
    if (@TypeOf(&value) != *u64 or @TypeOf(plain) != *u32) return 5;
    if (alignment(plain) != 0 or alignment(one) != 1) return 1;
    if (alignment(four) != 4 or alignment(eight) != 8) return 2;
    if (alignment(plain) != 0 or alignment(one) != 1) return 3;
    if (typeAlignment(*u32) != 0 or typeAlignment(*align(1) u32) != 1) return 4;
    const A = Aligned(1);
    const B = Aligned(8);
    if (A.read(one) != 1 or B.read(eight) != 8 or A.read(one) != 1) return 6;
    return @intCast(plain.*);
}
