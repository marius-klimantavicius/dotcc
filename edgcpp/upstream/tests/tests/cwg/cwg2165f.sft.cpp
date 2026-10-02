//type:fn
//options_all:--c++20 -tused -A
void f() {
  int x,y;
  void x();  // error: different entity for x
  int y;     // error: redefinition
}
enum { f };  // error: different entity for ::f
namespace A {}
namespace B = A;
namespace B {}    // error: different entity for B
