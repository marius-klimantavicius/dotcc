const Outer = struct {
    pub const Inner = struct {
        pub fn value() u8 { return 42; }
    };
};
pub const invoke = Outer.Inner.value;
