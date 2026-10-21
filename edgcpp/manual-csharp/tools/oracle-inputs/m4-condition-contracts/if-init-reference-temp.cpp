constexpr int f(){if(const int& v=3;true)return v;return 0;} static_assert(f()==3);
