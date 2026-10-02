//type:fp
//options_all:--c++14

void f2() {
  int i = 1;
  void g4(int = ([=]{ return 0; })());       // OK
  void g5(int = ([]{ return sizeof i; })()); // OK
  void g6(int = ([x=1] { return x; })());    // OK
}
