//type:fp
//options:--c++23
//options_all:-A

template<auto> struct C;
template<long long x> void f(C<x> *);
void g(C<0LL> *ap) {
  f(ap);     // OK, deduces long long value from 0LL
}

template<int> struct D;
template<auto x> void f(D<x> *);
void g(D<0LL> *ap) {
  f(ap);                        // OK, deduces x as an int value
}

template<const int &> struct F;
template<decltype(auto) x> void f(F<x> *);
int i;
void g(F<i> *ap) {
  f(ap);  // OK, deduces x as a constant template parameter of type const int &
}

template <decltype(auto) q> struct G;
template <auto x> long *f(G<x> *);            // #1
template <decltype(auto) x> short *f(G<x> *); // #2

const int j = 0;
short *g(G<(j)> *ap) {          // OK, q has type const int &
  return f(ap);                 // OK, only #2 matches
}

long *g(G<j> *ap) {             // OK, q has type int
  return f(ap);                 // OK, #1 is more specialized
}

//cwg: 2900
//title: Deduction of non-type template arguments with placeholder types
//meeting: Kona 11/25
//edg_status: EDGcpfe/28534
