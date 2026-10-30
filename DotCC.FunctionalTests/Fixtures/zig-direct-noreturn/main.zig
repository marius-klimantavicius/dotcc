fn stop() noreturn {
    while (true) {}
}

fn choose(ok: bool) u8 {
    return if (ok) 21 else stop();
}

fn classify(value: u8) u8 {
    return switch (value) {
        1 => 21,
        else => stop(),
    };
}

pub fn main() u8 {
    return choose(true) + classify(1);
}
