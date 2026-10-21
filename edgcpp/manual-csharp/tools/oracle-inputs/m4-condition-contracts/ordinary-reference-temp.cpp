constexpr int f(){const int& v=3;return v;} static_assert(f()==3);
