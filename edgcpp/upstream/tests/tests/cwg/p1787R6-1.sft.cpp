//type:fn
//options_all:--c++20 -tused -A
namespace N {
  inline namespace O {
    template<class T> void f(T);   // #1
    template<class T> void g(T) {}
  }
  namespace P {
    template<class T> void f(T*);  // #2, more specialized than #1
    template<class> int g;
  }
  using P::f,P::g;
}
template<> void N::f(int*) {}      // OK: #2 is not nominable
template void N::g(int);           // error: lookup is ambiguous
