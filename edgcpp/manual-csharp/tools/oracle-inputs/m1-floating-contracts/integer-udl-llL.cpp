constexpr unsigned long long operator ""llL(unsigned long long){return 7;}
static_assert(1llL==7);
