//type:fp
//options_all:--c++20 -tused -A
    namespace A {
      struct S {};
    }
    namespace B {
      typedef int S;
    }
    namespace D {
      using A::S;
      typedef struct S S;
      struct S s; // OK under DR407: S could be used in an elaborated-type-specifier before the typedef, so still can be
    }
    namespace E {
      typedef A::S S;
      using A::S;
      struct S s; // valid code (efh) ??? the identifier S could not have been used in an elaborated-type-specifier prior to the typedef, so is this lookup ill-formed because it finds a typedef-name?
    }
    namespace F {
      typedef A::S S;
    }
    namespace G {
      using namespace A;
      using namespace F;
      struct S s; // valid (efh) ??? F::S could not have been used as an elaborated-type-specifier before the typedef. is this ill-formed because the lookup finds a typedef-name?
    }
    namespace H {
      using namespace F;
      using namespace A;
      struct S s; // some implementations give different answers for G and H
    }

//cwg: 1894
//title: typedef-names and using-declarations
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23829
