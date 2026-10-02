//remark:GCC incomplete type casts
//option:--gnu=100200;fp:--gnu=100300;fn

struct I;
template<typename T> void f() {
  decltype(I(2)) *x;
  decltype(int(I())) z;
}

