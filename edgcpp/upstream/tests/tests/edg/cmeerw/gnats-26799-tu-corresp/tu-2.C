#include "incl.h"

namespace N1 {
  template<typename>
  int fn();

  struct A {
    template<typename>
    struct B {
      template<typename>
      friend int N1::fn();
    };
  };
}

namespace N2 {
  template<typename T>
  int fn(T);

  template<typename>
  struct B {
    template<typename T>
    friend int N2::fn(T);
  };
}
