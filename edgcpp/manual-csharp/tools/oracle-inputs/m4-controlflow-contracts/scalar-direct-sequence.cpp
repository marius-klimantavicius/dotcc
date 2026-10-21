constexpr int f(){int i=0;return i+(i=1);} static_assert(f()==1);
