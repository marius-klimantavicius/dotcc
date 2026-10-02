//options_all:-r -x -tused
//options: --strict;cp


struct A { int i; };          // should have internal linkage
static A a;
struct B { int i; };          // should have external linkage
static B b;
struct C { int i; };          // should have external linkage
static C *pc;
struct D { int i; };          // should have external linkage
static int D::* pmi = &D::i;
struct E { int i; };          // should have external linkage
typedef void (*pf)(E&);
static void x(E&) { }

//void f() {
//  try { }
//  catch (int) { }
//  catch (B) { }
//  catch (C*) { }
//  catch (int D::*) { }
//  catch (pf) { }
//}
void g(int i) {
  switch (i) {
    case 0:   throw a.i;  break;
    case 1:   throw b;    break;
    case 2:   throw pc;   break;
    case 3:   throw pmi;  break;
    default:  throw x;    break;
  }
}

