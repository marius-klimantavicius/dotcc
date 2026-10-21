constexpr int f(){if(const int& v=3)return v;return 0;} static_assert(f()==3);
