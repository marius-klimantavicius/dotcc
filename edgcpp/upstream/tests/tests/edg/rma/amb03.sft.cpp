//options_all:-r -x -tused
//options: --strict;cp

//           A{i,j}
//          / \
//         X   Y
//          \ /
//           B{i}
//           |
//           C
class A {public: int i,j; };
class X : virtual public A {};
class Y : virtual public A {};
class B : public X, public Y {public: float i; void f(); };
class C : public B { void f(); };
void B::f() {
  i = 0;       // B::i (float)
  j = 0;       // A::j (int)
}
void C::f() {
  i = 0;       // B::i (float)
  j = 0;       // A::j (int)
}

