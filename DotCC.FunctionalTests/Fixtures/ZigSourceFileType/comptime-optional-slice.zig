fn Reader(comptime valid: ?[]const u8) type {
    return struct {
        pub fn accepts(byte: u8) bool {
            if (valid) |characters| {
                for (characters) |character| {
                    if (character == byte) return true;
                }
                return false;
            }
            return true;
        }
        pub fn count() usize {
            return if (valid) |characters| characters.len else 999;
        }
        pub fn forward() usize { return countOptional(valid); }
    };
}
fn countOptional(comptime valid: ?[]const u8) usize {
    return if (valid) |characters| characters.len else 999;
}
fn present(comptime valid: ?[]const u8) usize {
    return if (valid) |characters| characters.len else @compileError("must be present");
}
pub fn main() u8 {
    if (!Reader("ab").accepts('a') or Reader("ab").accepts('z')) return 1;
    if (!Reader(null).accepts('z') or Reader("").accepts('z')) return 2;
    if (Reader("ab").count() != 2 or Reader("xyz").count() != 3) return 3;
    if (Reader(null).count() != 999 or Reader("").count() != 0) return 4;
    if (Reader("ab").forward() != 2 or Reader(null).forward() != 999) return 5;
    const typed: ?[]const u8 = "forty-two";
    if (Reader(typed).count() != 9 or countOptional(typed) != 9) return 6;
    if (countOptional(null) != 999 or countOptional("") != 0) return 7;
    if (present("abc") != 3) return 8;
    return 42;
}
