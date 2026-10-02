//options_all:-r -x -tused
//options: --strict;cn

// "Dominance" -- copied from Working Paper 10.1.1
//
//     W   V   W
//      \ / \ /
//       B   C
//        \ /
//         D
//
class V { public: int f(); int x; };
class W { public: int g(); int y; };
class B : public virtual V/*, public W*/ {
public:
  int f(); int x;
  int g(); int y;
};
class C : public virtual V, public W { };
class D : public B, public C { void ff(); };
void D::ff()
{
  x++;                // okay: B::x hides V::x
  f();                // okay: B::f() hides V::f()
  y++;                // error: B::y and C's W::y
  g();                // error: B::g() and C's W::g()
}

