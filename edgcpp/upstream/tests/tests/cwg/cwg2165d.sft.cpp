//type:fn
//options_all:--c++20 -tused -A

struct X {
  void g();
  void g() const;  // OK
  void g() &;      // error: redeclaration
};
