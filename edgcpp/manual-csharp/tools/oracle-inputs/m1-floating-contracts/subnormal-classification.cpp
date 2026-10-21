constexpr auto x=0x0.fffffeffffffp-126f;
constexpr auto y=0x0.fffffffffffff7ffffp-1022;
static_assert(x != 0.0f, "floatzero");
static_assert(x < 0x1p-126f, "floatnotbelow");
static_assert(x >= 0x0.fffffep-126f, "floatbelowmaxsub");
static_assert(y != 0.0, "doublezero");
static_assert(y < 0x1p-1022, "doublenotbelow");
static_assert(y >= 0x0.fffffffffffffp-1022, "doublebelowmaxsub");
