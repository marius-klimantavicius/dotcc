var ticks: u8 = 0;
fn step() error{Failed}!u8 {
    ticks += 1;
    return 42;
}
pub fn main() u8 {
    const result = nosuspend step() catch 0;
    return if (ticks == 1) result else 0;
}
