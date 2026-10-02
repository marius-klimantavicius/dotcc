//type:fp
//options:--c++20

namespace pr
{
  template<typename U>
  struct A {
    template<typename T> requires (sizeof(U) > 1)  // spurious error
    static int f(T t);
    static int f(long);
  };
  int i = A<void>::f(0);
}
