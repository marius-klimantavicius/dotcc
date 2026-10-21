constexpr int ignore(const char&){return 1;} static_assert(ignore("ab"[3])==1);
