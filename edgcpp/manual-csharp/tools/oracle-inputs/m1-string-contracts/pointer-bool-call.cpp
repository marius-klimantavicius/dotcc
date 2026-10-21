constexpr bool truth(bool value){return value;} static_assert(truth("x")); constexpr int pick(bool){return 1;} constexpr int pick(const char*){return 2;} static_assert(pick("x")==2);
