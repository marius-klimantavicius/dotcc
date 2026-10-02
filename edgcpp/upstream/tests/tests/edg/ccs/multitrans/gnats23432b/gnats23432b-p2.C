class X {
  template<typename T> X(T);
};

bool operator==(X, X);

template<int N> struct D {
  D() {
#ifndef HANG
    enum { e1 = N, e2 };
#else
    enum { e1, e2};
#endif
    if (e1 == e2) {}
  }
};

D<0> q;
D<1> r;
