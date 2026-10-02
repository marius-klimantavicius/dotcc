//options_all:-r -x -tused
//options: --strict;cp

class W { public : void f(); };
class A : private virtual W {};
class B : public virtual W {};
class C : public A, public B { void f(); };
void C::f() { W::f(); }

