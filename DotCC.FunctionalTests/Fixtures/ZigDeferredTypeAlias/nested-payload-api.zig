pub const Command = union(enum) {
    other: Unknown,
    pub const Unknown = struct { value: u32 };
};
