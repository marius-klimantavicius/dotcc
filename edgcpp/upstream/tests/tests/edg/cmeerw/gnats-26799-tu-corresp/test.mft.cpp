//type:fp
//options_all:--c++11 --multi_trans_unit
//source_files:incl.h tu-2.C

#include "incl.h"

namespace N1 {
  struct A {
    template<typename>
    struct B {
      template<typename>
      friend int fn();
    };
  };
}
