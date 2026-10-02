//type:fn
//options_all:--c++20 -tused -A
namespace N {
  template<class>
  struct A {
    struct B;
  };
}
using N::A;
template<class T> struct A<T>::B {};  // OK
template<> struct A<void> {};         // error: A not nominable in ::
