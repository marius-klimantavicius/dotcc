//options_all:-r -x -tused
//options: --strict;rp

struct A {
  void f1();
  inline void f2();
  static void f3();
  void f4();
  inline void f5();
  static void f6();
};
A a;
int main() {
  A a;
  a.f1();
  a.f2();
  a.f3();
}
inline void A::f1() { }          // Error  - already called
inline void A::f2() { }
inline void A::f3() { }          // Error -- already called
inline void A::f4() { }
inline void A::f5() { }
inline void A::f6() { }

