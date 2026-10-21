constexpr unsigned long long operator ""p_tag(unsigned long long){return 7;}
constexpr unsigned long long operator ""Pfoo(unsigned long long){return 9;}
static_assert(0x1p_tag==7 && 0x1Pfoo==9);
