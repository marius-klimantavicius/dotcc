pub fn Pool(comptime T: type) type {
    return struct {
        const required = @alignOf(T);
        ptr: *align(required) T,
    };
}
