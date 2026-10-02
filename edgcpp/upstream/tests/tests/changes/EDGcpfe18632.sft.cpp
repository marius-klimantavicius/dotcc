//type:fp
//options_all:--g++ --c++17
//remark:[5.0] Structured bindings in range-based-for iterator declarations
// 9/13/17  [EDGcpfe/18632,EDGcpfe/18694]
//
// Structured bindings in range-based-for iterator declarations
//
// The original implementation of structured bindings in version 4.14 omitted the
// case where that feature is used in range-based-for iterator declarations.
//
// This is now fixed.
struct A { int a, b; };
extern "C" int printf(char const*, ...);
int main() {
  A ar[] = { 1, 2, 3, 4, 5, 6 };
  for (auto [x, y] : ar) {       // Previously an error.  Now okay.
    printf("(%d, %d)\n", x, y);
  }
}
