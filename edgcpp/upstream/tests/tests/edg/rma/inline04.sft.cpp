//options_all:-r -x -tused
//options: --strict;cn

class A {
 void f1();
 void f2();
 void f3();
};
static inline void A::f1() { }
static void A::f2() { }
extern inline void A::f3() { }

