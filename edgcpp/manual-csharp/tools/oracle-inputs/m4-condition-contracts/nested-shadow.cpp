constexpr int f(){if(int x=1;false){}else{{int x=2;return x;}}return 0;} static_assert(f()==2);
