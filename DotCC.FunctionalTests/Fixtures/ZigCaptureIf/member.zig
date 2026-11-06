const Capture = struct {
    value: u32,
    fn trailing(self: *Capture) void { self.value += 2; }
};
const Parser = struct { capture: ?Capture, state: u32 };
var calls: u32 = 0;
var cleanups: u32 = 0;
fn getBox(parser: *Parser) *Parser { calls += 1; return parser; }
fn parse(parser: *Parser) ?u32 {
    defer cleanups += 1;
    const cap = if (getBox(parser).capture) |*c| c else {
        defer parser.state += 3;
        parser.state = 4;
        return null;
    };
    cap.trailing();
    return cap.value;
}
fn direct(parser: *Parser) *Capture {
    return if (parser.capture) |*c| c else unreachable;
}
fn statement(parser: *Parser) void {
    if (getBox(parser).capture) |*c| { c.value += 1; }
}
pub fn main() u8 {
    var parser: Parser = .{ .capture = .{ .value = 40 }, .state = 0 };
    if (parse(&parser).? != 42 or parser.capture.?.value != 42) return 1;
    if (calls != 1 or cleanups != 1 or parser.state != 0) return 2;
    direct(&parser).value += 1;
    statement(&parser);
    if (parser.capture.?.value != 44 or calls != 2) return 5;
    calls = 1;
    parser.capture = null;
    if (parse(&parser) != null) return 3;
    if (calls != 2 or cleanups != 2 or parser.state != 7) return 4;
    return 42;
}
