const Scalar = u32;
const seed: Scalar = 13;
fn initial() Scalar { return seed + 2; }
var number: Scalar = initial();
var file_size: usize = @sizeOf(@This());
var alias: *Scalar = &number;
pub const Reader = struct {
    field: u64,
    pub fn first() Scalar {
        if (file_size != 0) return 1;
        alias.* += 1;
        return number;
    }
};
pub fn read() Scalar { return number; }
pub fn pointer() **Scalar { return &alias; }
