constexpr int f(){int n=3;if(short x{n})return x;return 0;}static_assert(f()==3);
