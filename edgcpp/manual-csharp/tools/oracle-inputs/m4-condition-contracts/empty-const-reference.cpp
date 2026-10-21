constexpr int f(){if(const int& v{})return 1;return 0;}static_assert(f()==0);
