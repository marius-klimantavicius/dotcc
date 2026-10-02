//type:fp
//options_all:--c++20 -tused -A
void f() {
  int x,y;
}
namespace A {}
namespace B = A;
namespace B = A;  // OK: no effect
namespace B = B;  // OK: no effect
namespace A = B;  // OK: no effect
