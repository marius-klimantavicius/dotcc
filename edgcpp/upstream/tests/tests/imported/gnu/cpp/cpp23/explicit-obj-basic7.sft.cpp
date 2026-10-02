//type: fp
//options: --c++23
// { dg-do compile { target c++23 } }

// Shouldn't ICE
struct S {
  void a(this long);
  void b(this const long);
  void c(this long unsigned);
  void c(this signed);
};
