constexpr long double operator ""e(long double){return 7;}
constexpr long double operator ""e_tag(long double){return 9;}
constexpr long double operator ""E_tag(long double){return 11;}
static_assert(1.0e == 7 && 1.0e_tag == 9 && .1E_tag == 11);
