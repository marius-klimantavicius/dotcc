const t = @import("types.zig");
export fn Flags8_bits() usize { return @bitSizeOf(t.Flags8); }
export fn Flags8_size() usize { return @sizeOf(t.Flags8); }
export fn Mixed8_bits() usize { return @bitSizeOf(t.Mixed8); }
export fn Mixed8_size() usize { return @sizeOf(t.Mixed8); }
export fn NonByte9_bits() usize { return @bitSizeOf(t.NonByte9); }
export fn NonByte9_size() usize { return @sizeOf(t.NonByte9); }
export fn Backed16_bits() usize { return @bitSizeOf(t.Backed16); }
export fn Backed16_size() usize { return @sizeOf(t.Backed16); }
export fn Nested16_bits() usize { return @bitSizeOf(t.Nested16); }
export fn Nested16_size() usize { return @sizeOf(t.Nested16); }
export fn Empty_bits() usize { return @bitSizeOf(t.Empty); }
export fn Empty_size() usize { return @sizeOf(t.Empty); }
