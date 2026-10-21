constexpr const char& r="ab"[3]; constexpr int ignore(const char&){return 1;} static_assert(ignore(r)==1);
