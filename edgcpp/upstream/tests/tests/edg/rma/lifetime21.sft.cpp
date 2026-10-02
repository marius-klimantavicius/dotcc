//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;cp

extern "C" int printf(const char *, ...);
struct A {
  A() { printf("A::A()\n"); }
  A(const A&) { printf("A::A(const A&)\n"); }
  ~A() { printf("A::~A\n"); }
} a;
struct B {
  B(int) { printf("B::B(int)\n"); }
  B() { printf("B::B()\n"); }
  ~B() { printf("B::~B()\n"); }
  operator A() { printf("B::operator A()\n"); return a; }
};
struct C : public A, public B {
  A aa;
  C() : aa(B(1)) // Creates a temporary that must be destroyed
        { A a1; A a2; printf("C::C()\n"); }
  ~C() { A a1; A a2; printf("C::~C\n"); }
} c;
int main () { return 0; }


