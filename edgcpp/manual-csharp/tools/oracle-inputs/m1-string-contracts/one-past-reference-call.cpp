constexpr const char& past(){return "ab"[3];} constexpr int ignore(const char&){return 1;} static_assert(ignore(past())==1);
