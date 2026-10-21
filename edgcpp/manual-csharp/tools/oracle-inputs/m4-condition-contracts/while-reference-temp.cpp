constexpr int f(){int n=1;while(const int& v=n+1){return v;}return 0;} static_assert(f()==2);
