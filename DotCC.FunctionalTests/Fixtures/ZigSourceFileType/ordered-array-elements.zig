const Parser = struct {
    state: u8 = 3,
    trace: u32 = 0,
    fn end(self: *Parser) ?u8 { self.trace = self.trace * 10 + 1; return self.state; }
    fn step(self: *Parser) ?u8 { self.trace = self.trace * 10 + 2; return self.state + 10; }
    fn enter(self: *Parser) void { self.trace = self.trace * 10 + 3; self.state = 99; }
    fn next(self: *Parser) [3]?u8 {
        const next_state: u8 = 42;
        defer self.state = next_state;
        return [3]?u8{
            if (self.state == next_state) null else if (self.end()) |value| value else null,
            self.step(),
            if (self.state == next_state) null else entry: {
                self.enter();
                break :entry self.state;
            },
        };
    }
};
pub fn main() u8 {
    var parser: Parser = .{};
    const first = parser.next();
    if (parser.trace != 123 or parser.state != 42) return 1;
    if (first[0].? != 3 or first[1].? != 13 or first[2].? != 99) return 2;
    parser.trace = 0;
    const second = parser.next();
    if (parser.trace != 2 or parser.state != 42) return 3;
    if (second[0] != null or second[1].? != 52 or second[2] != null) return 4;
    if (first[0].? != 3 or first[1].? != 13 or first[2].? != 99) return 5;
    return 42;
}
