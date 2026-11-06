const name = "res/" ++ "b\x79tes.bin";
pub const bytes = @embedFile(name);
pub const empty = @embedFile("res/empty.bin");
pub const bom = @embedFile("res/bom.bin");
