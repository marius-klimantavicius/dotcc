//options_all:-r -x -tused
//options: --strict;cn:;cp

//   B   A   C
//    \ / \ /
//     X   Y
//      \ /
//       D
class A { public: int i, j, k; A() : i(1), j(0) {} };
class B { public: int i, j; };
class C { public: int j, k; };
class X : virtual public A, public B { public: int i; };
class Y : public C, virtual public A { public: int k; };
class D : public X, public Y { void f(); };
main () {
  D d;
}

