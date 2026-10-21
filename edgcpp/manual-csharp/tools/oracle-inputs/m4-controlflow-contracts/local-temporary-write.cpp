constexpr int f() { int&& local = 1; local = 2; return local; } static_assert(f() == 2);
