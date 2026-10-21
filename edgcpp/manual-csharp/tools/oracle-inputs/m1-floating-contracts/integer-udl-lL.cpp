constexpr unsigned long long operator ""lL(unsigned long long){return 7;}
static_assert(1lL==7);
