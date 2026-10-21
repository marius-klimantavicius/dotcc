constexpr unsigned long long operator ""e_tag(unsigned long long){return 7;}
constexpr unsigned long long operator ""Efoo(unsigned long long){return 9;}
static_assert(1e_tag==7 && 1Efoo==9);
