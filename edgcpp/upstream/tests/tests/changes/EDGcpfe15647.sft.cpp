//type:fp
//options_all:--g++ --c++11
//remark:[4.10.1] Abort with constexpr base class cast on explicit temporary
// 2/14/15   [EDGcpfe/15647]
//
// Abort with constexpr base class cast on explicit temporary
//
// The front end previously aborted with a failed assertion (in fold_expr)
// when processing a base-class cast applied in a constexpr context to an
// explicit temporary.  This is now fixed.
struct A {
  constexpr operator int() {
    return 0;
  }
};

struct B : A { };

template <class T> void f() {
  constexpr int i = B();   // Previously aborted on cast from B to A
}

void g() {
  f<int>();
}
