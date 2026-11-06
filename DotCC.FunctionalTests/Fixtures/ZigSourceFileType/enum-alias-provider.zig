pub const Level = enum(u16) {
    base = 62,
    other = 65,
    _,
    pub const alias = Level.base;
    pub const chained = Level.alias;
    pub const typed: Level = .other;
};
pub const Options = struct { level: Level = .chained };
pub fn get() Level { return .alias; }
