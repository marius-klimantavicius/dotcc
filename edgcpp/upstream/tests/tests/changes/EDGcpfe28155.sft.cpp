//type:fp
//options_all:--c++20
//remark:[6.8] C++-generating back end: unbounded recursion with friend function declaration
// 5/22/25  [EDGcpfe/28155]
//
// C++-generating back end: unbounded recursion with friend function declaration
//
// In some complex cases in which a friend function is declared with an
// in-scope private type in one of its parameters, the C++-generating back
// end could enter an unbounded recursion attempting to put out that function
// parameter.  This is now fixed.
template<typename T> struct A {
  using type = T;
};
template<typename T> using B = A<T>::type;
template<typename> using C = int;
template<typename T> class D {
  using E = C<B<T>>;
};
class F {
  class G {
    friend F f(G);   // G is private
  };
public:
  using H = G;
  using I = D<H>;
  D<G> g() {}
};
