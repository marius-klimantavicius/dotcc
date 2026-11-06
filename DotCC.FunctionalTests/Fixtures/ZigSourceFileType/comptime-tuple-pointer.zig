const Pair = struct { u8, u8 };
const Result = struct { pairs: []const Pair };
fn build() Result {
    comptime {
        var pairs = [_]Pair{ .{ 10, 1 }, .{ 20, 2 } };
        for (&pairs) |*pair| {
            pair[0] += 1;
            pair[1] = pair[0] + 1;
        }
        const final = pairs;
        return .{ .pairs = &final };
    }
}
const result = build();
pub fn main() u8 {
    if (result.pairs[0][0] != 11 or result.pairs[0][1] != 12) return 1;
    if (result.pairs[1][0] != 21 or result.pairs[1][1] != 22) return 2;
    var pair: Pair = .{ 20, 21 };
    const pointer = &pair;
    pointer[1] = pointer[0] + 2;
    return pointer[0] + pointer[1];
}
