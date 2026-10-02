//type:fn
//options_all:--c++14

void f2() {
  int i = 1;
  void g1(int = ([i]{ return i; })());       // ill-formed
  void g1a(int = ([&i]{ return i; })());       // ill-formed
  void g2(int = ([i]{ return 0; })());       // ill-formed
  void g2a(int = ([&i]{ return 0; })());       // ill-formed
  void g3(int = ([=]{ return i; })());       // ill-formed
  void g7(int = ([x=i] { return x; })());    // ill-formed
  void g7a(int = ([&x=i] { return x; })());    // ill-formed
}
