constexpr float x=0x0.fffffeffffffp-126f;
constexpr double y=0x0.fffffffffffff7ffffp-1022;
static_assert(x/0x1p-149f == 8388608.0f, "floatunits");
static_assert(y/0x1p-1074 == 4503599627370496.0, "doubleunits");
