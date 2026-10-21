constexpr int id(int x){return x;} constexpr int f(){int i=0;return id(i=1)+i;} static_assert(f()==2);
