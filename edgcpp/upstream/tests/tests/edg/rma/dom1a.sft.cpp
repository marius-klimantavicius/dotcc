//options_all:-r -x -tused
//options: --strict;cp

class A { public: int i; };
class X : public virtual A { public: int i; };
class Y : public virtual A {};
class B : public X, public Y { void f(); };
void B::f() {
  i = 0;
}

