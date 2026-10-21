constexpr int f(){int total=0;for(int n=3;int v=n;n=v-1) total+=v;return total;} static_assert(f()==6);
