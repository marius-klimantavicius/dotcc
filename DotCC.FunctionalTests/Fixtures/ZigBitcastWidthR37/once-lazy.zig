var calls: u8 = 0;
fn next() i24 { calls += 1; return -1; }
fn forbidden() i24 { calls += 100; return 0; }
fn choose(taken: bool) u24 {
    return if (taken) @bitCast(next()) else @bitCast(forbidden());
}
pub fn main() u8 {
    const first: u24 = @bitCast(next());
    const second = choose(first == 0xffffff);
    return if (first == second and calls == 2) 0 else 1;
}
