fn part(comptime tag: u8) []const u8 { return if (tag == 1) "idle,working" else "wrong"; }
pub const query_reply = "?:states=" ++ part(1);
pub const Terminator = enum {
    bell, st,
    pub fn string(self: Terminator) []const u8 {
        return switch (self) { .bell => "\x07", .st => "\x1b\\" };
    }
};
