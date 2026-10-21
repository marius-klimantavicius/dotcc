constexpr void noop() { return; }
constexpr int result() { noop(); return 1; }
static_assert(result() == 1);
