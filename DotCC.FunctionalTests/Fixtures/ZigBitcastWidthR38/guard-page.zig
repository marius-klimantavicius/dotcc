pub fn width_probe(source: []const u8) u24 { return @bitCast(source[0..3].*); }
