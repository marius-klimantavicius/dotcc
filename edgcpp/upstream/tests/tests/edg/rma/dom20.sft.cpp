//options_all:-r -x -tused
//options: --strict;cn

//    A   B
//     \ /
//      C
//     / \
//    D   E
//     \ /
//      F
struct A { int i, j; };
struct B { int i, j; };
struct C : A, B { };
struct CC : C { using C::i; };
struct D : virtual CC { int i, j; };
struct E : virtual CC { };
struct F : D, E { void f(); };
void F::f() {
  CC::i = 0;     // Error
  i = 0;
  j = 0;
  CC::j = 0;     // Error
}

