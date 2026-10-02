//type:fn
//options_all:--c++20 -tused -A

struct X {
  static void f();
  void f() const;  // error: redeclaration
  void g();
  void g() const;  // OK
};
