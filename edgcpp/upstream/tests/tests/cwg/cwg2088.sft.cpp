//type:fp
//options_all:--c++20 -tused -A 
template <typename T> struct A {
    struct typeA { };
    struct typeB { };
    using convTyA = T (*const &&)(typename A<T>::typeA);
    using convTyB = T (*const &)(typename A<T>::typeB);
    operator convTyA();
    operator convTyB();
  };

  template <typename T> void foo(T (*const &&)(typename A<T>::typeA));
  template <typename T> int foo(T (*const &)(typename A<T>::typeB));

  int main() {
    return foo<int>(A<int>());
  }

//cwg: 2088
//title: Late tiebreakers in partial ordering
//meeting: Jacksonville 2/18
//edg_status: EDGcpfe/21892
