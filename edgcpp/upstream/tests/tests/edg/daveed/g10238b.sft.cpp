//remark:GNU compatibility -- block-extern and namespace
//options:-A;ln:--g++ --gnu=40500;rp

  namespace N { struct S { void f(); }; }
  void N::S::f() {
    void g();  // ::g in g++ mode, N::g otherwise.
    g();
  }
  int main() {
  }
  void g() {}

