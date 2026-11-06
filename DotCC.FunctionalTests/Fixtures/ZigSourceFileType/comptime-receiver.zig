const Option = enum {
    flag,
    number,

    fn Type(comptime key: Option) type {
        return switch (key) { .flag => bool, .number => u8 };
    }
    fn read(comptime key: Option, metadata: []const u8) key.Type() {
        return switch (key) { .flag => metadata[0] == 'y', .number => metadata[0] };
    }
    fn index(comptime key: Option) u8 {
        return switch (key) { .flag => 1, .number => 2 };
    }
    fn add(key: Option, comptime amount: u8) u8 {
        return (if (key == .flag) @as(u8, 1) else @as(u8, 2)) + amount;
    }
};
fn countedMetadata(calls: *u8, text: []const u8) []const u8 {
    calls.* += 1;
    return text;
}
pub fn main() u8 {
    var calls: u8 = 0;
    if (!Option.flag.read(countedMetadata(&calls, "yes"))) return 1;
    if (Option.number.read(countedMetadata(&calls, "*")) != 42) return 2;
    if (Option.flag.read(countedMetadata(&calls, "no"))) return 3;
    if (calls != 3) return 4;
    if (Option.flag.index() != 1 or Option.number.index() != 2) return 5;
    const flag = Option.flag;
    if (!flag.read("yes")) return 6;
    var runtime_key: Option = .number;
    runtime_key = if (calls == 3) .flag else .number;
    if (runtime_key.add(40) != 41) return 7;
    return Option.number.read("*");
}
