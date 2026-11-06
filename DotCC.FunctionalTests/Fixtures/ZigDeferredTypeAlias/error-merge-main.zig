const api = @import("error-merge-api.zig");
const E = api.API.Combined;
fn work() E!u8 { return error.Two; }
pub fn main() u8 {
    return work() catch |err| if (err == error.Two) 42 else 0;
}
