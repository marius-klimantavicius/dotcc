var calls: u8 = 0;
const Counter = struct {
    pub const bit_length = 21;
    pub fn amount() u8 { return bit_length; }
};
fn next() Counter { calls += 1; return .{}; }
pub fn main() u8 {
    const length = @TypeOf(next()).bit_length;
    const amount = @TypeOf(next()).amount();
    if (calls != 0) return 1;
    return length + amount;
}
