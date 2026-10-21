constexpr const char* pass(const char* p,int){return p;} constexpr int f(){int i=0;const char* p=pass("abc",i=1)+i;return p[0];} static_assert(f()==98);
