//options_all:-r -x -tused
//options: --strict;cn:;rp

struct AA {
  AA(int);
  ~AA();
};
struct A {
  A(AA = 0, AA = 0);
};
A::A(AA,AA) { }
//struct B {
//  void f(AA=0) { }
//};
//static void f(AA=0);
//static void f(AA) { }
//static void g(AA);
//static void g(AA=0) { }
main() {
  A *p;
}

