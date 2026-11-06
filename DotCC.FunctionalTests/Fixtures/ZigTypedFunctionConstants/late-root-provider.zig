const root = @import("root");
pub fn invoke(value: u8) u8 { return root.callback(2, value); }
