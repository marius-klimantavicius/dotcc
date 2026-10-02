//options_all:-r -x -tused
//options: --strict;cp

class A { int a; };
class B : public A { int b; };
class C : public virtual B { int c; };
class D : public C { int d; };
class E : public C { int e; };
class F : public D, public E { int f; };
class G : public virtual F { int g; };
G g;

/*
struct A {
  int a__1A ;
};
struct B {
  int a__1A ;
  int b__1B ;
};
struct C {
  int c__1C ;
  struct B *PB;
  struct B OB;
};
struct D {
  int c__1C ;
  struct B *PB;
  int d__1D ;
  struct B OB;
};
struct E {
  int c__1C ;
  struct B *PB;
  int e__1E ;
  struct B OB;
};
struct F {
  int c__1C ;
  struct B *PB;
  int d__1D ;
  struct E OE;
  int f__1F ;
};
struct G {
  int g__1G ;
  struct F *PF;
  struct B *PB;
  struct F OF;
};
*/

