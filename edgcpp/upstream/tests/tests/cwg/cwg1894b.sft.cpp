//type:fn
//options_all:--c++20 -tused -A
    namespace A {
      struct S {};
    }
    namespace B {
      typedef int S;
    }
    namespace C {
      using namespace A;
      using namespace B;
      struct S s; // clearly ambiguous, S names different entities
    }
