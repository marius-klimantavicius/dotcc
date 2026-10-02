//remark:Microsoft using declaration access checking
//options:--microsoft --c++17;fp:--c++17;fn

class B {
  template<typename T> static int f(T);
};

struct C: B {};

template<typename T> struct D: T {
  using T::f;
};

D<C> x;
